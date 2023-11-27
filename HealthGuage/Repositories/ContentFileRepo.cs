using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.EntityFrameworkCore;
using Template.Models;

namespace Template.Repositories
{
    public interface IContentFileRepo
    {
        Task<int> GetContentFileCountByUserId(int UserId);
        Task<IEnumerable<ContentFile>> GetContentFileByUserId(int UserId);

        Task<IEnumerable<ContentFile>> GetActiveContentFileList();

        Task<ContentFile?> GetContentFileById(int id);

        Task<bool> AddContentFile(ContentFile ContentFile);
        Task<bool> AddContentFile(string filePath, IFormFile file, int UserId, int TileId);

        Task<bool> UpdateContentFile(ContentFile ContentFile);

        Task<bool> DeleteContentFile(int id);
    }
    public class ContentFileRepo : IContentFileRepo
    {
        private readonly GeneralPurpose gp;
        private readonly AppDbContext context;

        public ContentFileRepo(AppDbContext _appDbContext, IHttpContextAccessor haccess)
        {
            context = _appDbContext;
            gp = new GeneralPurpose(haccess);
        }
                
        public async Task<int> GetContentFileCountByUserId(int UserId)
        {
            return await context.ContentFile.CountAsync(x => x.IsActive == 1 && x.UserId == UserId);
        }

        public async Task<IEnumerable<ContentFile>> GetContentFileByUserId(int UserId)
        {
            var contentFiles = await context.ContentFile
                                    .Where(x => x.IsActive == 1 && x.UserId == UserId)
                                    .OrderByDescending(x => x.Id)
                                    .ToListAsync();

            return contentFiles;
        }
        
        public async Task<IEnumerable<ContentFile>> GetActiveContentFileList()
        {
            var contentFiles = await context.ContentFile
                                    .Where(x => x.IsActive == 1)
                                    .OrderByDescending(x => x.Id)
                                    .ToListAsync();

            return contentFiles;
        }
        
        public async Task<ContentFile?> GetContentFileById(int id)
        {
            var contentFile = await context.ContentFile
                                    .Where(x => x.IsActive == 1 && x.Id == id)
                                    .OrderByDescending(x => x.Id)
                                    .FirstOrDefaultAsync();

            return contentFile;
        }

        public async Task<bool> AddContentFile(ContentFile contentFile)
        {
            try
            {
                context.ContentFile.Add(contentFile);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddContentFile(string filePath, IFormFile file, int UserId, int TileId)
        {
            try
            {
                ContentFile contentFile = new ContentFile
                {
                    FileName = Path.GetFileName(file.FileName),
                    FilePath = filePath,
                    FileSize = Math.Round((double)file.Length, 2),
                    FileExtension = Path.GetExtension(file.FileName),
                    Type = "TileImage",
                    UserId = UserId, 
                    UploadedBy = UserId,
                    CreatedAt = GeneralPurpose.DateTimeNow(),
                    IsActive = 1,
                };

                context.ContentFile.Add(contentFile);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateContentFile(ContentFile contentFile)
        {
            try
            {
                context.Entry(contentFile).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteContentFile(int id)
        {
            try
            {
                ContentFile? contentFile = await GetContentFileById(id);
                if(contentFile == null)
                {
                    return true;
                }
                contentFile!.IsActive = 0;

                // delete previous file if exists
                DeleteFile(contentFile.FilePath);

                return await UpdateContentFile(contentFile);
            }
            catch
            {
                return false;
            }
        }

        private bool DeleteFile(string oldProfile = "")
        {
            try
            {
                string rootFolder = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\UserTile\");

                if (!string.IsNullOrEmpty(oldProfile))
                {
                    string[] getName = oldProfile.Split("/");

                    // Check if a profile picture already exists for the user
                    string existingProfilePicturePath = Path.Combine(rootFolder, getName.Last());
                    if (File.Exists(existingProfilePicturePath))
                    {
                        File.Delete(existingProfilePicturePath);
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}
