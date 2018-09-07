using System.ComponentModel.DataAnnotations;

namespace Panmedia.Artist.ViewModels
{
    public class EditProfileViewModel
    {        
        
        public int ProfilePictureMediaId { get; set; }
        public string Efternavn { get; set; }
        public string Fornavn { get; set; }
        public string Kunstnernavn { get; set; }        
        public string Telefon { get; set; }        
        public string Hjemmeside { get; set; }
        public string Bynavn { get; set; }
        public string Gade { get; set; }
        public string Husnummer { get; set; }
        public string Postnummer { get; set; }
        public string Profiltekst { get; set; }

        public string ProfilePictureName { get; set; }
        public string ProfilePictureURL { get; set; }

        public string EmailVisibility { get; set; }
        public string PhoneVisibility { get; set; }        
    }
}
