using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NotesMarketPlace.Models
{
    public class AddCountry
    {
        [Display(Name = "Country Name *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Country name is Required")]
        public string CountryName { get; set; }

        [Display(Name = "Country Code *")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Country Code is Required")]
        public string CountryCode { get; set; }
    }
}