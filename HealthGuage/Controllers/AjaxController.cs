using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using HealthGuage.Filters;
using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using LoginFinal.Filters;
using Microsoft.AspNetCore.Mvc;
using RestSharp.Extensions;
using System;
using System.Linq;
using System.Xml.Linq;
using Template.HelpingClasses;
using Template.Models;
using Template.Repositories;

namespace HealthGuage.Controllers
{
    [ValidationFilter(Roles = new int[] { 1, 2 })]
    public class AjaxController : Controller
    {
        private readonly IUserRepo userRepo;
        private readonly GeneralPurpose gp;
        private readonly IContentFileRepo contentFileRepo;

        public AjaxController(IUserRepo _userRepo, IHttpContextAccessor haccess, IContentFileRepo _contentFileRepo)
        {
            userRepo = _userRepo;
            gp = new GeneralPurpose(haccess);
            contentFileRepo = _contentFileRepo;
        }


        private async Task<bool> IsUserValidate()
        {
            int userId = Convert.ToInt32(gp.GetUserClaims().Id);
            if (userId != null)
            {
                bool isAutheticate = await userRepo.IsUserValidate(userId);
                if (!isAutheticate)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        #region User

        [HttpPost]
        public async Task<IActionResult> GetUserDataTableList(string Name = "", string email = "")
        {
            var ulist = await userRepo.GetActiveUserList();
            ulist = ulist.Where(x => x.Role == 2).ToList();

            if (!String.IsNullOrEmpty(Name))
            {
                ulist = ulist.Where(x => x.Name.ToLower().Contains(Name.Trim().ToLower())).ToList();
            }

            if (!String.IsNullOrEmpty(email))
            {
                ulist = ulist.Where(x => x.Email.ToLower().Contains(email.Trim().ToLower())).ToList();
            }

            int start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            int length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
            string searchValue = Request.Form["search[value]"].FirstOrDefault();
            string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"];
            string sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            if (sortColumnName != "" && sortColumnName != null)
            {
                if (sortColumnName != "0")
                {
                    if (sortDirection == "asc")
                    {
                        ulist = ulist.OrderByDescending(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                    else
                    {
                        ulist = ulist.OrderBy(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                }
            }

            int totalrows = ulist.Count();

            //filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                ulist = ulist.Where(x => x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(searchValue.ToLower()) 
                                        || x.Name != null && x.Name.ToLower().Contains(searchValue.ToLower())
                                        || x.Email != null && x.Email.ToLower().Contains(searchValue.ToLower())
                                        || x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(searchValue.ToLower())
                                    ).ToList();
            }

            int totalrowsafterfilterinig = ulist.Count();


            // pagination
            ulist = ulist.Skip(start).Take(length).ToList();

            List<UserDto> udto = new List<UserDto>();

            foreach (User u in ulist)
            {
                UserDto obj = new UserDto()
                {
                    Id = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                    Email = u.Email,
                    Contact = u.PhoneNumber,
                    Profile = u.ProfilePicture,
                };

                udto.Add(obj);
            }

            return Json(new { data = udto, draw = Request.Form["draw"].FirstOrDefault(), recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig });
        }

        [HttpPost]
        public async Task<IActionResult> GetUserById(string id)
        {
            int userId = StringCipher.DecryptId(id);
            User? u = await userRepo.GetUserById(userId);
            if (u == null)
            {
                return Json(0);
            }

            UserDto obj = new UserDto()
            {
                Id = u.Id.ToString(),
                Name = u.Name,
                Email = u.Email,
                Contact = u.PhoneNumber,
            };

            return Json(obj);
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> ValidateEmail(string email, string id  = "")
        {
            //int chk = -1;
            //if (!string.IsNullOrEmpty(id) && id != "-1")
            //{
            //    chk = StringCipher.DecryptId(id);
            //}
            return Json(await userRepo.ValidateEmail(email, Convert.ToInt32(id)));
        }

        #region content file region

        [HttpDelete]
        public async Task<IActionResult> DeleteContentFile(int ContentFileId)
        {
            var isDeleted = await contentFileRepo.DeleteContentFile(ContentFileId);
            
            if (isDeleted)
            {
                return Json(true);
            }
            return Json(false);
        }

        #endregion

    }
}
