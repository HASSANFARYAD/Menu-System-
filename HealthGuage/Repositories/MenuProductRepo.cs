using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;

namespace Template.Repositories
{
    public interface IMenuProductRepo
    {
        Task<MenuProduct?> GetMenuProductById(int id);
        Task<bool> IsMenuProductValidate(int id);
        Task<int> GetActiveMenuProductCount(int id = -1, int menuId = -1, int productId = -1);
        Task<IEnumerable<MenuProduct>> GetActiveMenuProductList(int id = -1);
        Task<IEnumerable<MenuProduct>> GetActiveMenuProductListByMenuId(int id = -1);
        Task<bool> AddMenuProduct(MenuProduct MenuProduct);
        Task<bool> AddMenuProductWithoutSaving(MenuProduct MenuProduct);
        Task<bool> UpdateMenuProduct(MenuProduct MenuProduct);
        Task<bool> DeleteMenuProduct(int id);
        Task<bool> SaveChanges();
    }

    public class MenuProductRepo : IMenuProductRepo
    {
        private readonly AppDbContext context;
        private readonly GeneralPurpose gp;

        public MenuProductRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<MenuProduct?> GetMenuProductById(int id)
        {
            return await context.MenuProduct.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        }

        public async Task<int> GetActiveMenuProductCount(int id = -1, int menuId = -1, int productId = -1)
        {
            if (id != -1)
            {
                return await context.MenuProduct.CountAsync(x => x.IsActive == 1);
            }
            else if (menuId != -1)
            {
                return await context.MenuProduct.CountAsync(x => x.IsActive == 1 && x.MenuId == menuId);
            }
            else if (productId != -1)
            {
                return await context.MenuProduct.CountAsync(x => x.IsActive == 1 && x.ProductId == productId);
            }
            else
            {
                return await context.MenuProduct.CountAsync(x => x.IsActive == 1 && x.CreatedBy == id);
            }
        }


        public async Task<IEnumerable<MenuProduct>> GetActiveMenuProductList(int id = -1)
        {
            if (id == -1)
            {
                var MenuProduct = await context.MenuProduct.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
                return MenuProduct;
            }
            else
            {
                var MenuProduct = await context.MenuProduct.Where(x => x.IsActive == 1 && x.CreatedBy == id).OrderByDescending(x => x.Id).ToListAsync();
                return MenuProduct;
            }
        }

        public async Task<IEnumerable<MenuProduct>> GetActiveMenuProductListByMenuId(int id = -1)
        {
            var MenuProduct = await context.MenuProduct.Where(x => x.IsActive == 1 && x.MenuId == id)
                .OrderByDescending(x => x.Id).ToListAsync();
            return MenuProduct;
            
        }


        public async Task<bool> AddMenuProduct(MenuProduct MenuProduct)
        {
            try
            {
                context.MenuProduct.Add(MenuProduct);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddMenuProductWithoutSaving(MenuProduct MenuProduct)
        {
            try
            {
                context.MenuProduct.Add(MenuProduct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsMenuProductValidate(int Id)
        {
            try
            {
                bool chk = await context.MenuProduct.AnyAsync(x => x.IsActive == 1 && x.Id == Id);
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

        public async Task<bool> UpdateMenuProduct(MenuProduct MenuProduct)
        {
            try
            {
                context.Entry(MenuProduct).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteMenuProduct(int id)
        {
            try
            {
                MenuProduct? MenuProduct = await GetMenuProductById(id);
                MenuProduct!.IsActive = 0;
                MenuProduct!.DeletedAt = GeneralPurpose.DateTimeNow();
                return await UpdateMenuProduct(MenuProduct);
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
