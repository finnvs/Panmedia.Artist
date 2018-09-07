using System;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Panmedia.Artist.ViewModels;
using Panmedia.Artist.Models;
using Panmedia.Artist.Services;
using Orchard.DisplayManagement;
using System.IO;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Roles.Events;
using Orchard.Services;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Core.Title.Models;
using Panmedia.Artist.Handlers;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Orchard.UI.Navigation;
using Orchard.Settings;

namespace Panmedia.Artist.Controllers
{
    [Themed]
    public class ArtistController : Controller, IUpdateModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IRoleService _roleService;
        private readonly IRoleEventHandler _roleEventHandlers;
        private readonly IClock _clock;
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IArtistUserService _artistUserService;
        private readonly IPasswordService _passwordService;
        private readonly ISiteService _siteService;

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        dynamic Shape { get; set; }


        public ArtistController(IOrchardServices services,
            IAuthenticationService authenticationService,
            IMembershipService membershipService,
            IUserService userService,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IRoleService roleService,
            IRoleEventHandler roleEventHandlers,
            IClock clock,
            IShapeFactory shapeFactory,
            IMediaLibraryService mediaLibraryService,
            IArtistUserService artistUserService,
            IPasswordService passwordService,
            ISiteService siteService)
        {
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _userService = userService;
            _userRolesRepository = userRolesRepository;
            _roleService = roleService;
            _roleEventHandlers = roleEventHandlers;
            _clock = clock;
            Services = services;
            Shape = shapeFactory;
            _mediaLibraryService = mediaLibraryService;
            _artistUserService = artistUserService;
            _passwordService = passwordService;
            _siteService = siteService;
        }



        /// <summary>
        /// HTTP GET for signing up a new user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public ActionResult SignUp(string username)
        {
            IUser user = _membershipService.GetUser(username);

            if (user == null || !Services.Authorizer.Authorize(Permissions.ViewProfiles, user, null))
            {
                return HttpNotFound();
            }

            // If an Artist content item matches the user name, get data from that to populate the view.
            var artist = GetProfileForCurrentUser();

            if (artist == null)
            {
                return HttpNotFound();
            }
            else
            {
                var model = new ProfileViewModel();

                model.Fornavn = artist.Fornavn;
                model.Efternavn = artist.Efternavn;
                model.Telefon = artist.Telefon;

                model.EmailAdresse = artist.EmailAdresse;
                model.Hjemmeside = artist.Hjemmeside;
                model.Kunstnernavn = artist.Kunstnernavn;

                model.ProfilePictureName = artist.ProfilePictureName;
                model.ProfilePictureURL = artist.ProfilePictureURL;
                model.Profiltekst = artist.Profiltekst;

                return View(model);
            }

        }


        /// <summary>
        /// POST Sign Up / Edit user profile scenarie: profileData from dynamic form submit       
        /// Sign Up POST outcome: 'text only' user created and email welcome msg sent, w redirect to EditImage
        /// </summary>
        /// <param name="profileData"></param>
        /// <returns></returns>
        [HttpPost, ActionName("SignUp")]
        public ActionResult SignUpPost(CreateProfileViewModel profileData)
        {
            var artist = Services.ContentManager.New("Artist");
            var artistProfilePart = artist.As<ProfilePart>();
            var artistUserPart = artist.As<UserPart>();


            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Information(T("Der opstod en fejl ved validering af profil data - kontakt evt. webmaster"));
                return View("CreateProfileFailure");
            }

            // validate user unicity on email supplied
            if (!_userService.VerifyUserUnicity(profileData.EmailAdresse, profileData.EmailAdresse))
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Information(T("Der er allerede en bruger oprettet med denne email - kontakt evt. webmaster"));
                return View("CreateProfileFailure");
            }

            if (ModelState.IsValid)
            {
                // Set up new user
                artistUserPart.UserName = profileData.EmailAdresse;
                artistUserPart.Email = profileData.EmailAdresse;
                artistUserPart.NormalizedUserName = profileData.EmailAdresse.ToLowerInvariant();
                artistUserPart.Record.HashAlgorithm = "SHA1";
                artistUserPart.Record.RegistrationStatus = UserStatus.Approved;
                artistUserPart.Record.EmailStatus = UserStatus.Approved;
                _membershipService.SetPassword(artistUserPart, profileData.Password);


                // Populate Profile Part with data from form post
                artistProfilePart.Bynavn = profileData.Bynavn;
                artistProfilePart.Gade = profileData.Gade;
                artistProfilePart.Hjemmeside = profileData.Hjemmeside;
                artistProfilePart.Husnummer = profileData.Husnummer;
                artistProfilePart.Kunstnernavn = profileData.Kunstnernavn;
                artistProfilePart.Postnummer = profileData.Postnummer;
                artistProfilePart.Profiltekst = profileData.Profiltekst;
                artistProfilePart.Telefon = profileData.Telefon;
                artistProfilePart.EmailAdresse = profileData.EmailAdresse;
                artistProfilePart.Fornavn = profileData.Fornavn;
                artistProfilePart.Efternavn = profileData.Efternavn;
                artistProfilePart.Foedselsaarogdato = profileData.Foedselsaarogdato;
                artistProfilePart.CreatedUtc = _clock.UtcNow;
                artistProfilePart.LastEditedUtc = _clock.UtcNow;
                // Set title for item
                artist.As<TitlePart>().Title = profileData.Fornavn + " " + profileData.Efternavn;

                switch (HttpContext.Request.Params.Get("EmailVisibility"))
                {
                    case ("Ingen"): 
                        { artistProfilePart.EmailVisibility = EmailVisibility.Ingen; }
                        break;

                    case ("Medlemmer"):
                        { artistProfilePart.EmailVisibility = EmailVisibility.Medlemmer; }
                        break;

                    case ("Alle"):
                        { artistProfilePart.EmailVisibility = EmailVisibility.Alle; }
                        break;

                    default:
                        artistProfilePart.EmailVisibility = EmailVisibility.Ingen;
                        break;
                }

                switch (HttpContext.Request.Params.Get("PhoneVisibility"))
                {
                    case ("Ingen"):
                        { artistProfilePart.PhoneVisibility = PhoneVisibility.Ingen; }
                        break;

                    case ("Medlemmer"):
                        { artistProfilePart.PhoneVisibility = PhoneVisibility.Medlemmer; }
                        break;

                    case ("Alle"):
                        { artistProfilePart.PhoneVisibility = PhoneVisibility.Alle; }
                        break;

                    default:
                        artistProfilePart.PhoneVisibility = PhoneVisibility.Ingen;
                        break;
                }
            }

            // TODO: set a default profile picture - use you own azure blob storage url here in place of mine.
            // artistProfilePart.ProfilePictureURL = "http://panmediablob.blob.core.windows.net/aasv-container/silhouette.jpg";
            artistProfilePart.ProfilePictureURL = "YOUR_AZURE_BLOB_URL";
            artistProfilePart.ProfilePictureName = "silhouette.jpg";

            // Persist new artist content item (publish after image upload)
            Services.ContentManager.Create(artist);

            Services.Notifier.Information(T("Din profil er nu oprettet - hvis du ikke ønsker at uploade et billede, kan du trykke 'Luk vindue'."));

            /// <summary>
            /// Note, Sipke S part 8: Whenever you create a new content type that has a UserPart, don't attach the CommonPart as well. 
            /// Doing so will cause a StackOverflowException when you sign in with that new user type. 
            /// This happens because whenever Orchard news up a content item, it invokes all content handlers, 
            /// including the CommonPartHandler. The CommonPartHandler will try to assign the currently loggedin user, 
            /// but in doing so it will have to load that user. Loading that user will again invoke the CommonPartHandler, 
            /// which in turn will invoke the AuthenticationService to get the current user, and so on.
            /// </summary>

            // login new user. As long as no CommonPart is included in Migrations.cs, this should work as expected.
            IUser user = _membershipService.ValidateUser(profileData.EmailAdresse, profileData.Password);
            _authenticationService.SignIn(user, true);
            
            TempData["ArtistItemId"] = artist.Id;
            return RedirectToAction("ImageUpload");
        }

        /// <summary>
        /// HTTP GET for Upload first ProfilePicture
        /// </summary>
        /// <returns></returns>
        public ActionResult ImageUpload()
        {
            return View("ImageUploadAvatar");
        }


        /// <summary>
        /// HTTP GET for Edit ProfilePicture - renders out only the image related items we want the user to edit
        /// </summary>
        /// <returns></returns>
        public ActionResult ImageEdit()
        {
            if (Services.WorkContext.CurrentUser == null)
            {
                return HttpNotFound();
            }

            IUser user = Services.WorkContext.CurrentUser;

            var artist = GetProfileForCurrentUser();

            if (artist == null)
            {
                return HttpNotFound();
            }
            else
            {
                var model = new EditProfileImageViewModel();

                model.Fornavn = artist.Fornavn;
                model.ProfilePictureName = artist.ProfilePictureName;
                model.ProfilePictureURL = artist.ProfilePictureURL;

                return View(model);
            }
        }


        /// <summary>
        /// POST Edit user profile image scenarie: profileImageData 
        /// </summary>
        /// <param name="ArtistItemId"></param>
        /// <returns></returns>
        [HttpPost, ActionName("ImageEdit")]
        public ActionResult ImageEditPost()
        {
            int id = Convert.ToInt32(TempData["ArtistItemId"]);
            if (id == 0)
            {
                id = Services.WorkContext.CurrentUser.ContentItem.Id;
            }

            var artist = Services.ContentManager.Get(id);

            if (artist == null)
            {
                Services.Notifier.Information(T("Profilen kunne ikke hentes - kontakt venligst webmaster@sang-skriver.dk."));
                return HttpNotFound();
            }

            // Cast the artist to a ProfilePart / UserPart
            var artistProfilePart = artist.As<ProfilePart>();
            var artistUserPart = artist.As<UserPart>();
            var profileImageData = new EditProfileImageViewModel();

            // populate the profilePart fields for profile image in Blob storage, if one has been uploaded
            if (HttpContext.Request.Files.Count > 0)
            {
                string imageName = HttpContext.Request.Files.Get(0).FileName;
                profileImageData.ProfilePictureName = imageName;

                // TODO: Set up your own Azure Blob container and set pwd details in web.config of Orchard.Web project
                // profileImageData.ProfilePictureURL = "http://panmediablob.blob.core.windows.net/aasv-container/" + imageName;
                profileImageData.ProfilePictureURL = "YOUR_AZURE_BLOB_URL" + imageName; 

                // TEST: file system upload Orchard
                // profileImageData.ProfilePictureURL = Url.Content("~/Media/Default/ProfilePics/" + imageName);

                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                   
                    // check if uploaded file is legit (file type extension checks)
                    var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();
                    var allowedExtensions = (settings.UploadAllowedFileTypeWhitelist ?? "")
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(x => x.StartsWith("."));

                    // skip file if the allowed extensions is defined and doesn't match
                    if (allowedExtensions.Any())
                    {
                        if (!allowedExtensions.Any(e => fileName.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                        {
                            Services.Notifier.Information(T("This file type is not allowed: {0}", Path.GetExtension(fileName)));
                            return View("ImageEdit", profileImageData);
                        }
                    }

                    // Store image in blob container
                    // BlobHandler bh = new BlobHandler("aasv-container");
                    // TODO: Pass the name of your own Azure Blob Container to the BlobHandler method
                    BlobHandler bh = new BlobHandler("YOUR_BLOB_CONTAINER_NAME_HERE");
                    bh.Upload(file);             
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Information(T("Der opstod en fejl ved validering af profilbillede data - kontakt evt. webmaster@sang-skriver.dk"));
                return View("ImageEdit", profileImageData);
            }

            if (ModelState.IsValid)
            {               
                if (profileImageData.ProfilePictureName != string.Empty &&
                    profileImageData.ProfilePictureName != null &&
                    profileImageData.ProfilePictureName != "")
                {
                    artistProfilePart.ProfilePictureURL = profileImageData.ProfilePictureURL;
                    artistProfilePart.ProfilePictureName = profileImageData.ProfilePictureName;
                }

                // TODO: set a fallback default 'silhouette' profile picture - 
                // use your own azure blob storage url here in place of mine.
                else if (
                    profileImageData.ProfilePictureName == null ||
                    profileImageData.ProfilePictureName == "" ||                    
                    profileImageData.ProfilePictureURL == "http://panmediablob.blob.core.windows.net/aasv-container" ||
                    artistProfilePart.ProfilePictureURL == null ||
                    artistProfilePart.ProfilePictureURL == "" ||                    
                    artistProfilePart.ProfilePictureURL == "http://panmediablob.blob.core.windows.net/aasv-container")
                {
                    // link to silhouette image; last resort                   
                    artistProfilePart.ProfilePictureURL = "http://panmediablob.blob.core.windows.net/aasv-container/silhouette.jpg";
                    artistProfilePart.ProfilePictureName = "silhouette.jpg";
                }

                Services.ContentManager.UpdateEditor(artist, this);
                Services.Notifier.Information(T("Dit profilbillede er redigeret"));

                // Publish full content item, with all props set correctly
                Services.ContentManager.Publish(artist);

                // send user to own profile view                
                return RedirectToAction("Index", new { username = artistUserPart.UserName });  
            }
            return Redirect("~/Profile/" + Services.WorkContext.CurrentUser.UserName);
        }


        /// <summary>
        /// Index method is called for display of user profile
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>

        public ActionResult ViewProfile(int userId)
        {
            // If an Artist content item matches the user name, get data from that to populate the view.           
            var artist = Services.ContentManager.Get(userId);
            var artistProfilePart = artist.As<ProfilePart>();

            if (artist == null)
            {
                var model = new ProfileViewModel();
                return new HttpNotFoundResult("No artist content item was found for user with id: " + userId);
            }
            else
            {
                var model = new ProfileViewModel();

                model.Fornavn = artistProfilePart.Fornavn;
                model.Efternavn = artistProfilePart.Efternavn;
                model.Telefon = artistProfilePart.Telefon;

                model.EmailAdresse = artistProfilePart.EmailAdresse;
                model.Hjemmeside = artistProfilePart.Hjemmeside;
                model.Kunstnernavn = artistProfilePart.Kunstnernavn;

                model.ProfilePictureName = artistProfilePart.ProfilePictureName;
                model.ProfilePictureURL = artistProfilePart.ProfilePictureURL;
                model.Profiltekst = artistProfilePart.Profiltekst;

                model.EmailVisibility = artistProfilePart.EmailVisibility.ToString();
                model.PhoneVisibility = artistProfilePart.PhoneVisibility.ToString();

                return View("Index", model);
            }

        }

        public ActionResult Index(string username)
        {
            IUser user = _membershipService.GetUser(username);

            if (user == null) // || !Services.Authorizer.Authorize(Permissions.ViewProfiles, user, null)) // currently, we are not logged in
            {
                return HttpNotFound();
            }

            // If an Artist content item matches the user name, get data from that to populate the view.           
            var artist = Services.ContentManager.Get(user.ContentItem.Id);
            var artistProfilePart = artist.As<ProfilePart>();

            if (artist == null)
            {
                var model = new ProfileViewModel();
                return new HttpNotFoundResult("No artist content item was found for user name: " + username);
            }
            else
            {
                var model = new ProfileViewModel();

                model.Fornavn = artistProfilePart.Fornavn;
                model.Efternavn = artistProfilePart.Efternavn;
                model.Telefon = artistProfilePart.Telefon;

                model.EmailAdresse = artistProfilePart.EmailAdresse;
                model.Hjemmeside = artistProfilePart.Hjemmeside;
                model.Kunstnernavn = artistProfilePart.Kunstnernavn;

                model.ProfilePictureName = artistProfilePart.ProfilePictureName;
                model.ProfilePictureURL = artistProfilePart.ProfilePictureURL;
                model.Profiltekst = artistProfilePart.Profiltekst;

                model.EmailVisibility = artistProfilePart.EmailVisibility.ToString();
                model.PhoneVisibility = artistProfilePart.PhoneVisibility.ToString();

                return View("Index", model);
            }            
        }
        /// <summary>
        /// GET Edit Profile - renders out only the items we want the user to edit
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit()
        {
            if (Services.WorkContext.CurrentUser == null)
            {
                return HttpNotFound();
            }

            IUser user = Services.WorkContext.CurrentUser;
            var artist = GetProfileForCurrentUser();

            if (artist == null)
            {        
                return HttpNotFound();
            }
            else
            {
                var model = new EditProfileViewModel();

                model.Fornavn = artist.Fornavn;
                model.Efternavn = artist.Efternavn;
                model.Postnummer = artist.Postnummer;
                model.Bynavn = artist.Bynavn;
                model.Gade = artist.Gade;
                model.Husnummer = artist.Husnummer;
                model.Telefon = artist.Telefon;
                model.EmailVisibility = artist.EmailVisibility.ToString();
                model.PhoneVisibility = artist.PhoneVisibility.ToString();

                model.Hjemmeside = artist.Hjemmeside;
                model.Kunstnernavn = artist.Kunstnernavn;

                // if sign-up or edit form was not posted with an image
                if (artist.ProfilePictureURL == "" || artist.ProfilePictureURL == null)
                {
                    // TODO: set a default profile picture - use your own azure blob storage url here in place of mine
                    model.ProfilePictureURL = "http://panmediablob.blob.core.windows.net/aasv-container/silhouette.jpg";
                    model.ProfilePictureName = "silhouette.jpg";
                }
                else
                {
                    model.ProfilePictureName = artist.ProfilePictureName;
                    model.ProfilePictureURL = artist.ProfilePictureURL;
                }
                                
                var MediaUrl = model.ProfilePictureURL;
                model.ProfilePictureMediaId = 2;
                model.Profiltekst = artist.Profiltekst;

                // check if user has clicked on flag, setting a cookie culture
                if (Services.WorkContext.HttpContext.Request.Cookies.AllKeys.Contains("AASV_CurrentCulture"))
                {
                    var cookieCulture = Services.WorkContext.HttpContext.Request.Cookies.Get("AASV_CurrentCulture").Value;
                    // set culture in WorkContext
                    Services.WorkContext.CurrentCulture = cookieCulture;                   
                    if (cookieCulture == "en-US")
                    {
                        return View("Edit_En", model);
                    }
                }
                return View(model);
            }            
        }


        /// <summary>
        /// POST Edit user profile scenarie: profileData fra dynamic form submit 
        /// sættes ind i en ny content type, 'Artist'. 
        /// TODO: Ændres der i brugerens password ved redigering, skal det sættes igen.
        /// TODO: Check at der kan gemmes ændringer af Email- og PhoneVisibility params via radio button list.
        /// </summary>
        /// <param name="profileData"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(EditProfileViewModel profileData)
        {
            if (Services.WorkContext.CurrentUser == null)
            {
                return HttpNotFound();
            }

            IUser user = Services.WorkContext.CurrentUser;
            bool newUser = false;

            // Find existing Content Item, or New up a new content item of type "Artist"
            var artist = GetProfileForCurrentUser().As<ContentItem>();
            if (artist == null)
            {
                artist = Services.ContentManager.New("Artist");
                newUser = true;
            }

            // Cast the artist to a ProfilePart
            var artistProfilePart = artist.As<ProfilePart>();
            // Cast the artist to a UserPart, adding the user bits in later
            var artistUserPart = artist.As<UserPart>();

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Information(T("Der opstod en fejl ved validering af profil data - kontakt evt. webmaster@sang-skriver.dk"));
                return View("Edit", profileData);
            }

            if (ModelState.IsValid)
            {
                // Populate profile part with data from form post
                artistProfilePart.Fornavn = profileData.Fornavn;
                artistProfilePart.Efternavn = profileData.Efternavn;
                artistProfilePart.Kunstnernavn = profileData.Kunstnernavn;
                artistProfilePart.Bynavn = profileData.Bynavn;
                artistProfilePart.Gade = profileData.Gade;
                artistProfilePart.Hjemmeside = profileData.Hjemmeside;
                artistProfilePart.Husnummer = profileData.Husnummer;
                artistProfilePart.Postnummer = profileData.Postnummer;
                artistProfilePart.Profiltekst = profileData.Profiltekst;
                artistProfilePart.Telefon = profileData.Telefon;
                artistProfilePart.LastEditedUtc = _clock.UtcNow;                

                // Set title for item
                artist.As<TitlePart>().Title = profileData.Fornavn + " " + profileData.Efternavn;         

                switch (HttpContext.Request.Params.Get("EmailVisibility"))
                {
                    case ("Ingen"):
                        { artistProfilePart.EmailVisibility = EmailVisibility.Ingen; }
                        break;

                    case ("Medlemmer"):
                        { artistProfilePart.EmailVisibility = EmailVisibility.Medlemmer; }
                        break;

                    case ("Alle"):
                        { artistProfilePart.EmailVisibility = EmailVisibility.Alle; }
                        break;

                    default:
                        artistProfilePart.EmailVisibility = EmailVisibility.Ingen;
                        break;
                }

                switch (HttpContext.Request.Params.Get("PhoneVisibility"))
                {
                    case ("Ingen"):
                        { artistProfilePart.PhoneVisibility = PhoneVisibility.Ingen; }
                        break;

                    case ("Medlemmer"):
                        { artistProfilePart.PhoneVisibility = PhoneVisibility.Medlemmer; }
                        break;

                    case ("Alle"):
                        { artistProfilePart.PhoneVisibility = PhoneVisibility.Alle; }
                        break;

                    default:
                        artistProfilePart.PhoneVisibility = PhoneVisibility.Ingen;
                        break;
                }
            }

           
            if (newUser == true)
            {
                // Persist and store new artist content item
                Services.ContentManager.Create(artist);
            }
            else
            {
                Services.ContentManager.UpdateEditor(artist, this);
                // Services.ContentManager.UpdateEditor(artistProfilePart, this);
            }

            Services.Notifier.Information(T("Din profil er redigeret"));
            // send user on to profile image edit screen
            return RedirectToAction("ImageUpload");
        }

        public ActionResult ProfileListing(PagerParameters pagerParameters)
        {
            var artists = _artistUserService.GetArtists().List();
            // Set up pager w page size 15 user profile links
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters.Page, pagerParameters.PageSize = 15);
            // Apply paging
            var artistPagedList = artists.Skip(pager.GetStartIndex()).Take(pager.PageSize);

            var pagerShape = Shape.Pager(pager).TotalItemCount(artists.Count());

            var model = new ProfileListVM
            {
                Artists = new List<ArtistInfo>(),
                Pager = pagerShape
            };

            foreach (var artist in artistPagedList)
            {
                model.Artists.Add(new ArtistInfo
                {
                    ArtistTitle = artist.Fornavn + " " + artist.Efternavn,
                    ArtistUserName = artist.EmailAdresse,
                    ArtistId = artist.Id
                });
            }

            return View("ProfileListing", model);
        }

        // Query content items of type Artist for a specific UserID based on email value
        // TODO: another fallback method if curent user is not an Artist? This won't work in that scenario.
        public ProfilePart GetProfileForCurrentUser()
        {
            var userEmail = Services.WorkContext.CurrentUser.UserName;
            var allArtists = Services.ContentManager.Query("Artist").List();

            foreach (ContentItem c in allArtists)
            {
                if (c.As<ProfilePart>().EmailAdresse == userEmail)
                    return c.As<ProfilePart>();
            }
            return null;
        }

        /// <summary>
        /// GET for ForgotPwd form 
        /// </summary>           
        public ActionResult ForgotPwd()
        {
            return View();
        }

        /// <summary>
        /// POST for ForgotPwd form 
        /// </summary>
        /// <param name="newPwdData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ForgotPwd(ForgotPwdVM newPwdData)
        {
            if (_artistUserService.VerifyExistingUser(newPwdData.EmailAdresse))
            {
                if (_passwordService.ForgottenPassword(newPwdData.EmailAdresse))
                {
                    Services.Notifier.Information(T("Der er nu afsendt et nyt password til din emailadresse."));
                    return View("ForgotPwdSuccess");
                }
                else
                {
                    Services.Notifier.Information(T("Der opstod en fejl ved afsendelsen af email. Dit password er ikke ændret."));
                    return View("ForgotPwdFailure");
                }
            }
            else
            {
                Services.Notifier.Information(T("Den angivne email er ikke kendt i systemet. The email entered is not a known user."));
                return View("ForgotPwdNoSuchUser");
            }
        }

        // Metoden GeneratePasswords() kaldes fra browserens adresselinie.
        // Der er sat en route op i Routes.cs, som muliggør dette - slå route til og fra efter behov
        [Authorize]
        public ActionResult GeneratePasswords()
        {
            // check for user name admin
            if (Services.WorkContext.CurrentUser.UserName == "admin")
            {
                var XDoc = _passwordService.XDocUidPwd();
                Services.Notifier.Information(T("GeneratePasswords method has executed"));
                return Redirect("~/Endpoint");
            }
            else
            {
                Services.Notifier.Information(T("Please log in as Admin user in order to access this method"));
                return Redirect("~/Endpoint");
            }
        }

        // Metoden TestWelcomeMail() kaldes fra browserens adresselinie.
        // Der er sat en route op i Routes.cs, som muliggør dette - slå route til og fra efter behov
        [Authorize]
        public ActionResult TestWelcomeEmail()
        {
            // check for user name admin
            if (Services.WorkContext.CurrentUser.UserName == "admin")
            {
                _passwordService.TestWelcomeEmail();
                Services.Notifier.Information(T("TestWelcomeEmail method has executed"));
                return Redirect("~/Endpoint");
            }
            else
            {
                Services.Notifier.Information(T("Please log in as Admin user in order to access this method"));
                return Redirect("~/Endpoint");
            }
        }

        // Metoden public void WelcomeEmailLogin() kaldes fra link i velkomst-email eller fra browserens adresselinie.
        // Der er sat en route op i Routes.cs, som muliggør dette - slå route til og fra efter behov
        public ActionResult WelcomeEmailLogin(string userName, string pass)
        {
            try
            {
                IUser user = _membershipService.ValidateUser(userName, pass);
                _authenticationService.SignIn(user, true);
                Services.Notifier.Information(T("Velkommen til ÅSV's nye hjemmeside - skift venligst dit midlertidige password til et permanent herunder!"));
                return Redirect("~/Users/Account/ChangePassword");
            }
            catch (Exception e)
            {
                Services.Notifier.Information(T("Login er desværre mislykket - prøv at bruge det tilsendte password, eller kontakt venligst webmaster Finn Vilsbæk på mvchead@gmail.com."));
                var msg = e.Message + e.InnerException + e.StackTrace;
                // TODO: Log error message, send mail til webmaster            
                return Redirect("~/Users/Account/LogOn");
            }
        }

        // Metoden public void ImportAllArtists() kaldes fra browserens adresselinie.
        // Der er sat en route op i Routes.cs, som muliggør dette - slå route til og fra efter behov
        // Translation: specialized XML import of users from an old system.
        [Authorize]
        public ActionResult ImportAllArtists()
        {
            // check for user name admin
            if (Services.WorkContext.CurrentUser.UserName == "admin")
            {
                XDocument doc = XDocument.Load(Server.MapPath("~/App_Data/XmlFiles/users.xml"));

                var userTable = (from n in doc.Descendants("table")
                                 where n.Attribute("name").Value == "users"
                                 select n);

                foreach (XElement user in userTable)
                {
                    var newArtist = Services.ContentManager.New("Artist");
                    var newArtistProfilePart = newArtist.As<ProfilePart>();
                    var newArtistUserPart = newArtist.As<UserPart>();

                    var bDayArray = new string[3];

                    foreach (XElement c in user.Descendants("column"))
                    {
                        switch (c.Attribute("name").Value)
                        {
                            case "artistname":
                                if (c.Value != "")
                                    newArtistProfilePart.Kunstnernavn = ReplaceAeOeAa(c.Value);
                                else
                                    newArtistProfilePart.Kunstnernavn = "";
                                break;
                            case "name":
                                if (c.Value != "")
                                {
                                    var navn = c.Value.Split(' ');
                                    if (navn.Count() >= 2 && navn[0] != null && navn[1] != null)
                                    {
                                        newArtistProfilePart.Fornavn = ReplaceAeOeAa(navn[0]);
                                        newArtistProfilePart.Efternavn = ReplaceAeOeAa(navn[1]);
                                    }
                                    else
                                    {
                                        newArtistProfilePart.Fornavn = ReplaceAeOeAa(c.Value);
                                        newArtistProfilePart.Efternavn = "X";
                                    }
                                }
                                else
                                {
                                    newArtistProfilePart.Fornavn = "Mr.";
                                    newArtistProfilePart.Efternavn = "X";
                                }
                                break;
                            case "zip":
                                if (c.Value != "")
                                    newArtistProfilePart.Postnummer = c.Value;
                                else
                                    newArtistProfilePart.Postnummer = "";
                                break;
                            case "town":
                                if (c.Value != "")
                                    newArtistProfilePart.Bynavn = c.Value;
                                else
                                    newArtistProfilePart.Bynavn = "";
                                break;
                            case "street":
                                if (c.Value != "")
                                    newArtistProfilePart.Gade = c.Value;
                                else
                                    newArtistProfilePart.Gade = "";
                                break;
                            case "number":
                                if (c.Value != "")
                                    newArtistProfilePart.Husnummer = c.Value;
                                else
                                    newArtistProfilePart.Husnummer = "";
                                break;
                            case "bday":
                                if (c.Value != "")
                                    bDayArray[0] = c.Value;
                                else
                                    bDayArray[0] = "1";
                                break;
                            case "bmonth":
                                if (c.Value != "")
                                    bDayArray[1] = c.Value;
                                else
                                    bDayArray[1] = "1";
                                break;
                            case "byear":
                                if (c.Value != "")
                                    bDayArray[2] = c.Value;
                                else
                                    bDayArray[2] = "1";
                                break;
                            case "email":
                                if (c.Value != "")
                                    newArtistProfilePart.EmailAdresse = c.Value;
                                else
                                    newArtistProfilePart.EmailAdresse = "mister-x@x.dk";
                                break;
                            case "phone":
                                if (c.Value != "")
                                    newArtistProfilePart.Telefon = c.Value;
                                else
                                    newArtistProfilePart.Telefon = "";
                                break;

                            // description er udeladt grundet html tags, unicode mess og double quote "\ problemer
                            case "description":
                                if (c.Value != "")
                                {
                                    //c.Value = StripHTMLTags(c.Value);
                                    //c.Value = CleanInput(c.Value);
                                    //newArtistProfilePart.Profiltekst = c.Value;
                                    newArtistProfilePart.Profiltekst = "";
                                }
                                else
                                    newArtistProfilePart.Profiltekst = "";
                                break;

                            case "homepage":
                                if (c.Value != "")
                                    newArtistProfilePart.Hjemmeside = c.Value;
                                else
                                    newArtistProfilePart.Hjemmeside = "";
                                break;
                            default: break;
                        }
                    }

                    // TODO: set a default profile picture - use you own azure blob storage url here in place of mine.
                    // sæt standard profilbillede
                    newArtistProfilePart.ProfilePictureURL = "http://panmediablob.blob.core.windows.net/aasv-container/silhouette.jpg";
                    newArtistProfilePart.ProfilePictureName = "silhouette.jpg";

                    // sæt fødselsår og dato ind fra værdier i array
                    newArtistProfilePart.Foedselsaarogdato = bDayArray[0] + "-" + bDayArray[1] + "-" + bDayArray[2];
                    newArtistProfilePart.CreatedUtc = _clock.UtcNow;
                    newArtistProfilePart.LastEditedUtc = _clock.UtcNow;

                    // default til vis email / tlf for medlemmer
                    newArtistProfilePart.EmailVisibility = EmailVisibility.Medlemmer;
                    newArtistProfilePart.PhoneVisibility = PhoneVisibility.Medlemmer;

                    // Set up new user
                    newArtistUserPart.CreatedUtc = DateTime.UtcNow;
                    newArtistUserPart.UserName = newArtistProfilePart.EmailAdresse;
                    newArtistUserPart.Email = newArtistProfilePart.EmailAdresse;
                    newArtistUserPart.NormalizedUserName = newArtistProfilePart.EmailAdresse.ToLowerInvariant();
                    newArtistUserPart.Record.HashAlgorithm = "SHA1";
                    newArtistUserPart.Record.RegistrationStatus = UserStatus.Approved;
                    newArtistUserPart.Record.EmailStatus = UserStatus.Approved;
                    // set default pwd until user logs in and changes it - or send user a random pwd with a mail?
                    _membershipService.SetPassword(newArtistUserPart.As<IUser>(), "1234");

                    // Set title for item - important!
                    if ((newArtistProfilePart.Fornavn + " " + newArtistProfilePart.Efternavn) != "")
                    {
                        newArtist.As<TitlePart>().Title = newArtistProfilePart.Fornavn + " " + newArtistProfilePart.Efternavn;
                    }
                    else newArtist.As<TitlePart>().Title = "Joe Doe";


                    // Persist and store new artist content item
                    Services.ContentManager.Create(newArtist);
                }

                Services.Notifier.Information(T("TestWelcomeEmail method has executed"));
                return Redirect("~/Endpoint");

            }
            else
            {
                Services.Notifier.Information(T("Please log in as Admin user in order to access this method"));
                return Redirect("~/Endpoint");
            }
        }

        /// <summary>
        /// Helper methods for html tag removal from description field in user import:
        /// 1) Kald til StripHTMLTags, og pass output herfra til
        /// 2) CleanInput
        /// 
        /// Also, a helper method to replace unicode chars with æ, ø, and å
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripHTMLTags(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty spaces. Then take care of double quotes.
            try
            {
                strIn = Regex.Replace(strIn, @"[^\w\.@-]", " ", RegexOptions.None, TimeSpan.FromSeconds(1.5));
                strIn = strIn.Replace("\"\\", "");
                strIn = strIn.Replace("\'", "");

                return Regex.Replace(strIn, "\"[^\"]*\"", string.Empty);
            }
            // If we timeout when replacing invalid characters, we should return empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public static string ReplaceAeOeAa(string strIn)
        {
            // Replace weird unicodes with Ae, Oe, and Aa
            try
            {
                strIn = strIn.Replace("Ã¦", "æ");
                strIn = strIn.Replace("Ã¸", "ø");
                strIn = strIn.Replace("Ã¥", "å");

                return strIn;
            }
            // If something happens when replacing invalid characters, we should return strIn.
            catch (Exception e)
            {
                return strIn;
            }

        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}