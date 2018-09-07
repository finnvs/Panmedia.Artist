using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Panmedia.Artist.Models;

namespace Panmedia.Artist.Bindings
{
    public class ProfilePartBindings : Component, IBindingProvider
    {
        public void Describe(BindingDescribeContext context)
        {         
            context.For<ProfilePart>()                
                .Binding("ProfilePicItemId", (contentItem, part, s) => part.ProfilePicItemId = XmlHelper.Parse<int>(s))                
                .Binding("Fornavn", (contentItem, part, s) => part.Fornavn = s)
                .Binding("Efternavn", (contentItem, part, s) => part.Efternavn = s)
                .Binding("Kunstnernavn", (contentItem, part, s) => part.Kunstnernavn = s)
                .Binding("Foedselsaarogdato", (contentItem, part, s) => part.Foedselsaarogdato = s)
                .Binding("Telefon", (contentItem, part, s) => part.Telefon = s)
                .Binding("EmailAdresse", (contentItem, part, s) => part.EmailAdresse = s)
                .Binding("Hjemmeside", (contentItem, part, s) => part.Hjemmeside = s)
                .Binding("Bynavn", (contentItem, part, s) => part.Bynavn = s)
                .Binding("Gade", (contentItem, part, s) => part.Gade = s)
                .Binding("Husnummer", (contentItem, part, s) => part.Husnummer = s)
                .Binding("Postnummer", (contentItem, part, s) => part.Postnummer = s)
                .Binding("Profiltekst", (contentItem, part, s) => part.Profiltekst = s);
        }
    }
}