using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;
using Template.Models;

namespace Template.Repositories
{
	public interface IIngredientRepo
	{
		Task<Ingredient?> GetIngredientById(int id);
		Task<bool> IsIngredientValidate(int id);
		Task<int> GetActiveIngredientCount(int Id = -1);
		Task<IEnumerable<Ingredient>> GetActiveIngredientList(int Id = -1);
		Task<bool> AddIngredient(Ingredient Ingredient);
		Task<bool> UpdateIngredient(Ingredient Ingredient);
		Task<bool> DeleteIngredient(int id);
		Task<bool> ValidateName(string name, int id = -1);
	}

	public class IngredientRepo : IIngredientRepo
	{
		private readonly AppDbContext context;
		private readonly GeneralPurpose gp;

		public IngredientRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
		{
			context = _appDbContext;
			gp = new GeneralPurpose(haccess);
		}

		public async Task<Ingredient?> GetIngredientById(int id)
		{
			return await context.Ingredient.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
		}

		public async Task<int> GetActiveIngredientCount(int Id = -1)
		{
			if (Id == -1)
			{
				return await context.Ingredient.CountAsync(x => x.IsActive == 1);
			}
			else
			{
                return await context.Ingredient.CountAsync(x => x.IsActive == 1 && x.CreatedBy == Id);
            }
		}

		public async Task<IEnumerable<Ingredient>> GetActiveIngredientList(int Id = -1)
		{
			if (Id == -1)
			{
				var Ingredient = await context.Ingredient.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
				return Ingredient;
			}
			else
			{
                var Ingredient = await context.Ingredient.Where(x => x.IsActive == 1 && x.CreatedBy == Id).OrderByDescending(x => x.Id).ToListAsync();
                return Ingredient;
            }
		}


		public async Task<bool> AddIngredient(Ingredient Ingredient)
		{
			try
			{
				context.Ingredient.Add(Ingredient);
				await context.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public async Task<bool> IsIngredientValidate(int Id)
		{
			try
			{
				bool chk = await context.Ingredient.AnyAsync(x => x.IsActive == 1 && x.Id == Id);
				if (chk)
				{
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}
		public async Task<bool> UpdateIngredient(Ingredient Ingredient)
		{
			try
			{
				context.Entry(Ingredient).State = EntityState.Modified;
				await context.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public async Task<bool> DeleteIngredient(int id)
		{
			try
			{
				Ingredient? Ingredient = await GetIngredientById(id);
				Ingredient!.IsActive = 0;
				Ingredient!.DeletedAt = GeneralPurpose.DateTimeNow();
				return await UpdateIngredient(Ingredient);
			}
			catch
			{
				return false;
			}
		}

		public async Task<bool> ValidateName(string name, int id = -1)
		{

			int emailCount = 0;

			if (id == -1)
			{
				emailCount = await context.Ingredient.CountAsync(x => x.IsActive == 1 && x.Name!.ToLower() == name.ToLower().Trim());
			}
			else
			{
				emailCount = await context.Ingredient.CountAsync(x => x.IsActive == 1 && x.Id != id && x.Name!.ToLower() == name.ToLower().Trim());
			}

			return emailCount == 0;
		}
	}
}
