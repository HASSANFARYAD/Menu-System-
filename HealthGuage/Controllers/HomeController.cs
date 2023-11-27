using HealthGuage.Filters;
using HealthGuage.HelpingClasses;
using HealthGuage.Repositories;
using LoginFinal.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;
using Template.Repositories;

namespace HealthGuage.Controllers
{
    [ExceptionFilter]
    [ValidationFilter(Roles = new int[] { 2 })]
    public class HomeController : Controller
    {
        private readonly IUserRepo userRepo;
        private readonly IIngredientRepo _ingredientRepo;
        private readonly GeneralPurpose gp;

        public HomeController(IUserRepo _userRepo, IIngredientRepo ingredientRepo, IHttpContextAccessor haccess)
        {
            userRepo = _userRepo;
            _ingredientRepo = ingredientRepo;
            gp = new GeneralPurpose(haccess);
        }

        private async Task<bool> IsUserValidate()
        {
            int userId = Convert.ToInt32(gp.GetUserClaims().Id);
            if(userId != null)
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

        public async Task<IActionResult> Index()
        {
            bool chkUserValidate = await IsUserValidate();
            if(!chkUserValidate)
            {
                return RedirectToAction("LogOut", "Auth", new { msg = "Something' wrong", color = "red" });
            }
            int userId = Convert.ToInt32(gp.GetUserClaims().Id);
            ViewBag.Ingredients = await _ingredientRepo.GetActiveIngredientCount(userId);
            return View();
        }
    }
}