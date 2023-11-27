using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;

namespace Template.Repositories
{
    public interface IProductRepo
    {
        Task<Product?> GetProductById(int id);
        Task<bool> IsProductValidate(int id);
        Task<int> GetActiveProductCount(int id = -1);
        Task<IEnumerable<Product>> GetActiveProductList(int id = -1);
        Task<bool> AddProduct(Product Product);
        Task<bool> UpdateProduct(Product Product);
        Task<bool> DeleteProduct(int id);
        Task<bool> ValidateName(string name, int id = -1);
    }

    public class ProductRepo : IProductRepo
    {
        private readonly AppDbContext context;
        private readonly GeneralPurpose gp;

        public ProductRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<Product?> GetProductById(int id)
        {
            return await context.Product.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        }

        public async Task<int> GetActiveProductCount(int id = -1)
        {
            if (id != -1)
            {
                return await context.Product.CountAsync(x => x.IsActive == 1);
            }
            else
            {
                return await context.Product.CountAsync(x => x.IsActive == 1 && x.CreatedBy == id);
            }
        }

        public async Task<IEnumerable<Product>> GetActiveProductList(int id = -1)
        {
            if (id == -1)
            {
                var Product = await context.Product.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
                return Product;
            }
            else
            {
                var Product = await context.Product.Where(x => x.IsActive == 1 && x.CreatedBy == id).OrderByDescending(x => x.Id).ToListAsync();
                return Product;
            }
        }


        public async Task<bool> AddProduct(Product Product)
        {
            try
            {
                context.Product.Add(Product);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsProductValidate(int Id)
        {
            try
            {
                bool chk = await context.Product.AnyAsync(x => x.IsActive == 1 && x.Id == Id);
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
        public async Task<bool> UpdateProduct(Product Product)
        {
            try
            {
                context.Entry(Product).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProduct(int id)
        {
            try
            {
                Product? Product = await GetProductById(id);
                Product!.IsActive = 0;
                Product!.DeletedAt = GeneralPurpose.DateTimeNow();
                return await UpdateProduct(Product);
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
                emailCount = await context.Product.CountAsync(x => x.IsActive == 1 && x.Name!.ToLower() == name.ToLower().Trim());
            }
            else
            {
                emailCount = await context.Product.CountAsync(x => x.IsActive == 1 && x.Id != id && x.Name!.ToLower() == name.ToLower().Trim());
            }

            return emailCount == 0;
        }
    }
}
