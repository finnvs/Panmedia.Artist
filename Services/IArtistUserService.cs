using Orchard;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Panmedia.Artist.Models;
using Panmedia.Artist.ViewModels;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Panmedia.Artist.Services
{
    public interface IArtistUserService : IDependency
    {
        ProfilePart GetProfile(UserPartRecord user);
        UserPart GetUserPart(UserPartRecord user);
        UserPart GetUserPart(string userName);
        UserPart CreateArtistUserPart(CreateProfileViewModel createModel);
        string GetFullName(string userName);
        string GetFirstName(string userName);
        int GetArtistProfileId(string userName);
        bool IsOrphan(int Id);
        void DeleteArtist(int Id);
        IContentQuery<ProfilePart, ProfilePartRecord> GetArtists();
        bool VerifyExistingUser(string userName);
    }
}