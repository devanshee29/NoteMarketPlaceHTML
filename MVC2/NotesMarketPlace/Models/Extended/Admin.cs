using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NotesMarketPlace.Models
{
    public class Memberdetails
    {


        public SellerNote note { get; set; }

        public SellerNotesAttachment attachment { get; set; }

        public Download downloads { get; set; }
        public NoteCategory category { get; set; }
        public ReferenceData status { get; set; }
    }
    public class Memberbasicdetail
    {
        public User user { get; set; }
        public UserProfile userprofile { get; set; }

        public IEnumerable<Memberdetails> memberdetails { get; set; }
    }
  
}   