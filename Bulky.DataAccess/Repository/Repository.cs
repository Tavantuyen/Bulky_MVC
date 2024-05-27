using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
            //_db.Categories==dbSet; giống cái bên dưới
            //_db.Categories.Add()==dbSet.Add();


            //lấy các dữ liệu Product bao gồm cả quan hệ với bảng khác
            //_db.Products.Include(u => u.Category); 
        }
        public void Add(T entity)
        {
            //throw new NotImplementedException();
            dbSet.Add(entity);
        }

        public T Get(System.Linq.Expressions.Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            ///IQueryable<T> query = dbSet; chuỗi truy vấn không thực thi dựa trên toàn bộ tập hợp dữ liệu được quản lý bởi dbSet. 
            ///Điều này cho phép bạn tiếp tục xây dựng và tùy chỉnh truy vấn của mình thông qua 
            ///các phương thức LINQ, như Where(), OrderBy(), Select()
            ///giống   Category? categoryFromDb =_db.Categories.Where(u => u.Id == id).FirstOrDefault();
            ///
            IQueryable<T> query;
            if (tracked)
            {
                query = dbSet; //theo dõi thực thể, ko cần dùng update vẫn cập nhập
            }    
            else
            {
                query = dbSet.AsNoTracking(); // ko theo dõi entity , phải dùng update để cập nhập
            }
            
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();


        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter=null, string? includeProperties=null)
        {
            IQueryable<T> query = dbSet;
            if(filter != null)
            {
                query = query.Where(filter);
            }    
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeprop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }
            return query.ToList();
            //Hoặc dùng
            //return (IEnumerable<T>)_db.Products.Include(u=>u.Category).ToList();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}
