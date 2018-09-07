using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using System;
using Orchard.Security;
using Orchard.Users.Models;

namespace Panmedia.Artist.Models
{
    public class ProfilePart : ContentPart<ProfilePartRecord>
    {
        
        public int ProfilePicItemId
        {
            get { return Retrieve(x => x.ProfilePicItemId); }
            set { Store(x => x.ProfilePicItemId, value); }
        }
        
        //[Required]
        public string Fornavn
        {
            get { return Retrieve(x => x.Fornavn); }
            set { Store(x => x.Fornavn, value); }
        }

        //[Required]
        public string Efternavn
        {
            get { return Retrieve(x => x.Efternavn); }
            set { Store(x => x.Efternavn, value); }
        }

        public string Kunstnernavn
        {
            get { return Retrieve(x => x.Kunstnernavn); }
            set { Store(x => x.Kunstnernavn, value); }
        }

        //[Required]
        public string Foedselsaarogdato
        {
            get { return Retrieve(x => x.Foedselsaarogdato); }
            set { Store(x => x.Foedselsaarogdato, value); }
        }

        //[Required]
        public string Telefon
        {
            get { return Retrieve(x => x.Telefon); }
            set { Store(x => x.Telefon, value); }
        }

        //[Required]
        public string EmailAdresse
        {
            get { return Retrieve(x => x.EmailAdresse); }
            set { Store(x => x.EmailAdresse, value); }
        }

        public string Hjemmeside
        {
            get { return Retrieve(x => x.Hjemmeside); }
            set { Store(x => x.Hjemmeside, value); }
        }

        //[Required]
        public string Bynavn
        {
            get { return Retrieve(x => x.Bynavn); }
            set { Store(x => x.Bynavn, value); }
        }

        //[Required]
        public string Gade
        {
            get { return Retrieve(x => x.Gade); }
            set { Store(x => x.Gade, value); }
        }

        //[Required]
        public string Husnummer
        {
            get { return Retrieve(x => x.Husnummer); }
            set { Store(x => x.Husnummer, value); }
        }

        //[Required]
        public string Postnummer
        {
            get { return Retrieve(x => x.Postnummer); }
            set { Store(x => x.Postnummer, value); }
        }

        public string Profiltekst
        {
            get { return Retrieve(x => x.Profiltekst); }
            set { Store(x => x.Profiltekst, value); }
        }

        public string ProfilePictureName
        {
            get { return Retrieve(x => x.ProfilePictureName); }
            set { Store(x => x.ProfilePictureName, value); }
        }

        public string ProfilePictureURL
        {
            get { return Retrieve(x => x.ProfilePictureURL); }
            set { Store(x => x.ProfilePictureURL, value); }
        }

        public DateTime? CreatedUtc
        {
            get { return Retrieve(x => x.CreatedUtc); }
            set { Store(x => x.CreatedUtc, value); }
        }

        public DateTime? LastEditedUtc
        {
            get { return Retrieve(x => x.LastEditedUtc); }
            set { Store(x => x.LastEditedUtc, value); }
        }


        public EmailVisibility EmailVisibility
        {
            get { return Retrieve(x => x.EmailVisibility); }
            set { Store(x => x.EmailVisibility, value); }
        }

        public PhoneVisibility PhoneVisibility
        {
            get { return Retrieve(x => x.PhoneVisibility); }
            set { Store(x => x.PhoneVisibility, value); }
        }

        public IUser User
        {
            get { return this.As<UserPart>(); }
        }

    }

    // Int reference is necessary for storing in db as string
    public enum EmailVisibility : int
    {
        Ingen = 1,
        Medlemmer = 2,
        Alle = 3
    }

    public enum PhoneVisibility : int
    {
        Ingen = 1,
        Medlemmer = 2,
        Alle = 3
    }
}
