using Panmedia.Artist.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Core.Contents.Extensions;
using System;
using Orchard.Indexing;
using System.Data;

namespace Panmedia.Artist
{
    /// <summary>
    /// Note: Whenever you create a new content type that has a UserPart, don't attach the CommonPart as well. 
    /// Doing so will cause a StackOverflowException when you sign in with that new user type. 
    /// This happens because whenever Orchard news up a content item, it invokes all content handlers, 
    /// including the CommonPartHandler. The CommonPartHandler will try to assign the currently loggedin user, 
    /// but in doing so it will have to load that user. Loading that user will again invoke the CommonPartHandler, 
    /// which in turn will invoke the AuthenticationService to get the current user, and so on.
    /// </summary>
    public class Migrations : DataMigrationImpl
    {

        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(ProfilePartRecord).Name, command => command
                .ContentPartRecord()                
                .Column<int>("ProfilePicItemId")
                .Column<string>("Fornavn")
                .Column<string>("Efternavn")
                .Column<string>("Kunstnernavn")
                .Column<string>("Foedselsaarogdato")
                .Column<string>("Telefon")
                .Column<string>("EmailAdresse")
                .Column<string>("Hjemmeside")
                .Column<string>("Bynavn")
                .Column<string>("Gade")
                .Column<string>("Husnummer")
                .Column<string>("Postnummer")
                .Column<string>("Profiltekst")
                .Column<string>("ProfilePictureName")
                .Column<string>("ProfilePictureURL")
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("LastEditedUtc")
                .Column<string>("EmailVisibility")
                .Column<string>("PhoneVisibility")
            );

            // Enum field for email visibility           
            string emailVisibilityOptions = string.Join(System.Environment.NewLine,
                new[] { "Ingen", "Medlemmer", "Alle" });

            ContentDefinitionManager.AlterPartDefinition(typeof(ProfilePart).Name, builder => builder
                .WithField("EmailVisibility", cfg => cfg
                .OfType("EnumerationField")
                .WithDisplayName("EmailVisibility")
                .WithSetting("EnumerationFieldSettings.Required", "True")
                .WithSetting("EnumerationFieldSettings.ListMode", "Radiobutton")
                .WithSetting("EnumerationFieldSettings.Options", emailVisibilityOptions))
            );


            // Enum field for phone visibility           
            string phoneVisibilityOptions = string.Join(System.Environment.NewLine,
                new[] { "Ingen", "Medlemmer", "Alle" });

            ContentDefinitionManager.AlterPartDefinition(typeof(ProfilePart).Name, builder => builder
                .WithField("PhoneVisibility", cfg => cfg
                .OfType("EnumerationField")
                .WithDisplayName("PhoneVisibility")
                .WithSetting("EnumerationFieldSettings.Required", "True")
                .WithSetting("EnumerationFieldSettings.ListMode", "Radiobutton")
                .WithSetting("EnumerationFieldSettings.Options", phoneVisibilityOptions))
            );


            ContentDefinitionManager.AlterPartDefinition(typeof(ProfilePart).Name, builder => builder
                .WithDescription("Custom Artist Profile Part for AASV Website")
                .Attachable(false)
            );            

            ContentDefinitionManager.AlterTypeDefinition("Artist", cfg => cfg
                 .WithPart("ProfilePart")
                 .WithPart("UserPart")
                 .WithPart("TitlePart")                 
                 .WithPart("TagsPart")                                          
                 .WithPart("ContainablePart")                  
                 .Indexed()
                 );

            return 1;
        }

        // set Profiltekst column to long string - default SQL CE behavior is to truncate strings,
        // because column string type is set to NVARCHAR(255) if no max string length is given
        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable("ProfilePartRecord",
                table =>
                {
                    table.AlterColumn("Profiltekst", x => x.WithType(DbType.String).Unlimited());
                }
            );
            return 2;
        }

        public int UpdateFrom2()
        {
            ContentDefinitionManager.AlterTypeDefinition("Artist", cfg => cfg
                 .WithPart("IdentityPart")
                 );
            return 3;
        }
    }
        
}