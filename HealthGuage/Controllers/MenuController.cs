using DocumentFormat.OpenXml.Spreadsheet;
using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Template.HelpingClasses;
using Template.Models;
using Template.Repositories;

namespace Template.Controllers
{
    public class MenuController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IIngredientRepo _ingredientRepo;
        private readonly IMenuRepo _menuRepo;
        private readonly IMenuIngredientRepo _menuIngredientRepo;
        private readonly GeneralPurpose gp;

        public MenuController(IUserRepo userRepo, IIngredientRepo ingredientRepo, IMenuRepo menuRepo,
            IMenuIngredientRepo menuIngredientRepo, IHttpContextAccessor haccess)
        {
            _userRepo = userRepo;
            _ingredientRepo = ingredientRepo;
            _menuRepo = menuRepo;
            _menuIngredientRepo = menuIngredientRepo;
            gp = new GeneralPurpose(haccess);
        }

        public IActionResult Index(string msg = "", string color = "")
        {
            ViewBag.message = msg;
            ViewBag.color = color;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetList(string Name = "", string Date = "", string Cooking = "",
            string Weight = "")
        {
            var getUserId = gp.GetUserClaims();
            var list = new List<Menu>();

            if (getUserId.Role == 1)
            {
                list = (List<Menu>)await _menuRepo.GetActiveMenuList();
            }
            else
            {
                list = (List<Menu>)await _menuRepo.GetActiveMenuList(Convert.ToInt32(getUserId.Id));
            }

            if (!String.IsNullOrEmpty(Name))
            {
                list = list.Where(x => x.Name.ToLower().Contains(Name.Trim().ToLower())).ToList();
            }

            if (!String.IsNullOrEmpty(Date))
            {
                list = list.Where(x => Convert.ToDateTime(x.Date).Date >= Convert.ToDateTime(Date).Date).ToList();
            }

            if (!String.IsNullOrEmpty(Cooking))
            {
                list = list.Where(x => x.Cooking.ToLower().Contains(Cooking.Trim().ToLower())).ToList();
            }

            if (!String.IsNullOrEmpty(Weight))
            {
                list = list.Where(x => x.Weight.ToLower().Contains(Weight.Trim().ToLower())).ToList();
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
                        list = list.OrderByDescending(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                    else
                    {
                        list = list.OrderBy(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                }
            }

            int totalrows = list.Count();

            //filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                list = list.Where(x => x.Name != null && x.Name.ToLower().Contains(searchValue.ToLower())
                                        || x.Cooking != null && x.Cooking.ToLower().Contains(searchValue.ToLower())
                                        || x.Weight != null && x.Weight.ToLower().Contains(searchValue.ToLower())
                                    ).ToList();
            }

            int totalrowsafterfilterinig = list.Count();


            // pagination
            list = list.Skip(start).Take(length).ToList();

            List<MenuDto> udto = new List<MenuDto>();

            foreach (Menu u in list)
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

                MenuDto obj = new MenuDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                    Date = u.Date != null ? u.Date.Value.ToString("MM/dd/yyyy") : null,
                    Photo = u.Photo,
                    Cooking = u.Cooking,
                    Weight = u.Weight,
                    Link = u.Link,
                    PreperationForBeaorStaff = u.PreperationForBeaorStaff,
                    CreatedBy = userName
                };

                udto.Add(obj);
            }

            return Json(new { data = udto, draw = Request.Form["draw"].FirstOrDefault(), recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig });
        }

        public IActionResult AddMenu(string msg = "", string color = "")
        {
            var getUserId = gp.GetUserClaims();
            if(getUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            ViewBag.message = msg;
            ViewBag.color = color;
            return View();
        }

        public async Task<IActionResult> PostAddMenu(Menu menu, IFormFile picture)
        {
            var getUserId = gp.GetUserClaims();
            var getImage = Request.Form["picture"];
            Menu menu1 = new Menu()
            {
                Name = menu.Name,
                Date = menu.Date,
                Cooking = menu.Cooking,
                Weight = menu.Weight,
                Link = menu.Link,
                PreperationForBeaorStaff = menu.PreperationForBeaorStaff,
                IsActive = 1,
                CreatedBy = Convert.ToInt32(getUserId.Id),
                CreatedAt = GeneralPurpose.DateTimeNow(),
            };

            if (picture != null)
            {
                if (picture.Length != 0)
                {
                    string FileExt = System.IO.Path.GetExtension(picture.FileName);
                    if (FileExt.ToLower().Equals(".jpg") || FileExt.ToLower().Equals(".png") || FileExt.ToLower().Equals(".jpeg"))
                    {
                        string updatedProfile = await GeneralPurpose.UploadProfilePicture(picture, menu.Photo, "menus", getUserId.Id);
                        menu1.Photo= string.IsNullOrEmpty(updatedProfile) ? menu1.Photo : updatedProfile;
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

            if (!await _menuRepo.AddMenuWithoutSaving(menu1))
            {
                return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
            }

            foreach (var i in menu.MenuIngredient)
            {
                MenuIngredient menuIngredient = new MenuIngredient()
                {
                    MenuId = menu1.Id,
                    IngredientId = i.Id,
                    IsActive = 1,
                    CreatedAt = GeneralPurpose.DateTimeNow(),
                    CreatedBy = menu1.CreatedBy,
                };
                await _menuIngredientRepo.AddMenuIngredientWithoutSaving(menuIngredient);
            }
            if(!await _menuIngredientRepo.SaveChanges())
            {
                return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
            }
            
            return RedirectToAction("Index", new { msg = "Record Added Successfully", color = "green" });

        }

        public async Task<IActionResult> UpdateMenu(string encId = "", string msg = "", string color = "")
        {
            if (!string.IsNullOrEmpty(encId))
            {
                var menuId = StringCipher.DecryptId(encId);
                ViewBag.Menu = await _menuRepo.GetMenuById(Convert.ToInt32(menuId));
            }
            ViewBag.message = msg;
            ViewBag.color = color;
            return View();
        }

        public async Task<IActionResult> PostUpdateMenu(Menu menu, IFormFile picture)
        {
            Menu getMenu = await _menuRepo.GetMenuById(menu.Id);
            getMenu.Name = menu.Name;
            getMenu.Date = menu.Date;
            getMenu.Link = menu.Link;
            getMenu.PreperationForBeaorStaff = menu.PreperationForBeaorStaff;
            getMenu.Cooking = menu.Cooking;
            getMenu.Weight = menu.Weight;

            if (picture != null)
            {
                if (picture.Length != 0)
                {
                    string FileExt = System.IO.Path.GetExtension(picture.FileName);
                    if (FileExt.ToLower().Equals(".jpg") || FileExt.ToLower().Equals(".png") || FileExt.ToLower().Equals(".jpeg"))
                    {
                        string updatedProfile = await GeneralPurpose.UploadProfilePicture(picture, getMenu.Photo, "menus", getMenu.CreatedBy.ToString());
                        getMenu.Photo = string.IsNullOrEmpty(updatedProfile) ? getMenu.Photo : updatedProfile;
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
            if (!await _menuRepo.UpdateMenu(getMenu))
            {
                return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
            }
            return RedirectToAction("Index", new { msg = "Record Updated Successfully", color = "green" });
        }

        [HttpPost]
        public async Task<IActionResult> GetMenuById(string id)
        {
            int MenuId = StringCipher.DecryptId(id);
            Menu? menu = await _menuRepo.GetMenuById(MenuId);
            if (menu == null)
            {
                return Json(0);
            }

            MenuDto obj = new MenuDto()
            {
                Id = menu.Id.ToString(),
                EncId = StringCipher.EncryptId(menu.Id),
                Name = menu.Name,
                Date = menu.Date.ToString(),
                Cooking = menu.Cooking,
                Weight = menu.Weight,
                Link = menu.Link,
                PreperationForBeaorStaff = menu.PreperationForBeaorStaff,
                Photo = menu.Photo,
            };

            return Json(obj);
        }

        public async Task<IActionResult> DeleteMenu(string id)
        {
            int Id = StringCipher.DecryptId(id);

            if (!await _menuRepo.DeleteMenu(Id))
            {
                return RedirectToAction("Index", new { msg = "Somethings' wrong", color = "red" });
            }

            return RedirectToAction("Index", new { msg = "Record deleted successfully!", color = "green" });
        }
    }
}
