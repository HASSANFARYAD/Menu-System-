using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;

namespace Template.Repositories
{
    public interface IMenuIngredientRepo
    {
        Task<MenuIngredient?> GetMenuIngredientById(int id);
        Task<bool> IsMenuIngredientValidate(int id);
        Task<int> GetActiveMenuIngredientCount(int id = -1);
        Task<IEnumerable<MenuIngredient>> GetActiveMenuIngredientList(int id = -1);
        Task<bool> AddMenuIngredient(MenuIngredient MenuIngredient);
        Task<bool> AddMenuIngredientWithoutSaving(MenuIngredient MenuIngredient);
        Task<bool> UpdateMenuIngredient(MenuIngredient MenuIngredient);
        Task<bool> DeleteMenuIngredient(int id);
        Task<bool> ValidateName(string name, int id = -1);
        Task<bool> SaveChanges();
    }

    public class MenuIngredientRepo : IMenuIngredientRepo
    {
        private readonly AppDbContext context;
        private readonly GeneralPurpose gp;

        public MenuIngredientRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<MenuIngredient?> GetMenuIngredientById(int id)
        {
            return await context.MenuIngredient.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        }

        public async Task<int> GetActiveMenuIngredientCount(int id = -1)
        {
            if (id != -1)
            {
                return await context.MenuIngredient.CountAsync(x => x.IsActive == 1);
            }
            else
            {
                return await context.MenuIngredient.CountAsync(x => x.IsActive == 1 && x.CreatedBy == id);
            }
        }

        public async Task<IEnumerable<MenuIngredient>> GetActiveMenuIngredientList(int id = -1)
        {
            if (id == -1)
            {
                var MenuIngredient = await context.MenuIngredient.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
                return MenuIngredient;
            }
            else
            {
                var MenuIngredient = await context.MenuIngredient.Where(x => x.IsActive == 1 && x.CreatedBy == id).OrderByDescending(x => x.Id).ToListAsync();
                return MenuIngredient;
            }
        }


        public async Task<bool> AddMenuIngredient(MenuIngredient MenuIngredient)
        {
            try
            {
                context.MenuIngredient.Add(MenuIngredient);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddMenuIngredientWithoutSaving(MenuIngredient MenuIngredient)
        {
            try
            {
                context.MenuIngredient.Add(MenuIngredient);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsMenuIngredientValidate(int Id)
        {
            try
            {
                bool chk = await context.MenuIngredient.AnyAsync(x => x.IsActive == 1 && x.Id == Id);
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
        
        public async Task<bool> UpdateMenuIngredient(MenuIngredient MenuIngredient)
        {
            try
            {
                context.Entry(MenuIngredient).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMenuIngredient(int id)
        {
            try
            {
                MenuIngredient? MenuIngredient = await GetMenuIngredientById(id);
                MenuIngredient!.IsActive = 0;
                MenuIngredient!.DeletedAt = GeneralPurpose.DateTimeNow();
                return await UpdateMenuIngredient(MenuIngredient);
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
                emailCount = await context.MenuIngredient.CountAsync(x => x.IsActive == 1 && x.Name!.ToLower() == name.ToLower().Trim());
            }
            else
            {
                emailCount = await context.MenuIngredient.CountAsync(x => x.IsActive == 1 && x.Id != id && x.Name!.ToLower() == name.ToLower().Trim());
            }

            return emailCount == 0;
        }

        public async Task<bool> SaveChanges()
        {
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
    }
}
