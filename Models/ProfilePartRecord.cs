using Orchard.ContentManagement.Records;
using Orchard.Users.Models;
using System;

namespace Panmedia.Artist.Models {
    public class ProfilePartRecord : ContentPartRecord {      
        
        public virtual string Fornavn { get; set; }
        public virtual string Efternavn { get; set; }
        public virtual string Kunstnernavn { get; set; }
        public virtual string Foedselsaarogdato { get; set; }
        public virtual string Telefon { get; set; }
        public virtual string EmailAdresse { get; set; }
        public virtual string Hjemmeside { get; set; }
        public virtual string Bynavn { get; set; }
        public virtual string Gade { get; set; }
        public virtual string Husnummer { get; set; }
        public virtual string Postnummer { get; set; }
        public virtual string Profiltekst { get; set; }
        public virtual string ProfilePictureName { get; set; }
        public virtual int ProfilePicItemId { get; set; }
        public virtual string ProfilePictureURL { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
        public virtual DateTime? LastEditedUtc { get; set; }
        public virtual EmailVisibility EmailVisibility { get; set; }
        public virtual PhoneVisibility PhoneVisibility { get; set; }

        // User part ref - for welding Orchard User and the profile part
        // public virtual UserPartRecord UserPartRecord { get; set; }
    }

    
}