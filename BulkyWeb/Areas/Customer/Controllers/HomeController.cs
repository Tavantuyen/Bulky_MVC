using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        //IActionResult interface trong framework .net core
        public IActionResult Index()
        {
            IEnumerable<Product> productList=_unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");

            return View(productList);
        }
        public IActionResult Details(int? productId)
        {
            if(productId == 0 || productId == null)
            {
                return NotFound();
            }

            ShoppingCart cart = new ShoppingCart()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
                Count=1,
                ProductId= (int)productId
            };

            if(cart.Product==null)
            {
                return NotFound();
            }    
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart? shoppingCart)
        {
            // định nghĩ xác thực danh tính .net team tạo ra
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            // tìm user id đang đăng nhập
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            shoppingCart.ApplicationUserId = userId;

            //shoppingCart.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            //shoppingCart.Product = _unitOfWork.Product.Get(u => u.Id==shoppingCart.ProductId);

            ShoppingCart cardFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId==userId && u.ProductId==shoppingCart.ProductId);

            if(cardFromDb!=null)
            {
                //shopping cart exits
                cardFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cardFromDb);
                TempData["success"] = "Update shopping cart successfully";
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
                _unitOfWork.Save();

            }   
            else
            {
                //add shopping cart
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                TempData["success"] = "Add shopping cart successfully";
                _unitOfWork.Save();
                //tạo session số lượng người dùng đã thêm vào card
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
           
            }

           
            //return RedirectToAction("Index","Home",new {area= "Customer" });
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
