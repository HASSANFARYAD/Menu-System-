using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Template.HelpingClasses;
using Template.Models;
using Template.Repositories;

namespace Template.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IMenuCategoryRepo _menuCategoryRepo;
        private readonly IMenuRepo _menuRepo;
        private readonly GeneralPurpose gp;

        public CategoryController(IUserRepo userRepo, IMenuRepo menuRepo, IMenuCategoryRepo menuCategoryRepo, IHttpContextAccessor haccess)
        {
            _userRepo = userRepo;
            _menuCategoryRepo = menuCategoryRepo;
            _menuRepo = menuRepo;
            gp = new GeneralPurpose(haccess);
        }

        public IActionResult Index(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCategoryDataTableList(string Name = "")
        {
            var getUserId = gp.GetUserClaims();
            var ulist = new List<MenuCategory>();
            if (getUserId.Role == 1)
            {
                ulist = (List<MenuCategory>)await _menuCategoryRepo.GetActiveMenuCategoryList();
            }
            else
            {
                ulist = (List<MenuCategory>)await _menuCategoryRepo.GetActiveMenuCategoryList(Convert.ToInt32(getUserId.Id));
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

            List<MenuCategoryDto> udto = new List<MenuCategoryDto>();

            foreach (MenuCategory u in ulist)
            {
                var userName = "";
                var createdby = await _userRepo.GetUserById((int)u.CreatedBy);
                if (createdby != null)
                {
                    if (Convert.ToInt32(getUserId.Id) == u.CreatedBy)
                    {
                        userName = "You";
                    }
                    else
                    {
                        userName = createdby.Name;
                    }
                }
                else
                {
                    userName = "user Deleted";
                }
                MenuCategoryDto obj = new MenuCategoryDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                    ProfilePath = u.FilePath,
                    CreatedBy = userName,
                };

                udto.Add(obj);
            }

            return Json(new { data = udto, draw = Request.Form["draw"].FirstOrDefault(), recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig });
        }

        [HttpPost]
        public async Task<IActionResult> PostAddCategory(AddMenuDto _category)
        {
            if (string.IsNullOrEmpty(_category.Name))
            {
                return RedirectToAction("Index", new { msg = "All Fields are Required", color = "green" });
            }
            var getUserId = gp.GetUserClaims();
            MenuCategory menuCategory = new MenuCategory
            {
                Name = _category.Name.Trim(),
                IsActive = 1,
                CreatedAt = GeneralPurpose.DateTimeNow(),
                CreatedBy = Convert.ToInt32(getUserId.Id)
            };
            if (_category.Picture != null)
            {
                if (_category.Picture.Length != 0)
                {
                    string FileExt = System.IO.Path.GetExtension(_category.Picture.FileName);
                    if (FileExt.ToLower().Equals(".jpg") || FileExt.ToLower().Equals(".png") || FileExt.ToLower().Equals(".jpeg"))
                    {
                        string updatedProfile = await GeneralPurpose.UploadProfilePicture(_category.Picture, _category.Photo, "category", getUserId.Id);
                        menuCategory.FilePath = string.IsNullOrEmpty(updatedProfile) ? "" : updatedProfile;
                    }
                    else
                    {
                        return RedirectToAction("AddMenu", new { msg = "File must be type of jpg/png/jpeg", color = "red" });
                    }
                }
                else
                {
                    return RedirectToAction("AddMenu", new { msg = "Corrupt file. Select other one", color = "red" });
                }
            }
            if (!await _menuCategoryRepo.AddMenuCategory(menuCategory))
            {
                return RedirectToAction("Index", new { msg = "Somethings' Wrong", color = "red" });
            }

            return RedirectToAction("Index", new { msg = "Record Inserted Successfully", color = "green" });
        }

        [HttpPost]
        public async Task<IActionResult> PostUpdateMenuCategory(AddMenuCategoryDto _category)
        {
            MenuCategory? menuCategory = await _menuCategoryRepo.GetMenuCategoryById((int)_category.Id);
            if (menuCategory == null)
            {
                return RedirectToAction("Index", new { msg = "Record not found", color = "red" });
            }
            menuCategory.Name = _category.Name.Trim();
            menuCategory.UpdatedAt = GeneralPurpose.DateTimeNow();

            if (_category.Picture != null)
            {
                if (_category.Picture.Length != 0)
                {
                    string FileExt = System.IO.Path.GetExtension(_category.Picture.FileName);
                    if (FileExt.ToLower().Equals(".jpg") || FileExt.ToLower().Equals(".png") || FileExt.ToLower().Equals(".jpeg"))
                    {
                        string updatedProfile = await GeneralPurpose.UploadProfilePicture(_category.Picture, menuCategory.FilePath, "category", menuCategory.CreatedBy.ToString());
                        menuCategory.FilePath = string.IsNullOrEmpty(updatedProfile) ? menuCategory.FilePath : updatedProfile;
                    }
                    else
                    {
                        return RedirectToAction("AddMenu", new { msg = "File must be type of jpg/png/jpeg", color = "red" });
                    }
                }
                else
                {
                    return RedirectToAction("AddMenu", new { msg = "Corrupt file. Select other one", color = "red" });
                }
            }

            if (await _menuCategoryRepo.UpdateMenuCategory(menuCategory))
            {
                return RedirectToAction("Index", new { msg = "MenuCategory updated successfully", color = "green" });
            }
            return RedirectToAction("Index", new { msg = "Somethings' wrong", color = "red" });
        }

        [HttpPost]
        public async Task<IActionResult> GetMenuCategoryById(string id)
        {
            int menuCategoryId = StringCipher.DecryptId(id);
            MenuCategory? u = await _menuCategoryRepo.GetMenuCategoryById(menuCategoryId);
            if (u == null)
            {
                return Json(0);
            }

            MenuCategoryDto obj = new MenuCategoryDto()
            {
                Id = u.Id.ToString(),
                Name = u.Name,
                ProfilePath = u.FilePath,
            };

            return Json(obj);
        }

        [HttpPost]
        public async Task<IActionResult> GetMenuCategoryList()
        {
            var getUserId = gp.GetUserClaims();

            var list = new List<MenuCategory>();
            if (getUserId.Role == 1)
            {
                list = (List<MenuCategory>)await _menuCategoryRepo.GetActiveMenuCategoryList();
            }
            else
            {
                list = (List<MenuCategory>)await _menuCategoryRepo.GetActiveMenuCategoryList(Convert.ToInt32(getUserId.Id));
            }

            List<MenuCategoryDto> udto = new List<MenuCategoryDto>();

            foreach (MenuCategory u in list)
            {
                MenuCategoryDto obj = new MenuCategoryDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                };

                udto.Add(obj);
            }

            return Json(udto);
        }

        public async Task<IActionResult> DeleteMenuCategory(string id)
        {
            int Id = StringCipher.DecryptId(id);

            if (!await _menuCategoryRepo.DeleteMenuCategory(Id))
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
                return Json(await _menuCategoryRepo.ValidateName(name, Convert.ToInt32(id)));
            }
            else
            {
                return Json("Name cannot be Empty");
            }
        }
    }
}
