using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return View(objCompanyList);
        }
        public IActionResult Upsert(int? id)
        {
            if(id==0 || id==null)
            {
                return View(new Company());
            }   
            else
            {
                Company companyFormDb = _unitOfWork.Company.Get(u => u.Id == id);
                if(companyFormDb==null)
                {
                    return NotFound();
                }    

                return View(companyFormDb);
            }    
           
        }
        [HttpPost]
        public IActionResult Upsert(Company company0bj)
        {
            if(ModelState.IsValid)
            {
                if(company0bj.Id==0)
                {
                    _unitOfWork.Company.Add(company0bj);
                    TempData["success"] = "Company created successfully";
                }   
                else
                {
                    _unitOfWork.Company.Update(company0bj);
                    TempData["success"] = "Company created successfully";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index", "Company", new { area = "Admin" });
                
            }    
            return View(company0bj);
        }




        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();

            return Json(new {data= objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
                return Json(new {success=false,message="Error while deleting"});
            }
            Company companyFormDb = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyFormDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });

            }
            _unitOfWork.Company.Remove(companyFormDb);
            _unitOfWork.Save();
            return Json(new { success = false, message = "Company deleted succefully " });

        }

    }
}
