using BookWeb.DataAccess.Repository.IRepository;
using BookWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookWeb.Areas.Admin.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            List<Category> categories = _unitOfWork.Category.GetAll().ToList();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (!ModelState.IsValid)
            {
                return View(ModelState);
            }

            _unitOfWork.Category.Add(obj);
            _unitOfWork.Save();

            TempData["success"] = "Category created successfully";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || !id.HasValue)
            {
                return NotFound();
            }

            Category? category = _unitOfWork.Category.Get(u => u.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (!ModelState.IsValid)
            {
                return View(ModelState);
            }

            Category? category = _unitOfWork.Category.Get(u => u.Id == obj.Id);

            if (category == null)
            {
                return NotFound();
            }

            // Update
            category.Name = obj.Name;
            category.DisplayOrder = obj.DisplayOrder;

            _unitOfWork.Category.Update(category);
            _unitOfWork.Save();

            TempData["success"] = "Category edited successfully";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || !id.HasValue)
            {
                return NotFound();
            }

            Category? category = _unitOfWork.Category.Get(u => u.Id == id);

            if (category == null)
            {
                return NotFound();
            }



            return View(category);
        }

        [HttpPost]
        public IActionResult Delete(Category obj)
        {
            int? id = obj.Id;

            if (id == null || !id.HasValue)
            {
                return NotFound();
            }

            Category? category = _unitOfWork.Category.Get(u => u.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();

            TempData["success"] = "Category deleted successfully";

            return RedirectToAction("Index");
        }
    }
}
