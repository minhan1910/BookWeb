using BookWeb.DataAccess.Repository.IRepository;
using BookWeb.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Utility.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Areas.Admin.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> categoryList = _unitOfWork.Category.GetAll()
              .Select(u => new SelectListItem()
              {
                  Text = u.Name,
                  Value = u.Id.ToString(),
              });            

            var productVm = new ProductVM()
            {
                Product = new(),
                CategoryList = categoryList
            };

            if (!id.HasValue || id == 0)
            {
                // create
                return View(productVm);
            }

            Product? product = _unitOfWork.Product.Get(x => x.Id == id.Value);

            if (product == null)
            {
                return BadRequest();
            }

            if (product.ImageUrl != null)
            {
                ViewBag.ProductUrl = FileHelper.NormalizeFilePathToWebRootPath(product.ImageUrl);
            }

            productVm.Product = product;

            return View(productVm);
        }   

        [HttpPost]
        public IActionResult Upsert(ProductVM productVm, IFormFile? file)
        {
            if (!ModelState.IsValid) 
            {
                productVm.CategoryList = _unitOfWork.Category.GetAll()
                    .Select(x => new SelectListItem()
                    {
                        Text= x.Name,
                        Value = x.Id.ToString(),
                    });               

                return View(productVm);
            }

            if (productVm == null)
            {
                return BadRequest();
            }

            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\product");

                if (!string.IsNullOrEmpty(productVm.Product.ImageUrl))
                {
                    // delete the old image
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productVm.Product.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                }

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productVm.Product.ImageUrl = @"images\product\" + fileName;
            
            }

            // Create
            if (productVm.Product.Id == 0)
            {                

                _unitOfWork.Product.Add(productVm.Product);
                _unitOfWork.Save();

                TempData["success"] = "Product created successfully";
                
                return RedirectToAction("Index");
            }

            Product product = productVm.Product;

            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();

            TempData["success"] = "Product edited successfully";

            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(int id)
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var productToDeleted = _unitOfWork.Product.Get(u => u.Id == id.Value);

            if (productToDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath =
                Path.Combine(_webHostEnvironment.WebRootPath,
                             productToDeleted.ImageUrl!.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(productToDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleting successfully" });
        }

        #endregion
    }
}
