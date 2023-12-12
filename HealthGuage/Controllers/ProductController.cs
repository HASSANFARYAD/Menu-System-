using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Template.HelpingClasses;
using Template.Models;
using Template.Repositories;

namespace Template.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUserRepo _userRepo;
        private readonly IProductRepo _productRepo;
        private readonly GeneralPurpose gp;

        public ProductController(IUserRepo userRepo, IProductRepo productRepo, IHttpContextAccessor haccess)
        {
            _userRepo = userRepo;
            _productRepo = productRepo;
            gp = new GeneralPurpose(haccess);
        }

        public async Task<IActionResult> Index(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetProductDataTableList(string Name = "")
        {
            var getUserId = gp.GetUserClaims();
            var ulist = new List<Product>();

            if (getUserId.Role != 1)
            {
                ulist = (List<Product>)await _productRepo.GetActiveProductList(Convert.ToInt32(getUserId.Id));
            }
            else
            {
                ulist = (List<Product>)await _productRepo.GetActiveProductList();
            }
            

            if (!String.IsNullOrEmpty(Name))
            {
                ulist = ulist.Where(x => x.Name.ToLower().Contains(Name.Trim().ToLower())).ToList();
            }

            int start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            int length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
            string searchValue = Request.Form["search[value]"].FirstOrDefault();
            string sortColumnName = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"];
            string sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            if (sortColumnName != "" && sortColumnName != null)
            {
                if (sortColumnName != "0")
                {
                    if (sortDirection == "asc")
                    {
                        ulist = ulist.OrderByDescending(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                    else
                    {
                        ulist = ulist.OrderBy(x => x.GetType().GetProperty(sortColumnName).GetValue(x)).ToList();
                    }
                }
            }

            int totalrows = ulist.Count();

            //filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                ulist = ulist.Where(x => x.Name != null && x.Name.ToLower().Contains(searchValue.ToLower())
                                        ).ToList();
            }

            int totalrowsafterfilterinig = ulist.Count();


            // pagination
            ulist = ulist.Skip(start).Take(length).ToList();

            List<ProductDto> udto = new List<ProductDto>();

            foreach (Product u in ulist)
            {
                var userName = "";
                var createdby = await _userRepo.GetUserById((int)u.CreatedBy);
                if (Convert.ToInt32(getUserId.Id) == u.CreatedBy)
                {
                    userName = "You";
                }
                else
                {
                    userName = createdby.Name;
                }
                ProductDto obj = new ProductDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                    CreatedBy = createdby != null ? userName : "",
                };

                udto.Add(obj);
            }

            return Json(new { data = udto, draw = Request.Form["draw"].FirstOrDefault(), recordsTotal = totalrows, recordsFiltered = totalrowsafterfilterinig });
        }

        [HttpPost]
        public async Task<IActionResult> PostAddProduct(Product _Product)
        {
            if (string.IsNullOrEmpty(_Product.Name))
            {
                return RedirectToAction("Index", new { msg = "All Fields are Required", color = "green" });
            }
            var getUserId = gp.GetUserClaims();
            Product Product = new Product
            {
                Name = _Product.Name.Trim(),
                IsActive = 1,
                CreatedAt = GeneralPurpose.DateTimeNow(),
                CreatedBy = Convert.ToInt32(getUserId.Id)
            };
            if (!await _productRepo.AddProduct(Product))
            {
                return RedirectToAction("Index", new { msg = "Somethings' Wrong", color = "red" });
            }

            return RedirectToAction("Index", new { msg = "Record Inserted Successfully", color = "green" });
        }

        [HttpPost]
        public async Task<IActionResult> PostUploadProducts(IFormFile ExcelFile)
        {
            try
            {
                var getUserId = gp.GetUserClaims();

                List<GeneralNameDto> ProductList = new List<GeneralNameDto>();
                ProductList = GeneralPurpose.ReadFromExcel(ExcelFile);
                int chkIfAllFieldsAddedCorrectly = 0;
                foreach (GeneralNameDto Obj in ProductList)
                {
                    Product thisProduct = new Product
                    {
                        Name = Obj.Name,
                        IsActive = 1,
                        CreatedAt = GeneralPurpose.DateTimeNow(),
                        CreatedBy = Convert.ToInt32(getUserId.Id)
                    };
                    if (!await _productRepo.AddProduct(thisProduct))
                    {
                        chkIfAllFieldsAddedCorrectly += 1;                 
                    }
                }
                if(chkIfAllFieldsAddedCorrectly > 0)
                {
                    return RedirectToAction("UploadTurboCharger", "TurboCharger", new { msg = "Record Inserted With Errors", color = "red" });

                }
                return RedirectToAction("UploadTurboCharger", "TurboCharger", new { msg = "Records Inserted Successfully", color = "green" });

            }
            catch (Exception ex)
            {
                return RedirectToAction("UploadTurboCharger", "TurboCharger", new { msg = "Invalid Document or Excel Pattern", color = "red" });

            }
        }


        [HttpPost]
        public async Task<IActionResult> PostUpdateProduct(Product _Product)
        {
            Product? Product = await _productRepo.GetProductById(_Product.Id);
            if (Product == null)
            {
                return RedirectToAction("Index", new { msg = "Record not found", color = "red" });
            }
            Product.Name = _Product.Name.Trim();
            Product.UpdatedAt = GeneralPurpose.DateTimeNow();

            if (await _productRepo.UpdateProduct(Product))
            {
                return RedirectToAction("Index", new { msg = "Product updated successfully", color = "green" });
            }
            return RedirectToAction("Index", new { msg = "Somethings' wrong", color = "red" });
        }

        [HttpPost]
        public async Task<IActionResult> GetProductById(string id)
        {
            int ProductId = StringCipher.DecryptId(id);
            Product? u = await _productRepo.GetProductById(ProductId);
            if (u == null)
            {
                return Json(0);
            }

            ProductDto obj = new ProductDto()
            {
                Id = u.Id.ToString(),
                EncId = StringCipher.EncryptId(u.Id),
                Name = u.Name,
            };

            return Json(obj);
        }

        [HttpPost]
        public async Task<IActionResult> GetProductList()
        {
            var getUserId = gp.GetUserClaims();

            var list = new List<Product>();
            if (getUserId.Role == 1)
            {
                list = (List<Product>)await _productRepo.GetActiveProductList();
            }
            else
            {
                list = (List<Product>)await _productRepo.GetActiveProductList(Convert.ToInt32(getUserId.Id));
            }

            List<ProductDto> udto = new List<ProductDto>();

            foreach (Product u in list)
            {
                ProductDto obj = new ProductDto()
                {
                    Id = u.Id.ToString(),
                    EncId = StringCipher.EncryptId(u.Id),
                    Name = u.Name,
                };

                udto.Add(obj);
            }

            return Json(udto);
        }

        public async Task<IActionResult> DeleteProduct(string id)
        {
            int Id = StringCipher.DecryptId(id);

            if (!await _productRepo.DeleteProduct(Id))
            {
                return RedirectToAction("Index", new { msg = "Somethings' wrong", color = "red" });
            }

            return RedirectToAction("Index", new { msg = "Record deleted successfully!", color = "green" });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateName(string name, string id = "")
        {
            if (!string.IsNullOrEmpty(name))
            {
                return Json(await _productRepo.ValidateName(name, Convert.ToInt32(id)));
            }
            else
            {
                return Json("Name cannot be Empty");
            }
        }
    }
}
