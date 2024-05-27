using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get;set; } //khóa ngoại User
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        
        public DateTime OrderDdate { get; set; } //OrderDdate ngày đặt hàng
        public DateTime ShipingDate { get; set; } //ngày giao hàng
        public double OrderTotal { get; set; }  //tổng giá trị đơn hàng

        public string? OrderStatus { get; set; } //trạng thái đơn hàng
        public string? PaymentStatus { get; set; } //trạng thái thanh toán 
        public string? TrackingNumber { get; set; } //số lượng đơn hàng
        public string? Carrier { get; set; } //thông tin vận chuyển
    
        public DateTime PaymentDate { get; set; } //ngày thanh toán
        public DateOnly PaymentDueDate { get; set; }  // thanh toán đáo hạn cuối

        //DateOnly là 1 tính năng trong .net8 mà các version cũ ko có
        //DateOnly chỉ có ngày tháng năm, ko lưu thời gian

       public string? SessionId { get; set; } //id phiên làm việc 
        public string? PaymentIntentId { get; set;} //mã thẻ id thanh toán online


        [Required]
        public string Name { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }
}
