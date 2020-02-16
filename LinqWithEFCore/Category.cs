using System.ComponentModel.DataAnnotations;

namespace LinqWithEFCore
{
    public class Category
    {
        public int CategoryID { get; set; }
        
        
        [Required]
        [StringLength(15)]
        public string CategoryName { get; set; }
        
        
        public string Description { get; set; }
    }
}