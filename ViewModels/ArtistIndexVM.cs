using Panmedia.Artist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.Artist.ViewModels
{
    public class ArtistIndexVM
    {
        public IList<ArtistEntry> Artists { get; set; }
        public ArtistIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class ArtistEntry
    {
        public string ArtistTitle { get; set; }               
        public ProfilePartRecord Artist { get; set; }
        public bool IsChecked { get; set; }
        public bool IsOrphan { get; set; }
    }

    public class ArtistIndexOptions
    {
        public ArtistIndexFilter Filter { get; set; }
        public ArtistIndexBulkAction BulkAction { get; set; }
    }

    public enum ArtistIndexBulkAction
    {
        None,
        //Orphans,
        //MatchedPairs,
        Delete
    }

    public enum ArtistIndexFilter
    {
        All,
        Orphans,
        MatchedPairs
    }
}