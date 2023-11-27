using HealthGuage.Filters;
using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using LoginFinal.Filters;
using Microsoft.AspNetCore.Mvc;
using Template.HelpingClasses;
using Template.Repositories;

namespace HealthGuage.Controllers
{
    [ExceptionFilter]
    [ValidationFilter(Roles = new int[] { 1 })]
    public class AdminController : Controller
    {
        private readonly IUserRepo userRepo;
        private readonly IIngredientRepo _ingredientRepo;

        public AdminController(IUserRepo _userRepo, IIngredientRepo ingredientRepo)
        {
            userRepo = _userRepo;
            _ingredientRepo = ingredientRepo;
        }

        
        public async Task<IActionResult> Index(string msg = "", string color = "black")
        {
            ViewBag.UserCount = await userRepo.GetActiveUserCount();
            ViewBag.Ingredients = await _ingredientRepo.GetActiveIngredientCount();
            
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        #region Manage User
        public IActionResult AddUser(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostAddUser(User _user)
        {
            _user.Name = _user.Name.Trim();
            _user.Email = _user.Email.Trim();
            _user.Password = StringCipher.EncryptString(_user.Password.Trim());
            _user.Role = 2;
            _user.IsActive = 1;
            _user.CreatedAt = GeneralPurpose.DateTimeNow();

            if (!await userRepo.AddUser(_user))
            {
                return RedirectToAction("AddUser", "Admin", new { msg = "Somethings' Wrong", color = "red" });
            }

            return RedirectToAction("AddUser", "Admin", new { msg = "Record Inserted Successfully", color = "green" });
        }

        public IActionResult ViewUser(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostUpdateUser(User _user)
        {
            User? user = await userRepo.GetUserById(_user.Id);
            if (user == null)
            {
                return RedirectToAction("ViewUser", "Admin", new { msg = "Record not found", color = "red" });
            }
            user.Name = _user.Name.Trim();
            user.Email = _user.Email.Trim();
            user.ProfilePicture = _user.ProfilePicture;
            user.PhoneNumber = _user.PhoneNumber;

            if (await userRepo.UpdateUser(user))
            {
                return RedirectToAction("ViewUser", "Admin", new { msg = "User updated successfully", color = "green" });
            }
            return RedirectToAction("ViewUser", "Admin", new { msg = "Somethings' wrong", color = "red" });
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            int userId = StringCipher.DecryptId(id);

            if (!await userRepo.DeleteUser(userId))
            {
                return RedirectToAction("ViewUser", "Admin", new { msg = "Somethings' wrong", color = "red" });

            }

            return RedirectToAction("ViewUser", "Admin", new { msg = "Record deleted successfully!", color = "green" });
        }

        #endregion

    }
}
