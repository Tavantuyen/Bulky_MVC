using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    //[BindProperties] //ràng buộc tất cả các dữ liệu code bên trang Create.cshtml
    public class CreateModel : PageModel
    {
        [BindProperty] //ràng buộc thuộc tính Category category bên trang Create.cshtml
        public Category category { get; set; }
        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet()
        {
            
        }
        
        public IActionResult OnPost()
        {
           if(category == null)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
                TempData["success"] = "Create category successfully";
                return RedirectToPage("Index","Categories");
            }    

            return Page();
        }
        
    }
}
