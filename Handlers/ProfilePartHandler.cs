using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Panmedia.Artist.Models;
using Orchard.Users.Models;
using System;
using Orchard.Roles.Models;
using Orchard.Core.Title.Models;
using Orchard.Core.Common.Models;

namespace Panmedia.Artist.Handlers
{
    public class ProfilePartHandler : ContentHandler
    {
        public ProfilePartHandler(IRepository<ProfilePartRecord> repository)
        {                      
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<ProfilePart>("Artist"));
            Filters.Add(new ActivatingFilter<UserPart>("Artist"));
            Filters.Add(new ActivatingFilter<UserRolesPart>("Artist"));
            Filters.Add(new ActivatingFilter<TitlePart>("Artist"));           
        }        
    }
}