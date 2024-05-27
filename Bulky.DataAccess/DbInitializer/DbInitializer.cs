using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DbInitializer(ApplicationDbContext db,UserManager<IdentityUser> userManager,RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager= userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            // add migration if they are not applied
            try
            {
                //lấy ra số lượng các migration đã được khai báo 
                if(_db.Database.GetPendingMigrations().Count()>0)
                {
                    _db.Database.Migrate(); //lưu migartion và create update-datate
                }
            }
            catch(Exception)
            {

            }

            //crate roles if they are not created
            //tạo ra các vai trò admin,customer,company,employee
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();


                //if roles are not created, then we will create admin user as well
                //tạo user
                _userManager.CreateAsync(new ApplicationUser()
                {
                    UserName = "daxua816@gmail.com",
                    Email = "daxua816@gmail.com",
                    Name = "Tạ Văn Tuyên",
                    PhoneNumber = "0367754060",
                    StreetAddress = "123, Gia phúc, Nguyễn Trãi, Thường Tín",
                    State = "OK",
                    PostalCode = "100000",
                    City = "Hà Nội"
                }, "Tatuyen123@").GetAwaiter().GetResult();
                //truy vấn user
                ApplicationUser? user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "daxua816@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult(); //thêm người dùng với vai trò quản trị viên
            }

            return;



        }
    }
}
