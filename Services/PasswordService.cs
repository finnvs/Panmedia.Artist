using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Panmedia.Artist.Models;
using Orchard.ContentManagement;
using Orchard;
using Orchard.Security;
using System.Net;
using System.Net.Mail;
using Orchard.Email.Services;
using System.Threading;
using Orchard.Logging;
using Orchard.Email.Models;
using System.Text;

namespace Panmedia.Artist.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly IArtistUserService _artistUserService;
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly ISmtpChannel _smtpChannel;
        public ILogger Logger { get; set; }

        public PasswordService(
            IArtistUserService artistUserService,
            IOrchardServices orchardServices,
            IMembershipService membershipService,
            ISmtpChannel smtpChannel
            )
        {
            _artistUserService = artistUserService;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _smtpChannel = smtpChannel;
            Logger = NullLogger.Instance;
        }

        public bool ForgottenPassword(string userEmail)
        {            
            if (_artistUserService.GetFirstName(userEmail) != "")
            {
                try
                {
                    var pwd = GenerateUniquePwd();                    
                    if (SendPwdToSingleUserViaSMTPChannel(userEmail, pwd))
                    {
                        SetPwdForUser(userEmail, pwd);
                        Logger.Information("Password successfully reset and sent to user "
                            + _artistUserService.GetFullName(userEmail) + " with email " + userEmail);
                        return true;
                    }
                    else return false;

                }
                catch (Exception e)
                {
                    Logger.Information("Password reset failure for user "
                            + _artistUserService.GetFullName(userEmail) + " with email "
                            + userEmail + ". Details: " + e.Message);
                    return false;
                }
            }
            return false;
        }

        public void SetPwdForUser(string userEmail, string pwd)
        {
            var userPart = _artistUserService.GetUserPart(userEmail);
            _membershipService.SetPassword(userPart.As<IUser>(), pwd);
        }

        public bool SendPwdToSingleUserViaSMTPChannel(string userEmail, string pwd)
        {
            ILogger logger = null;
            try
            {
                string fullName = _artistUserService.GetFullName(userEmail);

                var fakeLogger = new FakeLogger();
                var smtpChannelComponent = _smtpChannel as Component;
                if (smtpChannelComponent != null)
                {
                    logger = smtpChannelComponent.Logger;
                    smtpChannelComponent.Logger = fakeLogger;
                }

                // get SMTP settings from Orchard Admin UI
                var smtpSettings = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();

                StringBuilder htmlStr = new StringBuilder();

                htmlStr.Append("<!DOCTYPE html>");
                htmlStr.Append("<html>");
                htmlStr.Append("<head>");
                htmlStr.Append("<style type='text/css'>");
                htmlStr.Append("p{font-family:Tahoma,sans-serif;font-size:12px;margin:0}");
                htmlStr.Append("</style>");
                htmlStr.Append("</head>");
                htmlStr.Append("<body>");
                htmlStr.Append("<p>");
                htmlStr.Append("<div>");
                htmlStr.Append("Hej " + fullName
                                + " - din konto på http://sang-skriver.azurewebsites.net har nu følgende midlertidige password: "
                                + pwd + ". Log venligst ind for at ændre det. Med venlig hilsen, finnvs@gmail.com / webmaster@sang-skriver.dk.");
                htmlStr.Append("<br/>");
                htmlStr.Append("</div>");
                htmlStr.Append("</p>");
                htmlStr.Append("</body>");
                htmlStr.Append("</html>");


                if (!smtpSettings.IsValid())
                {
                    Logger.Error("Invalid settings.");
                }
                else
                {
                    _smtpChannel.Process(new Dictionary<string, object> {
                        {"Recipients", userEmail},
                        {"Subject", "Midlertidigt Password"},
                        {"Body", htmlStr + "For det tilfælde, at din email klient ikke kan modtage HTML er dit midlertidige password: " + pwd},
                        {"ReplyTo", "finnvs@gmail.com"},
                        {"Bcc", ""},
                        {"CC", ""}
                    });
                }

                if (!String.IsNullOrEmpty(fakeLogger.Message))
                {
                    return false; //Json(new { error = fakeLogger.Message });
                }

                return true; // Json(new { status = T("Message sent.").Text });
            }

            catch (Exception e)
            {
                Logger.Error("Password send failure for user "
                            + _artistUserService.GetFullName(userEmail) + " with email "
                           + userEmail + ". Details: " + e.Message + "   " + e.StackTrace);
                return false;
                // return Json(new { error = e.Message });
            }
            finally
            {
                var smtpChannelComponent = _smtpChannel as Component;
                if (smtpChannelComponent != null)
                {
                    smtpChannelComponent.Logger = logger;
                }
                
            }
        }

        public XDocument XDocUidPwd()
        {
            var UserList = GetUserList();
            XDocument XDocUidPwd =
                new XDocument(
                    new XElement("Users",
                    UserList.Select(x => new XElement("User",
                        new XAttribute("FullName", x.FullName),
                        new XAttribute("UserName", x.UserName),
                        new XAttribute("Pwd", x.Password)))
                    )
                );

            var Server = _orchardServices.WorkContext.HttpContext.Server;
            // XDocUidPwd.Save(Server.MapPath("~/App_Data/XmlFiles/XDocUidPwd.xml"));

            // Set password for each user in UserList / XDoc, and send off a mail
            foreach (var u in UserList)
            {
                SetPwdForUser(u.UserName, u.Password);                
                SendWelcomeEmailAndPwd(u.UserName, u.Password);
                // Thread.Sleep(2500); // wait before next mail is sent off
            }

            return XDocUidPwd;
        }

        public void TestWelcomeEmail()
        {
            var TestUserList = GetBetaTestUserList();
            foreach (var u in TestUserList)
            {
                SetPwdForUser(u.UserName, u.Password);
                SendWelcomeEmailAndPwd(u.UserName, u.Password);
                // Thread.Sleep(2500); // wait before next mail is sent off
            }
        }

        public bool SendWelcomeEmailAndPwd(string userEmail, string pwd)
        {
            ILogger logger = null;
            try
            {
                string fullName = _artistUserService.GetFullName(userEmail);

                var fakeLogger = new FakeLogger();
                var smtpChannelComponent = _smtpChannel as Component;
                if (smtpChannelComponent != null)
                {
                    logger = smtpChannelComponent.Logger;
                    smtpChannelComponent.Logger = fakeLogger;
                }

                // get SMTP settings from Orchard Admin UI
                var smtpSettings = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();

                StringBuilder htmlStr = new StringBuilder();

                htmlStr.Append("<!DOCTYPE html>");
                htmlStr.Append("<html>");
                htmlStr.Append("<head>");
                htmlStr.Append("<meta charset='utf- 8' />");
                htmlStr.Append("<title>Velkommen til ÅSV's hjemmeside</title>");
                htmlStr.Append("<style type='text/css'>");
                htmlStr.Append("p{font-family:Tahoma,sans-serif;font-size:12px;margin:0}");
                htmlStr.Append("</style>");
                htmlStr.Append("</head>");
                htmlStr.Append("<body>");
                htmlStr.Append("<p>");
                htmlStr.Append("<div>");

                // localhost testing               
                //var link = new StringBuilder();
                //link.Append("http://localhost:30321/OrchardLocal/Artist/WelcomeEmailLogin?userName=");
                //link.Append(userEmail);
                //link.Append("&pass=");
                //link.Append(pwd);

                // deploy to server
                var link = new StringBuilder();
                link.Append("http://sang-skriver.azurewebsites.net/Artist/WelcomeEmailLogin?userName=");
                link.Append(userEmail);
                link.Append("&pass=");
                link.Append(pwd);

                htmlStr.Append("Kære " + fullName);
                htmlStr.Append("<br/>");
                htmlStr.Append("<br/>");
                htmlStr.Append("I forbindelse med oprettelsen af ÅSV’s nye hjemmeside får du følgende midlertidige password: "
                    + pwd + ". <a href=" + link + ">Log venligst ind</a> for at ændre det til noget, der er nemmere at huske.");
                htmlStr.Append("<br/>");
                htmlStr.Append("<br/>");
                htmlStr.Append("Så kan du nominere og stemme for sange til ÅSE’rne, samt tilmelde dig Vinterlejr.");
                htmlStr.Append("<br/>");
                htmlStr.Append("<br/>");
                htmlStr.Append("Med venlig hilsen");
                htmlStr.Append("<br/>");
                htmlStr.Append("Finn Vilsbæk");
                htmlStr.Append("<br/>");
                htmlStr.Append("Webmaster");
                htmlStr.Append("<br/>");
                string siteLink = "http://sang-skriver.azurewebsites.net";
                htmlStr.Append("<a href=" + siteLink + ">Århus Sangskriver Værksted</a>");
                htmlStr.Append("<br/>");
                htmlStr.Append("finn@sang-skriver.dk / finnvs@gmail.com");
                htmlStr.Append("<br/>");
                htmlStr.Append("<br/>");
                htmlStr.Append("</div>");
                htmlStr.Append("</p>");
                htmlStr.Append("</body>");
                htmlStr.Append("</html>");


                if (!smtpSettings.IsValid())
                {
                    Logger.Error("Invalid settings.");
                }
                else
                {
                    _smtpChannel.Process(new Dictionary<string, object> {
                        {"Recipients", userEmail},
                        {"Subject", "Velkommen til ÅSV's webside"},
                        {"Body", htmlStr + "For det tilfælde, at din email klient ikke kan modtage HTML er dit midlertidige password: " + pwd},
                        {"ReplyTo", "finnvs@gmail.com"},
                        {"Bcc", ""},
                        {"CC", ""}
                    });
                }

                if (!String.IsNullOrEmpty(fakeLogger.Message))
                {
                    return false; //Json(new { error = fakeLogger.Message });
                }

                return true; // Json(new { status = T("Message sent.").Text });
            }

            catch (Exception e)
            {
                Logger.Error("Password send failure for user "
                            + _artistUserService.GetFullName(userEmail) + " with email "
                           + userEmail + ". Details: " + e.Message + "   " + e.StackTrace);
                return false;
                // return Json(new { error = e.Message });
            }
            finally
            {
                var smtpChannelComponent = _smtpChannel as Component;
                if (smtpChannelComponent != null)
                {
                    smtpChannelComponent.Logger = logger;
                }

            }
        }

        private List<XmlUserModel> GetUserList()
        {
            var UserList = new List<XmlUserModel>();
            var Artists = _artistUserService.GetArtists().List();

            foreach (var artist in Artists)
            {
                UserList.Add(new XmlUserModel
                {
                    FullName = artist.Fornavn + " " + artist.Efternavn,
                    UserName = artist.EmailAdresse,
                    Password = GenerateUniquePwd()
                });
            }

            return UserList;
        }

        private List<XmlUserModel> GetBetaTestUserList()
        {
            var UserList = new List<XmlUserModel>();
            // Fake artist: substitute your own email adress here
            var Artists = new List<string> { "youremail@yourdomain.com" };

            foreach (var artistEmail in Artists)
            {
                UserList.Add(new XmlUserModel
                {
                    FullName = _artistUserService.GetFullName(artistEmail),
                    UserName = artistEmail,
                    Password = GenerateUniquePwd()
                });
            }
            return UserList;
        }


        /// <summary>
        /// At the heart of this algorithm is the ability to randomly and easily sort a collection by using OrderBy(). 
        /// In the following code, OrderBy (e => Guid.NewGuid()) sorts the collection in random order, because for each element, 
        /// a new GUID is generated. The ASCII code for a is 97, and the code for A is 65. Take a look at the following two lines:
        /// Enumerable
        ///    .Range(65,26)
        ///    .Select (e => ((char)e).ToString())
        /// 
        /// This code generates the sequence ABCDEFGHIJKLMNOPQRSTUVWZYZabcdefghijklmnopqrstuvwxyz. 
        /// Next, we have the following line:
        /// 
        /// .Concat(Enumerable.Range(97,26).Select(e => ((char)e).ToString()))
        /// 
        /// This line appends the digits 0 to 9, so the sequence becomes ABCDEFGHIJKLMNOPQRSTUVWZYZabcdefghijklmnopqrstuvwxyz0123456789.
        /// Then the final call to OrderBy() randomly sorts the characters in this sequence, creating a different sequence every time. 
        /// Finally, it picks the first eight characters — and there you have it: a random serial.
        /// </summary>

        private string GenerateUniquePwd()
        {
            List<string> pwdCharList = new List<string>();

            Enumerable
                   .Range(65, 26)
                   .Select(e => ((char)e).ToString())
               .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
               .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
               .OrderBy(e => Guid.NewGuid())
               .Take(8)
               .ToList().ForEach(e => pwdCharList.Add(e));

            string pwd = "";

            foreach (var s in pwdCharList)
            {
                pwd += s;
            }

            return pwd;
        }

        private class FakeLogger : ILogger
        {
            public string Message { get; set; }

            public bool IsEnabled(LogLevel level)
            {
                return true;
            }

            public void Log(LogLevel level, Exception exception, string format, params object[] args)
            {
                Message = exception == null ? format : exception.Message;
            }
        }
    }
}