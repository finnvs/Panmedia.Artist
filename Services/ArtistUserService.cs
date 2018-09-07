using System.Linq;
using Orchard;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Panmedia.Artist.Models;
using Panmedia.Artist.ViewModels;
using Orchard.Services;
using Orchard.Security;
using Orchard.Core.Title.Models;
using Orchard.Roles.Models;
using Orchard.Data;
using Orchard.Roles.Services;
using Orchard.Roles.Events;
using System.Xml.Linq;
using System.Web;

namespace Panmedia.Artist.Services
{

    public class ArtistUserService : IArtistUserService
    {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly IRepository<ProfilePartRecord> _artistRepository;
        private readonly IRepository<UserPartRecord> _userRepository;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IRoleService _roleService;
        private readonly IRoleEventHandler _roleEventHandlers;
        private IOrchardServices Services { get; set; }
        private readonly IClock _clock;
        public ILogger Logger { get; set; }

        public ArtistUserService(
            IOrchardServices services,
            IContentManager contentManager,
            IMembershipService membershipService,
            IClock clock,
            IRepository<ProfilePartRecord> artistRepository,
            IRepository<UserPartRecord> userRepository,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IRoleService roleService,
            IRoleEventHandler roleEventHandlers
            )
        {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _artistRepository = artistRepository;
            _userRepository = userRepository;
            _clock = clock;
            _userRolesRepository = userRolesRepository;
            _roleService = roleService;
            _roleEventHandlers = roleEventHandlers;
            Services = services;
            Logger = NullLogger.Instance;
        }

        public UserPart GetUserPart(string userName)
        {
            var userPart = _contentManager.Query<UserPart>()
               .Where<UserPartRecord>(x => x.UserName == userName)
               .List().First();
            return userPart;
        }

        public ProfilePart GetProfile(UserPartRecord user)
        {
            return user == null ? null : GetUserPart(user).As<ProfilePart>();
        }

        public UserPart GetUserPart(UserPartRecord user)
        {
            if (user == null)
                return null;
            return _contentManager.Query<UserPart>()
               .Where<UserPartRecord>(x => x.Id == user.Id)
               .List().First();
        }

        public string GetFullName(string userName)
        {
            if (VerifyExistingUser(userName))
            {
                var userPart = _contentManager.Query<UserPart>()
               .Where<UserPartRecord>(x => x.UserName == userName)
               .List().First();

                var profile = GetProfile(userPart.Record);
                return profile.Fornavn + " " + profile.Efternavn;
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetFirstName(string userName)
        {
            if (VerifyExistingUser(userName))
            {
                var userPart = _contentManager.Query<UserPart>()
                   .Where<UserPartRecord>(x => x.UserName == userName)
                   .List().First();

                var profile = GetProfile(userPart.Record);
                return profile.Fornavn;
            }
            else
            {
                return string.Empty;
            }
        }

        public int GetArtistProfileId(string userName)
        {
            if (VerifyExistingUser(userName))
            {
                var userPart = _contentManager.Query<UserPart>()
               .Where<UserPartRecord>(x => x.UserName == userName)
               .List().First();

                var profile = GetProfile(userPart.Record);
                return profile.Id;
            }
            else
            {
                return 0;
            }
        }

        public bool IsOrphan(int Id)
        {
            var userPart = _contentManager.Query<UserPart>()
               .Where<UserPartRecord>(x => x.Id == Id)
               .List().First();

            var profile = GetProfile(userPart.Record);

            if (userPart != null && profile != null)
            {
                if (userPart.Id == profile.Id)
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        public void DeleteArtist(int Id)
        {
            Services.ContentManager.Remove(Services.ContentManager.Get(Id));
            _artistRepository.Delete(_artistRepository.Get(Id));
            _userRepository.Delete(_userRepository.Get(Id));
        }

        public IContentQuery<ProfilePart, ProfilePartRecord> GetArtists()
        {
            return Services.ContentManager
                .Query<ProfilePart, ProfilePartRecord>();
        }


        public bool VerifyExistingUser(string userName)
        {
            if (_contentManager.Query<UserPart>()
               .Where<UserPartRecord>(x => x.UserName == userName)
               .List().Any())
                return true;
            return false;
        }


        /// <summary>
        /// Service class to create a new Orchard User
        /// </summary>
        /// <param name="createModel"></param>
        /// <returns></returns>
        public UserPart CreateArtistUserPart(CreateProfileViewModel createModel)
        {
            Logger.Information("CreateArtist {0}", createModel.Fornavn + " " + createModel.Efternavn); // + " med User ID: " + createModel.UserID.ToString());

            // New up a content item of type "Artist"
            var artist = Services.ContentManager.New("Artist");
            // Cast the artist to a UserPart
            var artistUserPart = artist.As<UserPart>();
            // Set properties of user part in new Artist content item
            artistUserPart.UserName = createModel.EmailAdresse;
            artistUserPart.Email = createModel.EmailAdresse;
            artistUserPart.CreatedUtc = _clock.UtcNow;
            artistUserPart.NormalizedUserName = createModel.EmailAdresse.ToLowerInvariant();
            artistUserPart.Record.HashAlgorithm = "SHA1";
            artistUserPart.Record.RegistrationStatus = UserStatus.Approved;
            artistUserPart.Record.EmailStatus = UserStatus.Approved;
            // _membershipService.SetPassword(artistUserPart, createModel.Password); // mangler et pwd i modellen

            artist.As<TitlePart>().Title = createModel.Fornavn + " " + createModel.Efternavn;
            Services.ContentManager.Create(artistUserPart);

            // Set up roles for new user
            //if (createModel.OrchardRoles.Count() > 0)
            //{
            //    foreach (string s in createModel.OrchardRoles)
            //    {
            //        _userRolesRepository.Create(
            //            new UserRolesPartRecord
            //            {
            //                Role = _roleService.GetRoleByName(s),
            //                UserId = createModel.Id
            //            });
            //    }
            //}

            return artistUserPart;
        }
    }
}
