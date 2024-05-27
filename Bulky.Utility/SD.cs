using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Company = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        //trạng thái thanh toán cho khách hàng
        public const string StatusPending = "Pending"; //chờ giải quyết
        public const string StatusApproved = "Approved"; //tán thành
        public const string StatusInProcess = "Processing"; //đang xử lý
        public const string StatusShipped = "Shipped"; //đang giao
        public const string StatusCancelled = "Cancelled"; //hủy đơn hàng
        public const string StatusRefunded = "Refunded"; //hoàn hàng
        
        //trạng thái thanh toán cho nhân viên công ty
        public const string PaymentStatusPending = "Pending"; //chờ giải quyết
        public const string PaymentStatusApproved = "Approved"; //tán thành
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment"; //được phê duyệt thanh toán trả sau
        public const string PaymentStatusRejected = "Rejected"; //Loại bỏ

        public const string SessionCart = "SessionShoppingCart";
    }
}
