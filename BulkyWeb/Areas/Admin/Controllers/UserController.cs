using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        public UserController(ApplicationDbContext db,UserManager<IdentityUser> userManager)
        {
            _db= db;
            _userManager= userManager;
        }
        public IActionResult Index()
        {
            
            return View();
        }


        public IActionResult RoleManagement(string? userId)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }    

            string RoleId=_db.UserRoles.FirstOrDefault(u=>u.UserId==userId).RoleId;

            if(RoleId==null)
            {
                return NotFound();
            }

            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                ApplicationUser = _db.ApplicationUsers.Include(u=>u.Company).FirstOrDefault(value => value.Id == userId),
                RoleList=_db.Roles.Select(u=>new SelectListItem
                {
                    Text=u.Name,
                    Value=u.Name
                }),
                CompanyList=_db.Conpanies.Select(u=>new SelectListItem
                {
                    Text=u.Name,
                    Value=u.Id.ToString()
                })

            };
            roleManagementVM.ApplicationUser.role=_db.Roles.FirstOrDefault(u=>u.Id==RoleId).Name;
            return View(roleManagementVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            if(string.IsNullOrEmpty(roleManagementVM.ApplicationUser.Id))
            {
                return NotFound();
            }

            string? RoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            if(RoleId==null)
            {
                return NotFound();
            }
            string? oldRole = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;

            if(!(roleManagementVM.ApplicationUser.role==oldRole))
            {
                //update role
                ApplicationUser applicationUser=_db.ApplicationUsers.FirstOrDefault(u=>u.Id==roleManagementVM.ApplicationUser.Id);

                if(roleManagementVM.ApplicationUser.role==SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                  
                } 
                if(oldRole==SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }

                _db.ApplicationUsers.Update(applicationUser);
                _db.SaveChanges();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();//xóa thông tin trong bảng[AspNetUserRoles]
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.role).GetAwaiter().GetResult(); //thông tin trong bảng[AspNetUserRoles]


            }
            TempData["success"] = "Role Update Successfully";
            return RedirectToAction(nameof(Index));
        }

        #region call api all info user
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u=>u.Company).ToList();

            //duyệt vòng để khởi tạo các user ko phải là comapny tránh trả về null
            
            //danh sách người dùng và vai trò
            var userRoles = _db.UserRoles.ToList();

            //danh sách vai trò (có 4 vai trò)
            var roles = _db.Roles.ToList();

            foreach(var user in objUserList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                if(user.Company==null)
                {
                    user.Company=new Company() { Name=""};
                }    
            }
            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string?id)
        {
            var objFromDb=_db.ApplicationUsers.FirstOrDefault(u=>u.Id==id);
            if(objFromDb==null)
            {
                return Json(new { success = false ,message="Error while Locking/Unlocking"});
            }    
            //kiểm tra lock 
            if(objFromDb.LockoutEnd!=null && objFromDb.LockoutEnd>DateTime.Now)
            {
                //người dùng đang bị khóa tài khoản =>> cần mở khóa tài khoản
                objFromDb.LockoutEnd = DateTime.Now;
            }    
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000); //khóa đến chết
            }
            _db.ApplicationUsers.Update(objFromDb);
            _db.SaveChanges(); 
           
            return Json(new { success=true,message="Operation Successfully" });
        }

        #endregion
    }
}
