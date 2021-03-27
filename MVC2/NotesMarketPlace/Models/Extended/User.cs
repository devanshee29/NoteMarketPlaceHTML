using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketPlace.Models
{


    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserMetadata
    {
        [Display(Name = "First Name *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is Required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is Required")]
        public string LastName { get; set; }

        [Display(Name = "Email *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email is Required")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is Required")]
        [MinLength(6, ErrorMessage = "Minimum 6 characters Required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password and Password do not match")]
        public string ConfirmPassword { get; set; }



    }
}
