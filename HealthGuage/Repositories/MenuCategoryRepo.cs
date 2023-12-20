using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;
using Template.Models;

namespace Template.Repositories
{
    public interface IMenuCategoryRepo
    {
        Task<MenuCategory?> GetMenuCategoryById(int id);
        Task<bool> IsMenuCategoryValidate(int id);
        Task<int> GetActiveMenuCategoryCount(int Id = -1);
        Task<IEnumerable<MenuCategory>> GetActiveMenuCategoryList(int Id = -1);
        Task<bool> AddMenuCategory(MenuCategory MenuCategory);
        Task<bool> UpdateMenuCategory(MenuCategory MenuCategory);
        Task<bool> DeleteMenuCategory(int id);
        Task<bool> ValidateName(string name, int id = -1);
    }

    public class MenuCategoryRepo : IMenuCategoryRepo
    {
        private readonly AppDbContext context;
        private readonly GeneralPurpose gp;

        public MenuCategoryRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<MenuCategory?> GetMenuCategoryById(int id)
        {
            return await context.MenuCategory.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        }

        public async Task<int> GetActiveMenuCategoryCount(int Id = -1)
        {
            if (Id == -1)
            {
                return await context.MenuCategory.CountAsync(x => x.IsActive == 1);
            }
            else
            {
                return await context.MenuCategory.CountAsync(x => x.IsActive == 1 && x.CreatedBy == Id);
            }
        }

        public async Task<IEnumerable<MenuCategory>> GetActiveMenuCategoryList(int Id = -1)
        {
            if (Id == -1)
            {
                var MenuCategory = await context.MenuCategory.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
                return MenuCategory;
            }
            else
            {
                var MenuCategory = await context.MenuCategory.Where(x => x.IsActive == 1 && x.CreatedBy == Id).OrderByDescending(x => x.Id).ToListAsync();
                return MenuCategory;
            }
        }


        public async Task<bool> AddMenuCategory(MenuCategory MenuCategory)
        {
            try
            {
                context.MenuCategory.Add(MenuCategory);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsMenuCategoryValidate(int Id)
        {
            try
            {
                bool chk = await context.MenuCategory.AnyAsync(x => x.IsActive == 1 && x.Id == Id);
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
        public async Task<bool> UpdateMenuCategory(MenuCategory MenuCategory)
        {
            try
            {
                context.Entry(MenuCategory).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMenuCategory(int id)
        {
            try
            {
                MenuCategory? MenuCategory = await GetMenuCategoryById(id);
                MenuCategory!.IsActive = 0;
                MenuCategory!.DeletedAt = GeneralPurpose.DateTimeNow();
                return await UpdateMenuCategory(MenuCategory);
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
                emailCount = await context.MenuCategory.CountAsync(x => x.IsActive == 1 && x.Name!.ToLower() == name.ToLower().Trim());
            }
            else
            {
                emailCount = await context.MenuCategory.CountAsync(x => x.IsActive == 1 && x.Id != id && x.Name!.ToLower() == name.ToLower().Trim());
            }

            return emailCount == 0;
        }
    }
}
