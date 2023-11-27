using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Template.HelpingClasses;

namespace HealthGuage.Repositories
{

    public interface IUserRepo
    {
        Task<User?> GetUserByLogin(string email);
        Task<User?> GetUserById(int id);
        Task<bool> IsUserValidate(int id);
        Task<int> GetActiveUserCount();
        Task<IEnumerable<User>> GetActiveUserList();
        Task<bool> AddUser(User user);
        Task<bool> UpdateUser(User user);
        Task<bool> DeleteUser(int id);
        Task<bool> ValidateEmail(string email, int id = -1);
        Task<User?> GetUserByEmail(string email);
    }

    public class UserRepo: IUserRepo
    {
        private readonly AppDbContext context; 
        private readonly GeneralPurpose gp;

        public UserRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await context.User.FirstOrDefaultAsync(x=> x.Id == id && x.IsActive == 1);
        }
        
        public async Task<int> GetActiveUserCount()
        {
            return await context.User.CountAsync(x=> x.IsActive == 1 && x.Role == 2);
        }

        public async Task<User?> GetUserByLogin(string email)
        {
            return await context.User
                .FirstOrDefaultAsync(x => x.IsActive == 1 && 
                                            x.Email!.ToLower() == email.Trim().ToLower());
        }

        public async Task<IEnumerable<User>> GetActiveUserList()
        {
            var User = await context.User.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
            return User;
        }


        public async Task<bool> AddUser(User user)
        {
            try
            {
                context.User.Add(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsUserValidate(int Id)
        {
            try
            {
                bool chk = await context.User.AnyAsync(x=> x.IsActive == 1 && x.Id == Id);
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
        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                context.Entry(user).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUser(int id)
        {
            try
            {
                User? user = await GetUserById(id);
                user!.IsActive = 0;
                return await UpdateUser(user);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidateEmail(string email, int id = -1)
        {

            int emailCount = 0;

            if (id == -1)
            {
                emailCount = await context.User.CountAsync(x => x.IsActive == 1 && x.Email!.ToLower() == email.ToLower().Trim());
            }
            else
            {
                emailCount = await context.User.CountAsync(x => x.IsActive == 1 && x.Id != id && x.Email!.ToLower() == email.ToLower().Trim());
            }

            return emailCount == 0;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await context.User.FirstOrDefaultAsync(x => x.Email!.ToLower() == email.Trim().ToLower() && x.IsActive == 1);
        }
    }
}
