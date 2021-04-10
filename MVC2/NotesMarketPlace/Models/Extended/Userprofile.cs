using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketPlace.Models
{
    public class Userprofile
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is Required")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is Required")]
        public string LastName { get; set; }

        public string SecondaryEmailAddress { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public Nullable<int> Gender { get; set; }
        public string Phone_number_Country_Code { get; set; }
        public string Phone_Number { get; set; }
        public string Profile_Picture { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Address is Required")]
        public string Address_Line_1 { get; set; }
        public string Address_Line_2 { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "City is Required")]
        public string City { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "State is Required")]
        public string State { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Zipcode is Required")]
        public string ZipCode { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Country is Required")]
        public string Country { get; set; }
        public string University { get; set; }
        public string College { get; set; }
    }
}