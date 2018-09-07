using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.UI.Notify;
using Orchard.Localization;
using Panmedia.Artist.Models;
using Panmedia.Artist.Services;
using Orchard.ContentManagement.Handlers;
using System;

namespace Panmedia.Artist.Drivers
{
    public class ArtistDriver : ContentPartDriver<ProfilePart>
    {

        private readonly INotifier _notifier;
        private readonly IArtistUserService _artistUserService;

        public Localizer T { get; set; }
        protected override string Prefix
        {
            // get { return "Artist"; }
            get { return "Profile"; }
        }

        public ArtistDriver(INotifier notifier, IArtistUserService artistUserService)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _artistUserService = artistUserService;
        }


        protected override DriverResult Display(ProfilePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Profile", () => shapeHelper.Parts_Profile(
                Part: part
            ));
        }

        // GET
        protected override DriverResult Editor(ProfilePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Profile_Edit", () => shapeHelper.EditorTemplate(
                TemplateName: "Parts/Profile",
                Model: part,
                Prefix: Prefix
            ));
        }

        // POST        
        protected override DriverResult Editor(ProfilePart part, IUpdateModel updater, dynamic shapeHelper)
        {            
            updater.TryUpdateModel(part, Prefix, null, null);
            updater.TryUpdateModel(part.User, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        // Importing content
        protected override void Importing(ProfilePart part, ImportContentContext context)
        {            
            context.ImportAttribute(part.PartDefinition.Name, "Fornavn", fornavn => part.Fornavn = fornavn);
            context.ImportAttribute(part.PartDefinition.Name, "Efternavn", efternavn => part.Efternavn = efternavn);
            context.ImportAttribute(part.PartDefinition.Name, "Kunstnernavn", kunstnernavn => part.Kunstnernavn = kunstnernavn);
            context.ImportAttribute(part.PartDefinition.Name, "Foedselsaarogdato", foedselsaarogdato => part.Foedselsaarogdato = foedselsaarogdato);
            context.ImportAttribute(part.PartDefinition.Name, "Telefon", telefon => part.Telefon = telefon);
            context.ImportAttribute(part.PartDefinition.Name, "EmailAdresse", emailAdresse => part.EmailAdresse = emailAdresse);
            context.ImportAttribute(part.PartDefinition.Name, "Hjemmeside", hjemmeside => part.Hjemmeside = hjemmeside);
            context.ImportAttribute(part.PartDefinition.Name, "Bynavn", bynavn => part.Bynavn = bynavn);
            context.ImportAttribute(part.PartDefinition.Name, "Gade", gade => part.Gade = gade);
            context.ImportAttribute(part.PartDefinition.Name, "Husnummer", husnummer => part.Husnummer = husnummer);
            context.ImportAttribute(part.PartDefinition.Name, "Postnummer", postnummer => part.Postnummer = postnummer);
            context.ImportAttribute(part.PartDefinition.Name, "Profiltekst", profiltekst => part.Profiltekst = profiltekst);
            context.ImportAttribute(part.PartDefinition.Name, "ProfilePictureName", profilePictureName => part.ProfilePictureName = profilePictureName);
            context.ImportAttribute(part.PartDefinition.Name, "ProfilePicItemId", profilePicItemId => part.ProfilePicItemId = Convert.ToInt32(profilePicItemId));
            context.ImportAttribute(part.PartDefinition.Name, "ProfilePictureURL", profilePictureURL => part.ProfilePictureURL = profilePictureURL);
            context.ImportAttribute(part.PartDefinition.Name, "CreatedUtc", CreatedUtc => part.CreatedUtc = DateTime.Parse(CreatedUtc));
            context.ImportAttribute(part.PartDefinition.Name, "LastEditedUtc", lastEditedUtc => part.LastEditedUtc = DateTime.Parse(lastEditedUtc));
            context.ImportAttribute(part.PartDefinition.Name, "EmailVisibility",
                emailVisibility => part.EmailVisibility = (EmailVisibility)Enum.Parse(typeof(EmailVisibility), emailVisibility));
            context.ImportAttribute(part.PartDefinition.Name, "PhoneVisibility",
                phoneVisibility => part.PhoneVisibility = (PhoneVisibility)Enum.Parse(typeof(PhoneVisibility), phoneVisibility));            
        }

        // Exporting content
        protected override void Exporting(ProfilePart part, ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Fornavn", part.Fornavn);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Efternavn", part.Efternavn);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Kunstnernavn", part.Kunstnernavn);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Foedselsaarogdato", part.Foedselsaarogdato);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Telefon", part.Telefon);
            context.Element(part.PartDefinition.Name).SetAttributeValue("EmailAdresse", part.EmailAdresse);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Hjemmeside", part.Hjemmeside);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Bynavn", part.Bynavn);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Gade", part.Gade);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Husnummer", part.Husnummer);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Postnummer", part.Postnummer);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Profiltekst", part.Profiltekst);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ProfilePictureName", part.ProfilePictureName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ProfilePicItemId", part.ProfilePicItemId);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ProfilePictureURL", part.ProfilePictureURL);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CreatedUtc", part.CreatedUtc);
            context.Element(part.PartDefinition.Name).SetAttributeValue("LastEditedUtc", part.LastEditedUtc);
            context.Element(part.PartDefinition.Name).SetAttributeValue("EmailVisibility", part.EmailVisibility);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PhoneVisibility", part.PhoneVisibility);
        }
    }
}