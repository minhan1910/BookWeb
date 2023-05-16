using BookWeb.DataAccess.Repository.IRepository;
using BookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Areas.Admin.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Company> Companys = _unitOfWork.Company.GetAll().ToList();

            return View(Companys);
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

            if (!id.HasValue || id == 0)
            {
                // create
                return View(new Company());
            }

            // Update

            Company? Company = _unitOfWork.Company.Get(x => x.Id == id.Value);

            if (Company == null)
            {
                return BadRequest();
            }

            return View(Company);
        }   

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (!ModelState.IsValid) 
            {
                return View(new Company());
            }

            if (company == null)
            {
                return BadRequest();
            }

            // Create
            if (company.Id == 0)
            {                

                _unitOfWork.Company.Add(new Company());
                _unitOfWork.Save();

                TempData["success"] = "Company created successfully";
                
                return RedirectToAction("Index");
            }

        
            _unitOfWork.Company.Update(company);
            _unitOfWork.Save();

            TempData["success"] = "Company edited successfully";

            return RedirectToAction("Index");
        }
      
        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(int id)
        {
            List<Company> Companys = _unitOfWork.Company.GetAll().ToList();

            return Json(new { data = Companys });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var CompanyToDeleted = _unitOfWork.Company.Get(u => u.Id == id.Value);

            if (CompanyToDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Company.Remove(CompanyToDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleting successfully" });
        }

        #endregion
    }
}
