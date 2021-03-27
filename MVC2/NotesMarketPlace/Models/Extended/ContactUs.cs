using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketPlace.Models
{
    public class ContactUs
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Full name is Required")]
        public string FullName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email is Required")]
        public string EmailID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Subject is Required")]
        public string Subject { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Comment is Required")]
        public string Comment { get; set; }
    }
}