using BookWeb.DataAccess.Repository.IRepository;
using BookWeb.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;

            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                        _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId.Value).Count());
                }

                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }
            
            // logout, ...
            HttpContext.Session.Clear();
            return View(0);
        }
    }
}
