using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketPlace.Models
{
    public class AddCategory
    {
        [Display(Name = "Category Name *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Category name is Required")]
        public string CategoryName { get; set; }

        [Display(Name = "Description *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Description is Required")]
        public string Description { get; set; }
    }
}