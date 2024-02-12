using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;

namespace Template.Repositories
{
    public interface IMenuRepo
    {
        Task<Menu?> GetMenuById(int id);
        Task<bool> IsMenuValidate(int id);
        Task<int> GetActiveMenuCount(int id = -1, int categoryId = -1);
        Task<IEnumerable<Menu>> GetActiveMenuList(int id = -1);
        Task<bool> AddMenu(Menu Menu);
        Task<bool> AddMenuWithoutSaving(Menu Menu);
        Task<bool> UpdateMenu(Menu Menu);
        Task<bool> DeleteMenu(int id);
        Task<bool> ValidateName(string name, int id = -1);
    }

    public class MenuRepo : IMenuRepo
    {
        private readonly AppDbContext context;
        private readonly GeneralPurpose gp;

        public MenuRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<Menu?> GetMenuById(int id)
        {
            return await context.Menu.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        }

        public async Task<int> GetActiveMenuCount(int id = -1, int categoryId = -1)
        {
            if (id != -1)
            {
                return await context.Menu.CountAsync(x => x.IsActive == 1);
            }
            else if(categoryId != -1)
            {
                return await context.Menu.CountAsync(x => x.IsActive == 1 && x.CategoryId == categoryId);
            }
            else
            {
                return await context.Menu.CountAsync(x => x.IsActive == 1 && x.CreatedBy == id);
            }
        }

        public async Task<IEnumerable<Menu>> GetActiveMenuList(int id = -1)
        {
            if (id == -1)
            {
                var Menu = await context.Menu.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
                return Menu;
            }
            else
            {
                var Menu = await context.Menu.Where(x => x.IsActive == 1 && x.CreatedBy == id).OrderByDescending(x => x.Id).ToListAsync();
                return Menu;
            }
        }


        public async Task<bool> AddMenu(Menu Menu)
        {
            try
            {
                context.Menu.Add(Menu);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> AddMenuWithoutSaving(Menu Menu)
        {
            try
            {
                context.Menu.Add(Menu);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsMenuValidate(int Id)
        {
            try
            {
                bool chk = await context.Menu.AnyAsync(x => x.IsActive == 1 && x.Id == Id);
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
        public async Task<bool> UpdateMenu(Menu Menu)
        {
            try
            {
                context.Entry(Menu).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMenu(int id)
        {
            try
            {
                Menu? Menu = await GetMenuById(id);
                Menu!.IsActive = 0;
                Menu!.DeletedAt = GeneralPurpose.DateTimeNow();
                return await UpdateMenu(Menu);
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
                emailCount = await context.Menu.CountAsync(x => x.IsActive == 1 && x.Name!.ToLower() == name.ToLower().Trim());
            }
            else
            {
                emailCount = await context.Menu.CountAsync(x => x.IsActive == 1 && x.Id != id && x.Name!.ToLower() == name.ToLower().Trim());
            }

            return emailCount == 0;
        }
    }
}
