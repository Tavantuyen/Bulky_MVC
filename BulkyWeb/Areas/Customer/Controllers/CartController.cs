using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Linq;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork,IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };


            if (ShoppingCartVM.ShoppingCartList != null)
            {
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = GetPriceBaseOnQuatity(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }
            }

            return View(ShoppingCartVM);
        }

        [HttpGet]
        public IActionResult Summary()
        {
            //khai báo nhận dạng người dùng do .net team tạo ra code này
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            //lấy ra id người dùng đang đăng nhập
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            //ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId); 
            //truy vấn đoạn này ko bao giờ null vì phải đăng nhập mới vào được Summary do tính xác thực
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            if (ShoppingCartVM.ShoppingCartList != null)
            {
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = GetPriceBaseOnQuatity(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }
            }

            return View(ShoppingCartVM);
        }



        //[HttpPost]
        //[ActionName("Summary")]
        //hoặc
        [HttpPost, ActionName("Summary")]
        public IActionResult SummaryPost() //ShoppingCartVM shoppingCartVM được tham chiếu từ binding public ShoppingCartVM ShoppingCartVM { get; set; }
        {
            //khai báo nhận dạng người dùng do .net team tạo ra code này
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            //lấy ra id người dùng đang đăng nhập
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDdate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            //ko gọi ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId); 
            //truy vấn đoạn này ko bao giờ null vì phải đăng nhập mới vào được Summary do tính xác thực
            //mà gọi
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            //vì _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader); sẽ lỗi tạo thêm cột trong trong bảng OrderHeader.ApplicationUser

            if (applicationUser != null)
            {
                applicationUser.Name = ShoppingCartVM.OrderHeader.Name;
                applicationUser.StreetAddress = ShoppingCartVM.OrderHeader.StreetAddress;
                applicationUser.City = ShoppingCartVM.OrderHeader.City;
                applicationUser.State = ShoppingCartVM.OrderHeader.State;
                applicationUser.PostalCode = ShoppingCartVM.OrderHeader.PostalCode;
                applicationUser.PhoneNumber = ShoppingCartVM.OrderHeader.PhoneNumber;
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();
            }

            if (ShoppingCartVM.ShoppingCartList != null)
            {
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = GetPriceBaseOnQuatity(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //nếu CompanyId.GetValueOrDefault()==0 thì => người dùng đó ko phải nhân viên công ty    (vì nếu là nhân viên công ti thì id sẽ khác 0)
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //là người dùng với vai trò nhân viên công ti
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            if (ShoppingCartVM.ShoppingCartList != null)
            {
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVM.OrderHeader.Id, //id được lấy từ _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader) sau khi _unitOfWork.Save();
                        Price = cart.Price,// giá từ sản phẩm
                        Count = cart.Count // số lượng từng sản phẩm
                    };
                    _unitOfWork.OrderDetail.Add(orderDetail);
                    _unitOfWork.Save();
                }
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //nếu CompanyId.GetValueOrDefault()=0 thì => người dùng đó ko phải nhân viên công ty    (vì nếu là nhân viên công ti thì id sẽ khác 0)
                //trường hợp khách hàng là nhân viên công ti cần thanh toán luôn ko nợ
                //stripe login

                var domain = "https://localhost:7275/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {

                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}", //điều hướng khi thanh toán thành công
                    CancelUrl = domain + $"Customer/Cart/Index", //điều hướng khi thanh toán thất bại
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment", //chế độ thanh toán mặc định
                };

                if(ShoppingCartVM.ShoppingCartList!=null)
                {
                    foreach (var item in ShoppingCartVM.ShoppingCartList)
                    {
                        var sessionLineItem = new SessionLineItemOptions()
                        {
                            PriceData = new SessionLineItemPriceDataOptions() //khởi tạo đối tượng dữ liệu giá tùy chọn...
                            {
                                UnitAmount = (long)(item.Price * 100), //ví dụ $20.50=@2050
                                Currency = "usd",//xác định loại tiền tệ
                                ProductData = new SessionLineItemPriceDataProductDataOptions() //khởi tạo thông tin sản phẩm
                                {
                                    Name = item.Product.Title,
                                    Description = item.Product.Description
                                }
                            },
                            Quantity = item.Count //số lượng sản phẩm
                        };
                        options.LineItems.Add(sessionLineItem);
                    }
                }    

                var service = new Stripe.Checkout.SessionService(); //khởi tạo ra 1 session giao dịch
                Session session=service.Create(options); //tạo ra 1 session giao dịch và đợi kết quả trả về biến session
                //session trả về id,PaymentIntentId

                _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                //chuyển hướng đến stripe để thực hiện thanh toán
                Response.Headers.Add("Location",session.Url); //session.Url được cấp bởi stripe

                return new StatusCodeResult(303);

            }
            //tham sô 1 là action , tham số 2 là id của IActionResult OrderConfirmation(int? id)
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }



        public IActionResult OrderConfirmation(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }

            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            
            if(orderHeader==null)
            {
                return NotFound();
            }
            
            if(orderHeader.PaymentStatus!=SD.PaymentStatusDelayedPayment)
            {
                
                if(orderHeader.SessionId!=null)
                {
                    //nếu khác thì thì sẽ là của customer 
                    var service = new SessionService(); //service là 1 lớp thuộc api stripe package
                    Session session = service.Get(orderHeader.SessionId);

                    //check kiểm tra trạng thái
                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeader.Id, session.Id, session.PaymentIntentId);
                        _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusApproved, SD.PaymentStatusApproved);
                        _unitOfWork.Save();
                       
                    }
                    HttpContext.Session.Clear();

                }
                
            }

            if(orderHeader.Id!=0)
            {
                _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky", $"<p> New Order Craete - {orderHeader.Id} Successfully </p>");

            }

            IEnumerable<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId);

            if(shoppingCarts.ToList().Count > 0)
            {
                _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts); //xóa dữ liệu sau bảng shoppingcart sau khi đã thanh toán          
                _unitOfWork.Save();
            }    

            return View(id);
        }


        [HttpGet]
        public IActionResult Plus(int? cartId)
        {
            if (cartId == 0 || cartId == null)
            {
                return NotFound();
            }
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb == null)
            {
                return NotFound();
            }

            if (cartFromDb.Count >= 1000)
            {
                cartFromDb.Count = 999;
            }
            else
            {
                cartFromDb.Count += 1;
            }
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Minus(int? cartId)
        {
            if (cartId == 0 || cartId == null)
            {
                return NotFound();
            }
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb == null)
            {
                return NotFound();
            }



            if (cartFromDb.Count <= 1)
            {
                //remove that from cart
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public IActionResult Remove(int? cartId)
        {
            if (cartId == 0 || cartId == null)
            {
                return NotFound();
            }
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb == null)
            {
                return NotFound();
            }
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));

        }



        private double GetPriceBaseOnQuatity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count < 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
