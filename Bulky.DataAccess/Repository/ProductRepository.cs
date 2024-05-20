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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        public void Update(Product obj)
        {
            Product? objFormDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            if(objFormDb!=null)
            {
                objFormDb.Title = obj.Title;
                objFormDb.ISBN=obj.ISBN;
                objFormDb.Price = obj.Price;
                objFormDb.ListPrice=obj.ListPrice;
                objFormDb.Price50=obj.Price50;
                objFormDb.Price100=obj.Price100;
                objFormDb.CategoryId=obj.CategoryId;
                objFormDb.Description = obj.Description;
                objFormDb.Author=obj.Author;
                if(obj.ImageUrl!=null)
                {
                    objFormDb.ImageUrl = obj.ImageUrl;
                }
                _db.Products.Update(objFormDb);
            }    
            

            // _db.Products.Update(objFormDb);
            // không cần vì Entity framework nó tự theo dõi hành động 
        }
    }
}
