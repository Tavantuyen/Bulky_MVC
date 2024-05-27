using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        public void UpdateStatus(int? id, string? orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if(orderFromDb!=null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if(!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }    
                _db.OrderHeaders.Update(orderFromDb);
            }    
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymenIntentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderFromDb != null)
            {
               if(!string.IsNullOrEmpty(sessionId))
               {
                    orderFromDb.SessionId = sessionId;
               } 
               if(!string.IsNullOrEmpty(paymenIntentId))
               {
                    orderFromDb.PaymentIntentId = paymenIntentId;
                    orderFromDb.PaymentDate = DateTime.Now;
               }
                _db.OrderHeaders.Update(orderFromDb);
            }
        }
    }
}
