using Orchard;
using System.Xml.Linq;

namespace Panmedia.Artist.Services
{
    public interface IPasswordService : IDependency
    {        
        XDocument XDocUidPwd();
        bool ForgottenPassword(string userEmail);
        void SetPwdForUser(string userEmail, string pwd);        
        bool SendPwdToSingleUserViaSMTPChannel(string userEmail, string pwd);
        void TestWelcomeEmail();
    }
       
}