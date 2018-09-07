using Orchard;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.Artist
{
    public class AdminMenu : INavigationProvider
    {
        public string MenuName
        {
            get { return "admin"; }
        }

        public AdminMenu()
        {
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            builder.AddImageSet("Artists")
                .Add(T("Artists"), "4",
                    menu => menu.Add(T("Artists"), "2", 
                        item => item.Action("ArtistIndex", "Admin", new { area = "Panmedia.Artist" })));            
        }
    }
}