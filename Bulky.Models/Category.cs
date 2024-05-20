using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class Category
    {
        // định nghĩa khóa chính
        [Key]
        public int Id { get; set; }

        //định nghĩa để asp-for sẽ hiển thị trên label
        [DisplayName("Category Name")]
        //không được phép null
        [Required]
        //định nghĩa maxlength tối đa kí tự đầu vào
        [MaxLength(30)]
        public string Name { get; set; }


        [DisplayName("Display Order")]
        //định nghĩa phạm vi giá trị đầu vào
        [Range(1, 100, ErrorMessage = "Display Order must be between 1-100")]

        public int DisplayOrder { get; set; }

    }
}
