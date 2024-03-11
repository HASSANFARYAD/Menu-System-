using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Template.HelpingClasses;
using Template.Models;
using Template.Repositories;

namespace Template.Controllers
{
    public class MenuTypeController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IMenuTypeRepo _menuTypeRepo;
        private readonly IMenuRepo _menuRepo;
        private readonly GeneralPurpose gp;
        public MenuTypeController(IUserRepo userRepo, IMenuRepo menuRepo, IMenuTypeRepo menuTypeRepo, IHttpContextAccessor haccess)
        {
            _userRepo = userRepo;
            _menuTypeRepo = menuTypeRepo;
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
        public async Task<IActionResult> GetMenuTypeDataTableList(string Name = "")
        {
            var getUserId = gp.GetUserClaims();
            List<MenuType>? ulist = new List<MenuType>();
            if (getUserId.Role == 1)
            {
                ulist = (List<MenuType>)await _menuTypeRepo.GetActiveMenuTypeList();
            }
            else
            {
                ulist = (List<MenuType>)await _menuTypeRepo.GetActiveMenuTypeList(Convert.ToInt32(getUserId.Id));
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
            List<MenuTypeDto> udto = new List<MenuTypeDto>();

            foreach (MenuType u in ulist)
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
                MenuTypeDto obj = new MenuTypeDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                    Description = u.Description,
                    CreatedBy = userName,
                };

                udto.Add(obj);
            }
            return Json(new { data = udto, draw = Request.Form["draw"].FirstOrDefault(), recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig });
        }

        [HttpPost]
        public async Task<IActionResult> PostAddMenuType(AddMenuDto _menuType)
        {
            if (string.IsNullOrEmpty(_menuType.Name))
            {
                return RedirectToAction("Index", new { msg = "All Fields are Required", color = "green" });
            }
            var getUserId = gp.GetUserClaims();
            MenuType menuType = new MenuType
            {
                Name = _menuType.Name.Trim(),
                Description=_menuType.Description,
                IsActive = 1,
                CreatedAt = GeneralPurpose.DateTimeNow(),
                CreatedBy = Convert.ToInt32(getUserId.Id)
            };
            if (!await _menuTypeRepo.AddMenuType(menuType))
            {
                return RedirectToAction(actionName: "Index", new { msg = "Somethings' Wrong", color = "red" });
            }

            return RedirectToAction(actionName: "Index", new { msg = "Record Inserted Successfully", color = "green" });

        }

        public async Task<IActionResult> DeleteMenuType(string id)
        {
            int Id = StringCipher.DecryptId(id);
            var getList = await _menuRepo.GetActiveMenuCount(-1, (int)Id);
            if (getList > 0)
            {
                return RedirectToAction("Index", new { msg = "This menuType already has " + getList + " menus allocated to it. Please first delete them.", color = "red" });
            }
            if (!await _menuTypeRepo.DeleteMenuType(Id))
            {
                return RedirectToAction("Index", new { msg = "Somethings' wrong", color = "red" });
            }

            return RedirectToAction("Index", new { msg = "Record deleted successfully!", color = "green" });
        }

        //[HttpPost]
        public async Task<IActionResult> PostUpdateMenuType(AddMenuTypeDto _menuTypeDto)
        {
            MenuType? menuType = await _menuTypeRepo.GetMenuTypeById((int)_menuTypeDto.Id);
            if (menuType == null)
            {
                return RedirectToAction("Index", new { msg = "Record not found", color = "red" });
            }
            menuType.Name = _menuTypeDto.Name.Trim();
            menuType.Description = _menuTypeDto.Description.Trim();
            menuType.UpdatedAt = GeneralPurpose.DateTimeNow();


            if (await _menuTypeRepo.UpdateMenuType(menuType))
            {
                return RedirectToAction("Index", new { msg = "MenuType updated successfully", color = "green" });
            }
            return RedirectToAction("Index", new { msg = "Somethings' wrong", color = "red" });
        }

        [HttpPost]
        public async Task<IActionResult> GetMenuTypeById(string id)
        {
            int menuTypeId = StringCipher.DecryptId(id);
            MenuType? m = await _menuTypeRepo.GetMenuTypeById(menuTypeId);
            if (m == null)
            {
                return Json(0);
            }

            MenuTypeDto obj = new MenuTypeDto()
            {
                Id = m.Id.ToString(),
                Name = m.Name,
                Description = m.Description,
            };

            return Json(obj);
        }
        [HttpPost]
        public async Task<IActionResult> GetMenuTypeList()
        {
            UserDto? getUserId = gp.GetUserClaims();

            List<MenuType>? list = new List<MenuType>();
            if (getUserId.Role == 1)
            {
                list = (List<MenuType>)await _menuTypeRepo.GetActiveMenuTypeList();
            }
            else
            {
                list = (List<MenuType>)await _menuTypeRepo.GetActiveMenuTypeList(Convert.ToInt32(getUserId.Id));
            }

            List<MenuTypeDto> udto = new List<MenuTypeDto>();

            foreach (MenuType u in list)
            {
                MenuTypeDto obj = new MenuTypeDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                    Description=u.Description,
                };

                udto.Add(obj);
            }

            return Json(udto);
        }

        public async Task<IActionResult> GetMenuTypeLists()
        {
            UserDto? getUserId = gp.GetUserClaims();

            List<MenuType>? list = new List<MenuType>();
            if (getUserId.Role == 1)
            {
                list = (List<MenuType>)await _menuTypeRepo.GetActiveMenuTypeList();
            }
            else
            {
                list = (List<MenuType>)await _menuTypeRepo.GetActiveMenuTypeList(Convert.ToInt32(getUserId.Id));
            }

            List<MenuTypeDto> udto = new List<MenuTypeDto>();

            foreach (MenuType u in list)
            {
                MenuTypeDto obj = new MenuTypeDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                    Description = u.Description,
                };

                udto.Add(obj);
            }

            return View(udto);
        }
        [HttpPost]
        public async Task<IActionResult> ValidateName(string name, string id = "")
        {
            if (!string.IsNullOrEmpty(name))
            {
                return Json(await _menuTypeRepo.ValidateName(name, Convert.ToInt32(id)));
            }
            else
            {
                return Json("Name cannot be Empty");
            }
        }
    }
}
