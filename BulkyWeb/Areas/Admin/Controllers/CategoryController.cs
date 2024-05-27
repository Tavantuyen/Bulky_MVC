
using BulkyBook.Models;
using BulkyBook.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // có thể áp dụng cho từng các acction cũng đc
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }
        #region Create
        public IActionResult Create()
        {
            
            //return View(new Category()); //có thể khai báo như vậy 
            return View(); //model sẽ tự khởi tạo 1 đối tượng
        }
        [HttpPost]
        public IActionResult Create(Category? obj)
        {
            if(!string.IsNullOrEmpty(obj.Name) &&  obj.Name==obj.DisplayOrder.ToString())
            {
                //Key là tên thuộc tính Category cần xác định và value là nội dung
                ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name"); 
                ModelState.AddModelError("DisplayOrder", "The DisplayOrder cannot exactly match the Name");
            }
            //(ModelState.IsValid sẽ đi đến Category để kiểm tra các trường yêu cầu (length,range,required)
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Create Category Successfully";
                return RedirectToAction("Index", "Category", new { area = "Admin" });
            }
            return View();
        }
        #endregion
        #region update
        public IActionResult Edit(int? id)
        {
            if(id==0 || id==null)
            {
                return NotFound();
            }
            //Category? categoryFromDb = _db.Categories.FirstOrDefault(u => u.Id == id);
            //Category? categoryFromDb = _db.Categories.Find(id);
            Category? categoryFromDb = _unitOfWork.Category.GetAll().Where(u => u.Id == id).FirstOrDefault();
            if (categoryFromDb == null)
            {
                return NotFound();
            }    

            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category? obj)
        {
            //(ModelState.IsValid sẽ đi đến Category để kiểm tra các trường yêu cầu (length,range,required)
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Update Category Successfully";
                return RedirectToAction("Index", "Category",new {area="Admin" });
            }
            return View();
        }
        #endregion

        #region delete
        public IActionResult Delete(int? id)
        {

            if (id == 0 || id == null)
            {
                return NotFound();
            }
            //Category? categoryFromDb = _db.Categories.FirstOrDefault(u => u.Id == id);
            //Category? categoryFromDb = _db.Categories.Find(id);
            Category? categoryFromDb = _unitOfWork.Category.GetAll().Where(u => u.Id == id).FirstOrDefault();
            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }
        [HttpPost,ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Category? categoryFormDb = _unitOfWork.Category.Get(u=>u.Id==id);
            if(categoryFormDb==null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(categoryFormDb);
            _unitOfWork.Save();
            TempData["success"] = "Delete Category Successfully";
            return RedirectToAction("Index", "Category", new { area = "Admin" });
        }
        #endregion
    }
}
