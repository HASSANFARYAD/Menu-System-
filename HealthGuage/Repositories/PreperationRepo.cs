using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;

namespace Template.Repositories
{
    public interface IPreperationRepo
    {
        Task<Preperation?> GetPreperationById(int id);
        Task<bool> IsPreperationValidate(int id);
        Task<int> GetActivePreperationCount(int id = -1);
        Task<IEnumerable<Preperation>> GetActivePreperationList(int id = -1);
        Task<bool> AddPreperation(Preperation Preperation);
        Task<bool> UpdatePreperation(Preperation Preperation);
        Task<bool> DeletePreperation(int id);
        Task<bool> ValidateName(string name, int id = -1);
    }

    public class PreperationRepo : IPreperationRepo
    {
        private readonly AppDbContext context;
        private readonly GeneralPurpose gp;

        public PreperationRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<Preperation?> GetPreperationById(int id)
        {
            return await context.Preperation.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        }

        public async Task<int> GetActivePreperationCount(int id = -1)
        {
            if (id != -1)
            {
                return await context.Preperation.CountAsync(x => x.IsActive == 1);
            }
            else
            {
                return await context.Preperation.CountAsync(x => x.IsActive == 1 && x.CreatedBy == id);
            }
        }

        public async Task<IEnumerable<Preperation>> GetActivePreperationList(int id = -1)
        {
            if (id == -1)
            {
                var Preperation = await context.Preperation.Where(x => x.IsActive == 1).OrderByDescending(x => x.Id).ToListAsync();
                return Preperation;
            }
            else
            {
                var Preperation = await context.Preperation.Where(x => x.IsActive == 1 && x.CreatedBy == id).OrderByDescending(x => x.Id).ToListAsync();
                return Preperation;
            }
        }


        public async Task<bool> AddPreperation(Preperation Preperation)
        {
            try
            {
                context.Preperation.Add(Preperation);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsPreperationValidate(int Id)
        {
            try
            {
                bool chk = await context.Preperation.AnyAsync(x => x.IsActive == 1 && x.Id == Id);
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
        public async Task<bool> UpdatePreperation(Preperation Preperation)
        {
            try
            {
                context.Entry(Preperation).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePreperation(int id)
        {
            try
            {
                Preperation? Preperation = await GetPreperationById(id);
                Preperation!.IsActive = 0;
                Preperation!.DeletedAt = GeneralPurpose.DateTimeNow();
                return await UpdatePreperation(Preperation);
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
                emailCount = await context.Preperation.CountAsync(x => x.IsActive == 1 && x.Name!.ToLower() == name.ToLower().Trim());
            }
            else
            {
                emailCount = await context.Preperation.CountAsync(x => x.IsActive == 1 && x.Id != id && x.Name!.ToLower() == name.ToLower().Trim());
            }

            return emailCount == 0;
        }
    }
}
