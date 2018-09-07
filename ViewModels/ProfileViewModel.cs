using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panmedia.Artist.ViewModels
{
    public class ProfileViewModel
    {
        public string Fornavn { get; set; }
        public string Efternavn { get; set; }
        public string Kunstnernavn { get; set; }
        public string Telefon { get; set; }
        public string EmailAdresse { get; set; }
        public string Hjemmeside { get; set; }        
        public string Profiltekst { get; set; }
        public string ProfilePictureName { get; set; }
        public string ProfilePictureURL { get; set; }

        public string EmailVisibility { get; set; }
        public string PhoneVisibility { get; set; }
    }
}
