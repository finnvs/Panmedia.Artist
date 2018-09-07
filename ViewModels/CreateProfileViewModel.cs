using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panmedia.Artist.ViewModels
{
    public class CreateProfileViewModel
    {
        public int UserID { get; set; }
        public string Password { get; set; }
        public string Fornavn { get; set; }
        public string Efternavn { get; set; }
        public string Kunstnernavn { get; set; }
        public string Foedselsaarogdato { get; set; }
        public string Telefon { get; set; }
        public string EmailAdresse { get; set; }
        public string Hjemmeside { get; set; }
        public string Bynavn { get; set; }
        public string Gade { get; set; }
        public string Husnummer { get; set; }
        public string Postnummer { get; set; }
        public string Profiltekst { get; set; }
        public string ProfilePictureName { get; set; }
        public string ProfilePictureURL { get; set; }

        public enum EmailVisibility { Ingen, Medlemmer, Alle }
        public enum PhoneVisibility { Ingen, Medlemmer, Alle }
    }
}
