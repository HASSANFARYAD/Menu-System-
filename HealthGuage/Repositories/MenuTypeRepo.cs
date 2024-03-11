using DocumentFormat.OpenXml.InkML;
using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;
using Template.Models;

namespace Template.Repositories
{

    public interface IMenuTypeRepo
    {
        Task<MenuType?> GetMenuTypeById(int id);
        Task<IEnumerable<MenuType>> GetActiveMenuTypeList(int Id = -1);
        Task<bool> AddMenuType(MenuType menuType);
        Task<bool> DeleteMenuType(int id);
        Task<bool> UpdateMenuType(MenuType MenuType);
        Task<bool> ValidateName(string name, int id = -1);
    }
    public class MenuTypeRepo : IMenuTypeRepo
    {
        private readonly AppDbContext context;
        private readonly GeneralPurpose gp;
        public MenuTypeRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }
        public async Task<MenuType?> GetMenuTypeById(int id)
        {
            return await context.MenuType.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        }
        public async Task<bool> ValidateName(string name, int id = -1)
        {

            int emailCount = 0;

            if (id == -1)
            {
                emailCount = await context.MenuType.CountAsync(x => x.IsActive == 1 && x.Name!.ToLower() == name.ToLower().Trim());
            }
            else
            {
                emailCount = await context.MenuType.CountAsync(x => x.IsActive == 1 && x.Id != id && x.Name!.ToLower() == name.ToLower().Trim());
            }

            return emailCount == 0;
        }
        public async Task<bool> UpdateMenuType(MenuType MenuType)
        {
            try
            {
                context.Entry(MenuType).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<IEnumerable<MenuType>> GetActiveMenuTypeList(int Id = -1)
        {
            if (Id == -1)
            {
                var MenuType = await context.MenuType.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
                return MenuType;
            }
            else
            {
                var MenuType = await context.MenuType.Where(x => x.IsActive == 1 && x.CreatedBy == Id).OrderByDescending(x => x.Id).ToListAsync();
                return MenuType;
            }
        }
        public async Task<bool> AddMenuType(MenuType menuType)
        {
            try
            {
                context.MenuType.Add(menuType);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> DeleteMenuType(int id)
        {
            try
            {
                MenuType? MenuType = await GetMenuTypeById(id);
                MenuType!.IsActive = 0;
                MenuType!.DeletedAt = GeneralPurpose.DateTimeNow();
                return await UpdateMenuType(MenuType);
            }
            catch
            {
                return false;
            }
        }
    }
    
}
