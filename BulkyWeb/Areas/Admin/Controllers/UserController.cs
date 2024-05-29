using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
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

        
            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                ApplicationUser = _unitOfWork.ApplicationUser.Get(value => value.Id == userId,includeProperties:"Company"),
                RoleList=_roleManager.Roles.Select(u=>new SelectListItem
                {
                    Text=u.Name,
                    Value=u.Name
                }),
                CompanyList=_unitOfWork.Company.GetAll().Select(u=>new SelectListItem
                {
                    Text=u.Name,
                    Value=u.Id.ToString()
                })

            };

            //lấy vai trò của người dùng đăng nhập
            roleManagementVM.ApplicationUser.role=_userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u=>u.Id==userId)).GetAwaiter().GetResult().FirstOrDefault();
            return View(roleManagementVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            if(string.IsNullOrEmpty(roleManagementVM.ApplicationUser.Id))
            {
                return NotFound();
            }


         
            string? oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();
            
            ApplicationUser applicationUser=_unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);

            if (applicationUser == null)
            {
                return NotFound();
            } 
                

            if (!(roleManagementVM.ApplicationUser.role==oldRole))
            {
                //update role
                if(roleManagementVM.ApplicationUser.role==SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                  
                } 
                if(oldRole==SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }

                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save(); 

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();//xóa thông tin trong bảng[AspNetUserRoles]
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.role).GetAwaiter().GetResult(); //thông tin trong bảng[AspNetUserRoles]

            }
            else
            {
                if(oldRole==SD.Role_Company && applicationUser.CompanyId!=roleManagementVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId=roleManagementVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }    
            }    
            TempData["success"] = "Role Update Successfully";
            return RedirectToAction(nameof(Index));
        }

        #region call api all info user
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();

            //duyệt vòng để khởi tạo các user ko phải là comapny tránh trả về null
            
            //danh sách người dùng và vai trò
            //var userRoles = _userManager.Roles.ToList();

            //danh sách vai trò (có 4 vai trò)
            //var roles = _roleManager.Roles.ToList();

            foreach(var user in objUserList)
            {
                user.role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
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
            var objFromDb=_unitOfWork.ApplicationUser.Get(u => u.Id == id);
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
            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save(); 
           
            return Json(new { success=true,message="Operation Successfully" });
        }

        #endregion
    }
}
