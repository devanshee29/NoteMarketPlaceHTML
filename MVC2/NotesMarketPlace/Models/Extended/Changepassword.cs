using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketPlace.Models
{
    public class Changepassword
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is Required")]
        [MinLength(6, ErrorMessage = "Minimum 6 characters Required")]
        public string OldPassword { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is Required")]
        [MinLength(6, ErrorMessage = "Minimum 6 characters Required")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirm password and NewPassword do not match")]
        public string ConfirmPassword { get; set; }
    }
}