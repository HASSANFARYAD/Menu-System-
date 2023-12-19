using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.Spreadsheet;
using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Template.HelpingClasses;
using Template.Models;
using Template.Repositories;

namespace Template.Controllers
{
    public class MenuController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IIngredientRepo _ingredientRepo;
        private readonly IProductRepo _productRepo;
        private readonly IPreperationRepo _preparationRepo;
        private readonly IMenuRepo _menuRepo;
        private readonly IMenuIngredientRepo _menuIngredientRepo;
        private readonly IMenuProductRepo _menuProductRepo;
        private readonly IMenuPreperationRepo _menuPreperationRepo;
        private readonly GeneralPurpose gp;

        public MenuController(IUserRepo userRepo, IIngredientRepo ingredientRepo, IMenuRepo menuRepo,
            IMenuIngredientRepo menuIngredientRepo, IMenuProductRepo menuProductRepo,
            IMenuPreperationRepo menuPreperationRepo, IHttpContextAccessor haccess, IProductRepo productRepo,
            IPreperationRepo preperationRepo)
        {
            _userRepo = userRepo;
            _ingredientRepo = ingredientRepo;
            _menuRepo = menuRepo;
            _menuIngredientRepo = menuIngredientRepo;
            _menuProductRepo = menuProductRepo;
            _menuPreperationRepo = menuPreperationRepo;
            gp = new GeneralPurpose(haccess);
            _productRepo = productRepo;
            _preparationRepo = preperationRepo;
        }

        public IActionResult Index(string msg = "", string color = "")
        {
            ViewBag.message = msg;
            ViewBag.color = color;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GetList(string Name = "", string StartDate = "",
            string EndDate = "", string Cooking = "",
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

            if (!String.IsNullOrEmpty(StartDate))
            {
                list = list.Where(x => Convert.ToDateTime(x.Date).Date >= Convert.ToDateTime(StartDate).Date).ToList();
            }

            if (!String.IsNullOrEmpty(EndDate))
            {
                list = list.Where(x => Convert.ToDateTime(x.Date).Date <= Convert.ToDateTime(EndDate).Date).ToList();
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
                var ingredients = await _menuIngredientRepo.GetActiveMenuIngredientListByMenuId(u.Id);
                var products = await _menuProductRepo.GetActiveMenuProductListByMenuId(u.Id);
                var preparations = await _menuPreperationRepo.GetActiveMenuPreperationListByMenuId(u.Id);

                List<string> ingredientsArray = new List<string>();
                List<string> productsArray = new List<string>();
                List<string> preparationsArray = new List<string>();


                foreach (var ingredient in ingredients)
                {
                    var aaa = await _ingredientRepo.GetIngredientById((int)ingredient.IngredientId);
                    ingredientsArray.Add("<span class='badge badge-warning' style='margin:1px'>" + aaa?.Name + "</span>" ?? string.Empty);
                }

                foreach (var product in products)
                {
                    var aaa = await _productRepo.GetProductById((int)product.ProductId);
                    productsArray.Add("<span class='badge badge-success' style='margin:1px'>" + aaa?.Name + "</span>" ?? string.Empty);
                }

                foreach (var preparation in preparations)
                {
                    var aaa = await _preparationRepo.GetPreperationById((int)preparation.PreperationId);
                    preparationsArray.Add("<span class='badge badge-info' style='margin:1px'>" + aaa?.Name + "</span>" ?? string.Empty);
                }

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
                    Ingredients = string.Join("", ingredientsArray),
                    Products = string.Join("", productsArray),
                    Preperations = string.Join("", preparationsArray),
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
            if (getUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            ViewBag.message = msg;
            ViewBag.color = color;
            return View();
        }

        public async Task<IActionResult> PostAddMenu(AddMenuDto menu)
        {
            var getUserId = gp.GetUserClaims();
            var getImage = Request.Form["picture"];
            List<string> ingredientsMenu = menu.Ingredients?.ToList();
            List<string> productsMenu = menu.Products?.ToList();
            List<string> preperationsMenu = menu.Preperations?.ToList();
            Menu menu1 = new Menu()
            {
                Name = menu.Name.ToString(),
                Date = Convert.ToDateTime(menu.Date),
                Cooking = menu.Cooking,
                Weight = menu.Weight,
                Link = menu.Link,
                PreperationForBeaorStaff = menu.PreperationForBeaorStaff,
                IsActive = 1,
                CreatedBy = Convert.ToInt32(getUserId.Id),
                CreatedAt = GeneralPurpose.DateTimeNow(),
            };

            if (menu.Picture != null)
            {
                if (menu.Picture.Length != 0)
                {
                    string FileExt = System.IO.Path.GetExtension(menu.Picture.FileName);
                    if (FileExt.ToLower().Equals(".jpg") || FileExt.ToLower().Equals(".png") || FileExt.ToLower().Equals(".jpeg"))
                    {
                        string updatedProfile = await GeneralPurpose.UploadProfilePicture(menu.Picture, menu.Photo, "menus", getUserId.Id);
                        menu1.Photo = string.IsNullOrEmpty(updatedProfile) ? menu1.Photo : updatedProfile;
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

            if (!await _menuRepo.AddMenu(menu1))
            {
                return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
            }

            foreach (var i in ingredientsMenu)
            {
                MenuIngredient menuIngredient = new MenuIngredient()
                {
                    MenuId = menu1.Id,
                    IngredientId = Convert.ToInt32(StringCipher.DecryptId(i)),
                    IsActive = 1,
                    CreatedAt = GeneralPurpose.DateTimeNow(),
                    CreatedBy = menu1.CreatedBy,
                };

                if (!await _menuIngredientRepo.AddMenuIngredient(menuIngredient))
                {
                    return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
                }
            }

            foreach (var i in productsMenu)
            {
                MenuProduct menuProducts = new MenuProduct()
                {
                    MenuId = menu1.Id,
                    ProductId = Convert.ToInt32(StringCipher.DecryptId(i)),
                    IsActive = 1,
                    CreatedAt = GeneralPurpose.DateTimeNow(),
                    CreatedBy = menu1.CreatedBy,
                };
                if (!await _menuProductRepo.AddMenuProduct(menuProducts))
                {
                    return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
                }
            }

            foreach (var i in preperationsMenu)
            {
                MenuPreperation menuPreperations = new MenuPreperation()
                {
                    MenuId = menu1.Id,
                    PreperationId = Convert.ToInt32(StringCipher.DecryptId(i)),
                    IsActive = 1,
                    CreatedAt = GeneralPurpose.DateTimeNow(),
                    CreatedBy = menu1.CreatedBy,
                };

                if (!await _menuPreperationRepo.AddMenuPreperation(menuPreperations))
                {
                    return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
                }
            }

            return RedirectToAction("Index", new { msg = "Record Added Successfully", color = "green" });

        }

        public async Task<IActionResult> UpdateMenu(string encId = "", string msg = "", string color = "")
        {
            if (!string.IsNullOrEmpty(encId))
            {
                var menuId = StringCipher.DecryptId(encId);
                var ingredients = await _menuIngredientRepo.GetActiveMenuIngredientListByMenuId(menuId);
                var ingredientsArray = ingredients.Select(x => x.IngredientId).ToList();
                ViewBag.ingredients = string.Join(",", ingredientsArray.Select(id => StringCipher.EncryptId((int)id)));

                var products = await _menuProductRepo.GetActiveMenuProductListByMenuId(menuId);
                var productsArray = products.Select(x => x.ProductId).ToList();
                ViewBag.products = string.Join(",", productsArray.Select(id => StringCipher.EncryptId((int)id)));

                var preparations = await _menuPreperationRepo.GetActiveMenuPreperationListByMenuId(menuId);
                var preparationsArray = preparations.Select(x => x.PreperationId).ToList();
                ViewBag.preparations = string.Join(",", preparationsArray.Select(id => StringCipher.EncryptId((int)id)));
                ViewBag.Menu = await _menuRepo.GetMenuById(Convert.ToInt32(menuId));
            }

            ViewBag.message = msg;
            ViewBag.color = color;
            return View();
        }

        public async Task<IActionResult> PostUpdateMenu(AddMenuDto menu)
        {
            Menu getMenu = await _menuRepo.GetMenuById((int)menu.Id);
            if(!await UpdateIngredients(menu.Ingredients, (int)menu.Id))
            {
                return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
            }
            if(!await UpdateProducts(menu.Products, (int)menu.Id))
            {
                return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
            }
            if(!await UpdatePreperations(menu.Preperations, (int)menu.Id))
            {
                return RedirectToAction("Index", new { msg = "Something Went Wrong", color = "red" });
            }
            getMenu.Name = menu.Name;
            getMenu.Date = Convert.ToDateTime(menu.Date);
            getMenu.Link = menu.Link;
            getMenu.PreperationForBeaorStaff = menu.PreperationForBeaorStaff;
            getMenu.Cooking = menu.Cooking;
            getMenu.Weight = menu.Weight;

            if (menu.Picture != null)
            {
                if (menu.Picture.Length != 0)
                {
                    string FileExt = System.IO.Path.GetExtension(menu.Picture.FileName);
                    if (FileExt.ToLower().Equals(".jpg") || FileExt.ToLower().Equals(".png") || FileExt.ToLower().Equals(".jpeg"))
                    {
                        string updatedProfile = await GeneralPurpose.UploadProfilePicture(menu.Picture, getMenu.Photo, "menus", getMenu.CreatedBy.ToString());
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

        [HttpPost]
        public async Task<IActionResult> GenerateCSV(string Name = "", string StartDate = "",
            string EndDate = "", string Cooking = "",
            string Weight = "")
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}/";
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

            if (!String.IsNullOrEmpty(StartDate))
            {
                list = list.Where(x => Convert.ToDateTime(x.Date).Date >= Convert.ToDateTime(StartDate).Date).ToList();
            }

            if (!String.IsNullOrEmpty(EndDate))
            {
                list = list.Where(x => Convert.ToDateTime(x.Date).Date <= Convert.ToDateTime(EndDate).Date).ToList();
            }

            if (!String.IsNullOrEmpty(Cooking))
            {
                list = list.Where(x => x.Cooking.ToLower().Contains(Cooking.Trim().ToLower())).ToList();
            }

            if (!String.IsNullOrEmpty(Weight))
            {
                list = list.Where(x => x.Weight.ToLower().Contains(Weight.Trim().ToLower())).ToList();
            }

            List<MenuDto> udto = new List<MenuDto>();

            foreach (Menu u in list)
            {
                var ingredients = await _menuIngredientRepo.GetActiveMenuIngredientListByMenuId(u.Id);
                var products = await _menuProductRepo.GetActiveMenuProductListByMenuId(u.Id);
                var preparations = await _menuPreperationRepo.GetActiveMenuPreperationListByMenuId(u.Id);

                List<string> ingredientsArray = new List<string>();
                List<string> productsArray = new List<string>();
                List<string> preparationsArray = new List<string>();


                foreach (var ingredient in ingredients)
                {
                    var aaa = await _ingredientRepo.GetIngredientById((int)ingredient.IngredientId);
                    ingredientsArray.Add(aaa?.Name ?? string.Empty);
                }

                foreach (var product in products)
                {
                    var aaa = await _productRepo.GetProductById((int)product.ProductId);
                    productsArray.Add(aaa?.Name ?? string.Empty);
                }

                foreach (var preparation in preparations)
                {
                    var aaa = await _preparationRepo.GetPreperationById((int)preparation.PreperationId);
                    preparationsArray.Add(aaa?.Name ?? string.Empty);
                }

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
                    Photo = string.IsNullOrEmpty(u.Photo) ? "" : baseUrl + u.Photo,
                    Cooking = u.Cooking,
                    Weight = u.Weight,
                    Link = u.Link,
                    Ingredients = string.Join(",", ingredientsArray),
                    Products = string.Join(",", productsArray),
                    Preperations = string.Join(",", preparationsArray),
                    PreperationForBeaorStaff = u.PreperationForBeaorStaff,
                    CreatedBy = userName
                };
                udto.Add(obj);
            }
            try
            {
                var csv = await ExportUser(udto);
                return File(csv.ToArray(), "text/csv", "Heather Stock - Menu.csv");
            }
            catch (Exception ex)
            {
                var exx = ex.Message.ToString();
                return null;
            }
        }

        public async Task<MemoryStream> ExportUser(List<MenuDto> udto)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);
            var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
            csv.WriteField("Name");
            csv.WriteField("Dates");
            csv.WriteField("Photo");
            csv.WriteField("Link");
            csv.WriteField("Preparation");
            csv.WriteField("Preparation for bea or staff");
            csv.WriteField("Ingredients Needed");
            csv.WriteField("Products That Needed to be Prepared");
            csv.WriteField("Cooking");
            csv.WriteField("Weight");
            csv.NextRecord();
            foreach (var i in udto)
            {
                csv.WriteField(i.Name);
                csv.WriteField(i.Date);
                csv.WriteField(i.Photo);
                csv.WriteField(i.Link);
                csv.WriteField(i.Preperations);
                csv.WriteField(i.PreperationForBeaorStaff);
                csv.WriteField(i.Ingredients);
                csv.WriteField(i.Products);
                csv.WriteField(i.Cooking);
                csv.WriteField(i.Weight);
                csv.NextRecord();
            }
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private async Task<bool> UpdateIngredients(string[] ingredientsId, int menuId)
        {
            try
            {
                var getUserId = gp.GetUserClaims();

                var ingredients = await _menuIngredientRepo.GetActiveMenuIngredientListByMenuId(menuId);
                var list = ingredients.ToList();
                foreach (var item in list)
                {
                    if (!await _menuIngredientRepo.DeleteMenuIngredient(item.Id))
                    {
                        return false;
                    }
                }

                var getIngre = ingredientsId.ToList();

                foreach (var ingredient in getIngre)
                {
                    MenuIngredient menuIngredient = new MenuIngredient()
                    {
                        MenuId = menuId,
                        IngredientId = StringCipher.DecryptId(ingredient),
                        IsActive = 1,
                        CreatedBy = Convert.ToInt32(getUserId.Id),
                        CreatedAt = GeneralPurpose.DateTimeNow()
                    };

                    if (!await _menuIngredientRepo.AddMenuIngredient(menuIngredient))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return false;
            }
        }

        private async Task<bool> UpdateProducts(string[] productsId, int menuId)
        {
            try
            {
                var getUserId = gp.GetUserClaims();

                var products = await _menuProductRepo.GetActiveMenuProductListByMenuId(menuId);
                var list = products.ToList();
                foreach (var item in list)
                {
                    if (!await _menuProductRepo.DeleteMenuProduct(item.Id))
                    {
                        return false;
                    }
                }

                var getIdsList = productsId.ToList();

                foreach (var ingredient in getIdsList)
                {
                    MenuProduct menu = new MenuProduct()
                    {
                        MenuId = menuId,
                        ProductId = StringCipher.DecryptId(ingredient),
                        IsActive = 1,
                        CreatedBy = Convert.ToInt32(getUserId.Id),
                        CreatedAt = GeneralPurpose.DateTimeNow()
                    };

                    if (!await _menuProductRepo.AddMenuProduct(menu))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return false;
            }
        }

        private async Task<bool> UpdatePreperations(string[] preperationsId, int menuId)
        {
            try
            {
                var getUserId = gp.GetUserClaims();

                var products = await _menuPreperationRepo.GetActiveMenuPreperationListByMenuId(menuId);
                var list = products.ToList();
                foreach (var item in list)
                {
                    if (!await _menuPreperationRepo.DeleteMenuPreperation(item.Id))
                    {
                        return false;
                    }
                }

                var getIdsList = preperationsId.ToList();

                foreach (var ingredient in getIdsList)
                {
                    MenuPreperation menu = new MenuPreperation()
                    {
                        MenuId = menuId,
                        PreperationId = StringCipher.DecryptId(ingredient),
                        IsActive = 1,
                        CreatedBy = Convert.ToInt32(getUserId.Id),
                        CreatedAt = GeneralPurpose.DateTimeNow()
                    };

                    if (!await _menuPreperationRepo.AddMenuPreperation(menu))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return false;
            }
        }
    }
}
