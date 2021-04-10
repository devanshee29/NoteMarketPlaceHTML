using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketPlace.Models
{
    public class AddType
    {
        [Display(Name = "Type Name *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Type name is Required")]
        public string TypeName { get; set; }

        [Display(Name = "Description *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Description is Required")]
        public string Description { get; set; }
    }
}