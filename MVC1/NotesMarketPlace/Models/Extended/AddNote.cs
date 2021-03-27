using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NotesMarketPlace.Models
{
    /*[MetadataType(typeof(SellerNoteMetadata))]
    public partial class SellerNote
    {
        public int NoteID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }*/

   /* [MetadataType(typeof(SellerNoteMetadata))]
    public partial class SellerNotesAttachment
    {
    }*/
    public class AddNote
    {
        [Required(AllowEmptyStrings =false)]
        public string Title { get; set; }

        public int Category { get; set; }
        public int SellerID { get; set; }
        public string Display_Picture { get; set; }
        public int NoteID { get; set; }
        public int Status { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public Nullable<int> NoteType { get; set; }
        public Nullable<int> Number_Of_Pages { get; set; }
        public string Description { get; set; }
        public string University_Name { get; set; }
        public Nullable<int> Country { get; set; }
        public string Course { get; set; }
        public string CourseCode { get; set; }
        public string Professor { get; set; }
        public bool IsPaid { get; set; }
        public Nullable<decimal> SellingPrice { get; set; }
        public string NotesPreview { get; set; }

        public IEnumerable<NoteType> types { get; set; }
        public IEnumerable<NoteCategory> categories { get; set; }
        public IEnumerable<Country> countries { get; set; }
    }
}