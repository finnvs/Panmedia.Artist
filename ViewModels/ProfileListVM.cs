using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.Artist.ViewModels
{
    public class ProfileListVM
    {
        public IList<ArtistInfo> Artists { get; set; }
        public dynamic Pager { get; set; }
    }

    public class ArtistInfo
    {
        public string ArtistTitle { get; set; }
        public string ArtistUserName { get; set; }
        public int ArtistId { get; set; }
    }
}