using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        //tiêm phụ thuộc để truy cập vào thư mục www
        private readonly IWebHostEnvironment _webHostEnvironment;
        //tiêm phụ thuộc để truy cập vào thư mục www

        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return View(objProductList);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().
                Select(u => new SelectListItem
                {
                    Text=u.Name,
                    Value=u.Id.ToString()
                })
            };
            if (id == 0 || id == null)
            {
                //create    

                return View(productVM);
                //return RedirectToPage("Index","Product",new {area="Admin"});
            }   
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                if(productVM.Product==null)
                {
                    return NotFound();
                }    

                return View(productVM);
            }    
           
        }
        [HttpPost]   
        
        public IActionResult Upsert(ProductVM productVM,IFormFile? file)
        {
            
            if(ModelState.IsValid)
            {
                // đường dẫn đang đứng ở wwwroot
                //"C:\\Users\\Mycomputer\\source\\repos\\Bulky\\BulkyWeb\\wwwroot"
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    //Path.GetExtension(file.FileName); lấy phần mở rộng tệp bao gồm cả dấu chấm

                    // tạo ra chuỗi ngẫu nhiên có 128bit
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    //Path.Combine(wwwRootPath,@"images\product"); gộp đường dẫn lại với nhau
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //delete file image cũ
                        var oldImage = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        //sao chép file vào thư mục wwwroot/product
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                    
                    if (productVM.Product.Id == 0)
                    {
                        _unitOfWork.Product.Add(productVM.Product);
                        TempData["success"] = "Create product successfully";
                        _unitOfWork.Save();
                    }
                    else
                    {
                        _unitOfWork.Product.Update(productVM.Product);
                        TempData["success"] = "Update product successfully";
                        _unitOfWork.Save();
                    }
                    return RedirectToAction("Index", "Product", new { area = "Admin" });
                }
                else
                {
                    if (productVM.Product.Id == 0)
                    {
                        productVM.Product.ImageUrl = "";
                        _unitOfWork.Product.Add(productVM.Product);
                        TempData["success"] = "Create product successfully";
                        _unitOfWork.Save();
                    }
                    else
                    {
                        _unitOfWork.Product.Update(productVM.Product);
                        TempData["success"] = "Update product successfully";
                        _unitOfWork.Save();
                    }
                }

                return RedirectToAction("Index", "Product", new { area = "Admin" });
            }   
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().
                Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                return View(productVM);
            }    

            
        }
        #region Create_Update
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().
            Select(u => new SelectListItem
            {
                Text = u.Name, //Text là nội dung hiển thị
                Value = u.Id.ToString() //Value là giá trị nhận được khi chọn text

            });

            //ViewBag.Key=Value;
            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"] = CategoryList;
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = CategoryList
            };
            return View(productVM);

        }
        [HttpPost]
        public IActionResult Create(ProductVM? productVM)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product create successfully";
                return RedirectToAction("Index", "Product", new { area = "Admin" });
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().
                Select(u => new SelectListItem
                {
                    Text = u.Name, //Text là nội dung hiển thị
                    Value = u.Id.ToString() //Value là giá trị nhận được khi chọn text

                });

                return View(productVM);
            }


        }
        public IActionResult Edit(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            Product? productFormDb = _unitOfWork.Product.Get(u => u.Id == id);
            if (productFormDb == null)
            {
                return NotFound();
            }
            return View(productFormDb);
        }
        [HttpPost]
        public IActionResult Edit(Product obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Product update successfully";

                return RedirectToAction("Index", "Product", new { area = "Admin" });

            }
            return View();
        }
        #endregion

        #region Delete
        //public IActionResult Delete(int? id)
        //{
        //    if (id == 0 || id == null)
        //    {
        //        return NotFound();
        //    }
        //    Product? productFormDb = _unitOfWork.Product.Get(u => u.Id == id);
        //    if (productFormDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productFormDb);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{
        //    Product? obj = _unitOfWork.Product.Get(u => u.Id == id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Product.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product delete successfully";
        //    return RedirectToAction("Index", "Product", new { area = "Admin" });

        //}
        #endregion


        #region API CALLS

        //hoặc dùng public JsonResult GetAll() 
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return Json(new { data = objProductList });
        }
        #endregion

        [HttpDelete]
        public IActionResult Delete(int?id)
        {
            var productTobeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if(productTobeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            if(!string.IsNullOrEmpty(productTobeDeleted.ImageUrl))
            {
                string wwwrootPath = _webHostEnvironment.WebRootPath;
                var oldImage = Path.Combine(wwwrootPath, productTobeDeleted.ImageUrl.TrimStart('\\'));

                if(System.IO.File.Exists(oldImage))
                {
                    System.IO.File.Delete(oldImage);
                }

            }

            _unitOfWork.Product.Remove(productTobeDeleted);
            _unitOfWork.Save();

            return Json(new {success=true,message="Product delete successfully"});
        }
    }
}
