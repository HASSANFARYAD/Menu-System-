using DocumentFormat.OpenXml.Spreadsheet;
using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Template.Repositories;

namespace Template.Controllers
{
    public class MenuController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IIngredientRepo _ingredientRepo;
        private readonly IMenuRepo _menuRepo;
        private readonly GeneralPurpose gp;

        public MenuController(IUserRepo userRepo, IIngredientRepo ingredientRepo, IMenuRepo menuRepo, IHttpContextAccessor haccess)
        {
            _userRepo = userRepo;
            _ingredientRepo = ingredientRepo;
            _menuRepo = menuRepo;
            gp = new GeneralPurpose(haccess);
        }

        public IActionResult Index(string msg = "", string color = "")
        {
            ViewBag.message = msg;
            ViewBag.color = color;
            return View();
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
            Menu menu1 = new Menu()
            {
                Name = menu.Name,
                Date = menu.Date,
                Cooking = menu.Cooking,
                Weight = menu.Weight,
                CreatedAt = GeneralPurpose.DateTimeNow(),
                IsActive = 1,
                CreatedBy = Convert.ToInt32(getUserId.Id),
            };

            //if (picture != null)
            //{
            //    if (picture.Length != 0)
            //    {
            //        string FileExt = System.IO.Path.GetExtension(picture.FileName);

            //        if (FileExt.ToLower().Equals(".jpg") || FileExt.ToLower().Equals(".png") || FileExt.ToLower().Equals(".jpeg"))
            //        {
            //            string updatedProfile = await GeneralPurpose.UploadProfilePicture(picture, menu.Photo);
            //            u.ProfilePicture = string.IsNullOrEmpty(updatedProfile) ? u.ProfilePicture : updatedProfile;
            //        }
            //        else
            //        {
            //            return RedirectToAction("UpdateProfile", "Auth", new { msg = "File must be type of jpg/png/jpeg", color = "red" });
            //        }
            //    }
            //    else
            //    {
            //        return RedirectToAction("UpdateProfile", "Auth", new { msg = "Corrupt file. Select other one", color = "red" });
            //    }

            //}
            return View();
        }

        public IActionResult UpdateMenu(string msg = "", string color = "")
        {
            ViewBag.message = msg;
            ViewBag.color = color;
            return View();
        }
    }
}
