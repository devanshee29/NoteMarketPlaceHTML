using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketPlace.Models
{
    public class Addadministrator
    {
        [Display(Name = "First Name *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is Required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is Required")]
        public string LastName { get; set; }

        [Display(Name = "Email *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email is Required")]
        public string EmailID { get; set; }

        public string Phone_number_Country_Code { get; set; }

        public string Phone_Number { get; set; }
    }
}