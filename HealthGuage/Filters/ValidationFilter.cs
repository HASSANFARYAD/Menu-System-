using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LoginFinal.Filters
{
    //we need this class to register in Startup.cs as Scoped service
    //paste the following code in that class
    //services.AddScoped<ValidationFilter>();
    public class ValidationFilter : Attribute, IActionFilter
    {
        public int[] Roles;

        public ValidationFilter(int[] Roles = null)
        {
            this.Roles = Roles;
        }

        public async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //this is called manually dependency injection
            //the best approach is to use it through constructor
            //but here it is currently not possible because
            //we can not pass oject as parameter in an attribute
            var services = filterContext.HttpContext.RequestServices;
            var gp = (GeneralPurpose)services.GetService(typeof(GeneralPurpose));

            UserDto? LoggedinUser = gp.GetUserClaims();

            if (LoggedinUser != null)
            {
                if (!Roles.Contains((int)LoggedinUser.Role))
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{
                            { "controller", "Auth" },{ "action", "Login" }, });
                }
            }
            else
            {
                var values = new RouteValueDictionary(new
                {
                    action = "Login",
                    controller = "Auth",
                    msg = "Session expired, Please login",
                    color = "red"
                });

                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(values));
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

    }
}
