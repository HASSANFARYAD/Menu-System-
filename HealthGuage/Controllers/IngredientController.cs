using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Template.HelpingClasses;
using Template.Models;
using Template.Repositories;

namespace Template.Controllers
{
	public class IngredientController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IIngredientRepo _ingredientRepo;
		private readonly GeneralPurpose gp;

		public IngredientController(IUserRepo userRepo, IIngredientRepo ingredientRepo, IHttpContextAccessor haccess)
        {
			_userRepo = userRepo;
            _ingredientRepo = ingredientRepo;
			gp = new GeneralPurpose(haccess);
		}

		public async Task<IActionResult> Index(string msg = "", string color = "black")
		{
			ViewBag.Message = msg;
			ViewBag.Color = color;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> GetIngredientDataTableList(string Name = "")
		{
			var getUserId = gp.GetUserClaims();
			var ulist = new List<Ingredient>();
			if (getUserId.Role == 1)
			{
				ulist = (List<Ingredient>)await _ingredientRepo.GetActiveIngredientList();
			}
			else
			{
				ulist = (List<Ingredient>)await _ingredientRepo.GetActiveIngredientList(Convert.ToInt32(getUserId.Id)) ;
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

			List<IngredientDto> udto = new List<IngredientDto>();

			foreach (Ingredient u in ulist)
			{
				var userName = "";
				var createdby = await _userRepo.GetUserById((int)u.CreatedBy);
				if(Convert.ToInt32(getUserId.Id) == u.CreatedBy)
				{
					userName = "You";
                }
				else
				{
					userName = createdby.Name;
				}
				IngredientDto obj = new IngredientDto()
				{
					Id = StringCipher.EncryptId(u.Id),
					Name = u.Name,
					CreatedBy = createdby != null ? userName : "",
				};

				udto.Add(obj);
			}

			return Json(new { data = udto, draw = Request.Form["draw"].FirstOrDefault(), recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig });
		}

		[HttpPost]
		public async Task<IActionResult> PostAddIngredient(Ingredient _ingredient)
		{
			if (string.IsNullOrEmpty(_ingredient.Name))
			{
				return RedirectToAction("Index", new { msg = "All Fields are Required", color = "green" });
			}
            var getUserId = gp.GetUserClaims();
            Ingredient ingredient = new Ingredient {
				Name = _ingredient.Name.Trim(),
				IsActive = 1,
				CreatedAt = GeneralPurpose.DateTimeNow(),
				CreatedBy = Convert.ToInt32(getUserId.Id)
            };
            if (!await _ingredientRepo.AddIngredient(ingredient))
            {
                return RedirectToAction("Index", new { msg = "Somethings' Wrong", color = "red" });
            }

            return RedirectToAction("Index", new { msg = "Record Inserted Successfully", color = "green" });
        }

        [HttpPost]
        public async Task<IActionResult> PostUpdateIngredient(Ingredient _ingredient)
        {
            Ingredient? ingredient = await _ingredientRepo.GetIngredientById(_ingredient.Id);
            if (ingredient == null)
            {
                return RedirectToAction("Index", new { msg = "Record not found", color = "red" });
            }
            ingredient.Name = _ingredient.Name.Trim();
            ingredient.UpdatedAt = GeneralPurpose.DateTimeNow();

            if (await _ingredientRepo.UpdateIngredient(ingredient))
            {
                return RedirectToAction("Index", new { msg = "Ingredient updated successfully", color = "green" });
            }
            return RedirectToAction("Index", new { msg = "Somethings' wrong", color = "red" });
        }

        [HttpPost]
		public async Task<IActionResult> GetIngredientById(string id)
		{
			int IngredientId = StringCipher.DecryptId(id);
			Ingredient? u = await _ingredientRepo.GetIngredientById(IngredientId);
			if (u == null)
			{
				return Json(0);
			}

			IngredientDto obj = new IngredientDto()
			{
				Id = u.Id.ToString(),
				Name = u.Name,
			};

			return Json(obj);
		}

        public async Task<IActionResult> DeleteIngredient(string id)
        {
            int Id = StringCipher.DecryptId(id);

            if (!await _ingredientRepo.DeleteIngredient(Id))
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
				return Json(await _ingredientRepo.ValidateName(name, Convert.ToInt32(id)));
			}
			else
			{
				return Json("Name cannot be Empty");
			}
		}
	}
}
