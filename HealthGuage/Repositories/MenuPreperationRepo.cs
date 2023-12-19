using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;

namespace Template.Repositories
{
    public interface IMenuPreperationRepo
    {
        Task<MenuPreperation?> GetMenuPreperationById(int id);
        Task<bool> IsMenuPreperationValidate(int id);
        Task<int> GetActiveMenuPreperationCount(int id = -1, int menuId = -1, int preperationId = -1);
        Task<IEnumerable<MenuPreperation>> GetActiveMenuPreperationList(int id = -1);
        Task<IEnumerable<MenuPreperation>> GetActiveMenuPreperationListByMenuId(int id = -1);
        Task<bool> AddMenuPreperation(MenuPreperation MenuPreperation);
        Task<bool> AddMenuPreperationWithoutSaving(MenuPreperation MenuPreperation);
        Task<bool> UpdateMenuPreperation(MenuPreperation MenuPreperation);
        Task<bool> DeleteMenuPreperation(int id);
        Task<bool> SaveChanges();
    }

    public class MenuPreperationRepo : IMenuPreperationRepo
    {
        private readonly AppDbContext context;
        private readonly GeneralPurpose gp;

        public MenuPreperationRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<MenuPreperation?> GetMenuPreperationById(int id)
        {
            return await context.MenuPreperation.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        }

        public async Task<int> GetActiveMenuPreperationCount(int id = -1, int menuId = -1, int preperationId = -1)
        {
            if (id != -1)
            {
                return await context.MenuPreperation.CountAsync(x => x.IsActive == 1);
            }
            else if (menuId != -1)
            {
                return await context.MenuPreperation.CountAsync(x => x.IsActive == 1 && x.MenuId == menuId);
            }
            else if (preperationId != -1)
            {
                return await context.MenuPreperation.CountAsync(x => x.IsActive == 1 && x.PreperationId == preperationId);
            }
            else
            {
                return await context.MenuPreperation.CountAsync(x => x.IsActive == 1 && x.CreatedBy == id);
            }
        }

        public async Task<IEnumerable<MenuPreperation>> GetActiveMenuPreperationList(int id = -1)
        {
            if (id == -1)
            {
                var MenuPreperation = await context.MenuPreperation.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
                return MenuPreperation;
            }
            else
            {
                var MenuPreperation = await context.MenuPreperation.Where(x => x.IsActive == 1 && x.CreatedBy == id).OrderByDescending(x => x.Id).ToListAsync();
                return MenuPreperation;
            }
        }

        public async Task<IEnumerable<MenuPreperation>> GetActiveMenuPreperationListByMenuId(int id = -1)
        {
            var MenuPreperation = await context.MenuPreperation.Where(x => x.IsActive == 1 && x.MenuId == id).OrderByDescending(x => x.Id).ToListAsync();
            return MenuPreperation;
        }


        public async Task<bool> AddMenuPreperation(MenuPreperation MenuPreperation)
        {
            try
            {
                context.MenuPreperation.Add(MenuPreperation);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddMenuPreperationWithoutSaving(MenuPreperation MenuPreperation)
        {
            try
            {
                context.MenuPreperation.Add(MenuPreperation);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsMenuPreperationValidate(int Id)
        {
            try
            {
                bool chk = await context.MenuPreperation.AnyAsync(x => x.IsActive == 1 && x.Id == Id);
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

        public async Task<bool> UpdateMenuPreperation(MenuPreperation MenuPreperation)
        {
            try
            {
                context.Entry(MenuPreperation).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMenuPreperation(int id)
        {
            try
            {
                MenuPreperation? MenuPreperation = await GetMenuPreperationById(id);
                MenuPreperation!.IsActive = 0;
                MenuPreperation!.DeletedAt = GeneralPurpose.DateTimeNow();
                return await UpdateMenuPreperation(MenuPreperation);
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> SaveChanges()
        {
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
