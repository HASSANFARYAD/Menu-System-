using HealthGuage.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Template.HelpingClasses;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;


namespace HealthGuage.HelpingClasses
{
    public class GeneralPurpose
    {
        private readonly HttpContext hcontext;
        public GeneralPurpose(IHttpContextAccessor haccess)
        {
            hcontext = haccess.HttpContext;
        }

        public UserDto? GetUserClaims()
        {
            string? userId = hcontext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            string? encId = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "EncId")?.Value;
            string? name = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
            string? email = hcontext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string? role = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            string? profile = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "Profile")?.Value;

            UserDto? loggedInUser = null;
            if (userId != null)
            {
                loggedInUser = new UserDto()
                {
                    Id = userId,
                    EncId = encId,
                    Name = name,
                    Email = email,
                    Role = Convert.ToInt32(role),
                    Profile = string.IsNullOrEmpty(profile) ? "" : profile
                };
            }
            
            return loggedInUser;
        }

        public async Task<bool> SetUserClaims(User user)
        {
            try
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim("EncId", StringCipher.EncryptId(user.Id)),
                    new Claim("UserName", user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("Role", user.Role.ToString()),
                    new Claim("Profile", string.IsNullOrEmpty(user.ProfilePicture) ? "" : user.ProfilePicture.ToString()),
                };


                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await hcontext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    principal, 
                    new AuthenticationProperties { IsPersistent = true }
                );

                return true;
            }
            catch 
            {
                return false;
            }
        }

        public static DateTime DateTimeNow()
        {
            return DateTime.Now;
        }

        public static async Task<string> UploadProfilePicture(IFormFile file, string oldProfile = "",
            string folderName = "", string userId = "")
        {
            try
            {
                CreateUserDirectory(userId);
                var fileDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/users/user" + userId);
                if (!string.IsNullOrEmpty(folderName))
                {
                    fileDir = Path.Combine(fileDir, folderName);
                }

                // Generate a unique filename for the uploaded file
                string fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + GeneralPurpose.DateTimeNow().Ticks + Path.GetExtension(file.FileName);

                // Combine the root folder and the generated filename
                string filePath = Path.Combine(fileDir, fileName);

                if (!string.IsNullOrEmpty(oldProfile))
                {
                    await DeleteFile(oldProfile, fileDir);
                }

                if (!await SaveFile(file, filePath))
                {
                    return "Something went wrong";
                }
                string[] getFileUrl = filePath.Split("wwwroot/");
                string actualPath = getFileUrl.Last();

                return actualPath;
                
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public static void CreateUserDirectory(string? Id = "")
        {
            var rootDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var userDir = Path.Combine(rootDir, "users");
            var uIdDir = Path.Combine(userDir, "user" + Id);
            var menusDir = Path.Combine(uIdDir, "menus");

            if (!Directory.Exists(userDir))
                Directory.CreateDirectory(userDir);

            if (!Directory.Exists(uIdDir))
                Directory.CreateDirectory(uIdDir);

            if (!Directory.Exists(menusDir))
                Directory.CreateDirectory(menusDir);
        }

        private static async Task<bool> DeleteFile(string oldProfile, string fileDir)
        {
            try
            {
                string[] getName = oldProfile.Split("/");

                // Check if a profile picture already exists for the user
                string existingProfilePicturePath = Path.Combine(fileDir, getName.Last());
                if (System.IO.File.Exists(existingProfilePicturePath))
                {
                    System.IO.File.Delete(existingProfilePicturePath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> SaveFile(IFormFile file, string filePath)
        {
            try
            {
                var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                var bytes = stream.ToArray();
                stream.Close();

                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Reading XML

        public static List<GeneralNameDto> ReadFromExcel(IFormFile file)
        {
            try
            {
                List<GeneralNameDto> turboChargerList = new List<GeneralNameDto>();

                using (var excelPackage = new ExcelPackage(file.OpenReadStream()))
                {
                    var worksheet = excelPackage.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int i = 2; i <= rowCount; i++)
                    {
                        GeneralNameDto obj = new GeneralNameDto
                        {
                            Name = GetCellValue(worksheet, i, 1),
                        };

                        turboChargerList.Add(obj);
                    }
                }
                return turboChargerList;
            }
            catch (Exception e)
            {
                List<GeneralNameDto> GeneralNameDtoList = null;
                return null;

            }

        }
        private static string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = worksheet.Cells[row, col].Value;
            return cellValue != null ? cellValue.ToString() : null;
        }

        #endregion
    }
}
