using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Template.HelpingClasses;
using Template.Models;
using Template.Repositories;

namespace Template.Controllers
{
    public class PreperationController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IPreperationRepo _preperationRepo;
        private readonly GeneralPurpose gp;

        public PreperationController(IUserRepo userRepo, IPreperationRepo preperationRepo, IHttpContextAccessor haccess)
        {
            _userRepo = userRepo;
            _preperationRepo = preperationRepo;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<IActionResult> Index(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetPreperationDataTableList(string Name = "")
        {
            var getUserId = gp.GetUserClaims();
            var ulist = new List<Preperation>();

            if (getUserId.Role != 1)
            {
                ulist = (List<Preperation>)await _preperationRepo.GetActivePreperationList(Convert.ToInt32(getUserId.Id));
            }
            else
            {
                ulist = (List<Preperation>)await _preperationRepo.GetActivePreperationList();
            }

            if (!String.IsNullOrEmpty(Name))
            {
                ulist = ulist.Where(x => x.Name.ToLower().Contains(Name.Trim().ToLower())).ToList();
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
                ulist = ulist.Where(x => x.Name != null && x.Name.ToLower().Contains(searchValue.ToLower())
                                        ).ToList();
            }

            int totalrowsafterfilterinig = ulist.Count();


            // pagination
            ulist = ulist.Skip(start).Take(length).ToList();

            List<PreperationDto> udto = new List<PreperationDto>();

            foreach (Preperation u in ulist)
            {
                var userName = "";
                var createdby = await _userRepo.GetUserById((int)u.CreatedBy);
                if (Convert.ToInt32(getUserId.Id) == u.CreatedBy)
                {
                    userName = "You";
                }
                else
                {
                    userName = createdby.Name;
                }
                PreperationDto obj = new PreperationDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                    CreatedBy = createdby != null ? userName : "",
                };

                udto.Add(obj);
            }

            return Json(new { data = udto, draw = Request.Form["draw"].FirstOrDefault(), recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig });
        }

        [HttpPost]
        public async Task<IActionResult> PostAddPreperation(Preperation _Preperation)
        {
            if (string.IsNullOrEmpty(_Preperation.Name))
            {
                return RedirectToAction("Index", new { msg = "All Fields are Required", color = "green" });
            }
            var getUserId = gp.GetUserClaims();
            Preperation Preperation = new Preperation
            {
                Name = _Preperation.Name.Trim(),
                IsActive = 1,
                CreatedAt = GeneralPurpose.DateTimeNow(),
                CreatedBy = Convert.ToInt32(getUserId.Id)
            };
            if (!await _preperationRepo.AddPreperation(Preperation))
            {
                return RedirectToAction("Index", new { msg = "Somethings' Wrong", color = "red" });
            }

            return RedirectToAction("Index", new { msg = "Record Inserted Successfully", color = "green" });
        }

        [HttpPost]
        public async Task<IActionResult> PostUpdatePreperation(Preperation _Preperation)
        {
            Preperation? Preperation = await _preperationRepo.GetPreperationById(_Preperation.Id);
            if (Preperation == null)
            {
                return RedirectToAction("Index", new { msg = "Record not found", color = "red" });
            }
            Preperation.Name = _Preperation.Name.Trim();
            Preperation.UpdatedAt = GeneralPurpose.DateTimeNow();

            if (await _preperationRepo.UpdatePreperation(Preperation))
            {
                return RedirectToAction("Index", new { msg = "Preperation updated successfully", color = "green" });
            }
            return RedirectToAction("Index", new { msg = "Somethings' wrong", color = "red" });
        }

        [HttpPost]
        public async Task<IActionResult> GetPreperationById(string id)
        {
            int PreperationId = StringCipher.DecryptId(id);
            Preperation? u = await _preperationRepo.GetPreperationById(PreperationId);
            if (u == null)
            {
                return Json(0);
            }

            PreperationDto obj = new PreperationDto()
            {
                Id = u.Id.ToString(),
                EncId = StringCipher.EncryptId(u.Id),
                Name = u.Name,
            };

            return Json(obj);
        }

        [HttpPost]
        public async Task<IActionResult> GetPreperationList()
        {
            var getUserId = gp.GetUserClaims();

            var list = new List<Preperation>();
            if (getUserId.Role == 1)
            {
                list = (List<Preperation>)await _preperationRepo.GetActivePreperationList();
            }
            else
            {
                list = (List<Preperation>)await _preperationRepo.GetActivePreperationList(Convert.ToInt32(getUserId.Id));
            }

            List<PreperationDto> udto = new List<PreperationDto>();

            foreach (Preperation u in list)
            {
                PreperationDto obj = new PreperationDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                };

                udto.Add(obj);
            }

            return Json(udto);
        }

        public async Task<IActionResult> DeletePreperation(string id)
        {
            int Id = StringCipher.DecryptId(id);

            if (!await _preperationRepo.DeletePreperation(Id))
            {
                return RedirectToAction("Index", new { msg = "Somethings' wrong", color = "red" });
            }

            return RedirectToAction("Index", new { msg = "Record deleted successfully!", color = "green" });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateName(string name, string id = "")
        {
            if (!string.IsNullOrEmpty(name))
            {
                return Json(await _preperationRepo.ValidateName(name, Convert.ToInt32(id)));
            }
            else
            {
                return Json("Name cannot be Empty");
            }
        }
    }
}
