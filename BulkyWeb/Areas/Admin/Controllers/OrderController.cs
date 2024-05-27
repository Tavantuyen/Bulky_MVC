using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int? orderId)
        {
            //orderId đây là id của OrderHeader
            if (orderId==0 || orderId==null)
            {
                return NotFound();
            }

            OrderVM= new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, "ApplicationUser"),
                OrderDetaislList = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeader.Id==orderId, "Product")
            };

            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+ SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            OrderHeader orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id, "ApplicationUser");   
            if(orderHeaderFromDb==null)
            {
                return NotFound();
            }

            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            if(!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                //theo dõi vận chuyển
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                //theo dõi số lượng
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Details Update Successfully";

            return RedirectToAction(nameof(Details),new { orderId = orderHeaderFromDb.Id});
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id,SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Update Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader=_unitOfWork.OrderHeader.Get(u=>u.Id== OrderVM.OrderHeader.Id,includeProperties: "ApplicationUser");
            if(orderHeader==null)
            {
                return NotFound();
            }


            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier=OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShipingDate = DateTime.Now;

            if(orderHeader.PaymentStatus==SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)); //chuyển từ datetime về dateonly
            }    

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order Shiped Update Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            if (orderHeader == null)
            {
                return NotFound();
            }
            if(orderHeader.PaymentStatus==SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions()// hàm của api stripe
                {
                    Reason = RefundReasons.RequestedByCustomer, //Reason là lý do hoàn tiền?
                    PaymentIntent=orderHeader.PaymentIntentId,//cung cấp id thanh toán
                };
                var service=new RefundService(); //hàm refundService của api stripe
                Refund refund=service.Create(options); //hoàn  lãi đã được xử lý

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id,SD.StatusCancelled,SD.StatusRefunded);
           
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ActionName("Details")]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult Details_PAY_NOW()
        {

            OrderVM.OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id, "ApplicationUser");
            OrderVM.OrderDetaislList = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, "Product");

            //stripe logic
            var domain = "https://localhost:7275/";
            var options = new Stripe.Checkout.SessionCreateOptions()
            {
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl=domain+$"Admin/Order/Details?orderId={OrderVM.OrderHeader.Id}",
                LineItems=new List<SessionLineItemOptions>(),
                Mode="payment"
            };

            if(OrderVM.OrderDetaislList!=null)
            {
                foreach(var item in OrderVM.OrderDetaislList)
                {
                    var sessionLineItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount=(long)(item.Price*100),
                            Currency="usd",
                            ProductData=new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name=item.Product.Title,
                                Description=item.Product.Description
                            }
                        },
                        Quantity=item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }    
            }

            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
       
            //chuyển hướng đến stripe để thực hiện thanh toán
            Response.Headers.Add("Location", session.Url); //session.Url được cấp bởi stripe
            return new StatusCodeResult(303);

        }

        [HttpGet]
        public IActionResult PaymentConfirmation(int? orderHeaderId)
        {
            if(orderHeaderId==0 || orderHeaderId==null)
            {
                return NotFound();
            }    
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId, includeProperties: "ApplicationUser");
            if(orderHeader==null)
            {
                return NotFound();
            }    

            if(orderHeader.PaymentStatus==SD.PaymentStatusDelayedPayment)
            {
                //đây sẽ là trường hợp nhân viên thuộc công ti phải thanh toán
                var service=new Stripe.Checkout.SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if(session.PaymentStatus.ToLower()=="paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, orderHeader.OrderStatus,SD.PaymentStatusApproved);
                    _unitOfWork.Save();

                }    
            }


            //trả về  CancelUrl=domain+$"Admin/Order/Details?orderId={OrderVM.OrderHeader.Id}",
            return View(orderHeader.Id);
        }

        #region  CALL API
        [HttpGet]
        public IActionResult GetAll(string? status)
        {

  
           
            IEnumerable<OrderHeader> objOrderHeaders = null;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsidentity = (ClaimsIdentity)User.Identity;
                var userid = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userid, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "inProcess":
                    {
                        objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);  //tìm các đơn đang xử lý
                        break;
                    }
                case "paymentPending":
                    {
                        objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment); //tìm các đơn được gia hạn thanh toán
                        break;
                    }
                case "completed":
                    {
                        objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);  //tìm các đơn đang giao
                        break;
                    }
                case "approved":
                    {
                        objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved); //tìm các đơn đã chấp nhận
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return Json(new { data = objOrderHeaders });


        }
        #endregion
    }
}
