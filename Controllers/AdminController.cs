using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.DisplayManagement;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Panmedia.Artist.Models;
using Panmedia.Artist.Services;
using Panmedia.Artist.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Panmedia.Artist.Controllers
{
    public class AdminController : Controller
    {
        private readonly IArtistUserService _artistService;
        private readonly ISiteService _siteService;

        dynamic Shape { get; set; }
        public AdminController(

            ISiteService siteService,
            IArtistUserService artistService,
            IShapeFactory shapeFactory
            )
        {
            _siteService = siteService;
            _artistService = artistService;
            Shape = shapeFactory;
        }

        // GET - Admin UI for Artists
        public ActionResult ArtistIndex(ArtistIndexOptions options, PagerParameters pagerParameters)
        {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // Default options
            if (options == null)
                options = new ArtistIndexOptions();

            // Filtering
            IContentQuery<ProfilePart, ProfilePartRecord> artistsQuery;
            switch (options.Filter)
            {
                case ArtistIndexFilter.All:
                    artistsQuery = _artistService.GetArtists();
                    break;
                case ArtistIndexFilter.Orphans:
                    throw new NotImplementedException();
                    //artistsQuery = _artistService.GetArtists(false);
                    break;
                case ArtistIndexFilter.MatchedPairs:
                    throw new NotImplementedException();
                    //artistsQuery = _artistService.GetArtists(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("options");
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(artistsQuery.Count());
            var entries = artistsQuery
                .OrderByDescending<ProfilePartRecord>(e => e.Id)
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList()
                .Select(e => CreateArtistEntry(e.Record));

            var model = new ArtistIndexVM
            {
                Artists = entries.ToList(),
                Options = options,
                Pager = pagerShape
            };
            return View((object)model);
        }

        public ArtistEntry CreateArtistEntry(ProfilePartRecord e)
        {
            if (e == null)
                throw new ArgumentNullException("Artist");
            return new ArtistEntry
            {
                ArtistTitle = e.Fornavn + " " + e.Efternavn,
                Artist = e,
                IsChecked = false,
                IsOrphan = _artistService.IsOrphan(e.Id)
            };
        }

        // POST - bulk delete action for Artist Index
        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ArtistIndex(FormCollection form)
        {
            var viewModel = new ArtistIndexVM { Artists = new List<ArtistEntry>(), Options = new ArtistIndexOptions() };
            UpdateModel(viewModel);

            IEnumerable<ArtistEntry> checkedEntries = viewModel.Artists.Where(s => s.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case ArtistIndexBulkAction.None:
                    break;
                //case ArtistIndexBulkAction.Open:
                //    foreach (ArtistEntry entry in checkedEntries)
                //    {
                //        throw new NotImplementedException();
                //        // _eventService.OpenEvent(entry.Event.Id);
                //    }
                //    break;
                //case ArtistIndexBulkAction.Close:
                //    foreach (ArtistEntry entry in checkedEntries)
                //    {
                //        throw new NotImplementedException();
                //        // _eventService.CloseEvent(entry.Event.Id);
                //    }
                //    break;
                case ArtistIndexBulkAction.Delete:
                    foreach (ArtistEntry entry in checkedEntries)
                    {
                        _artistService.DeleteArtist(entry.Artist.Id);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("form");
            }
            return RedirectToAction("ArtistIndex");
        }

        // POST - single delete action for Artist
        [HttpPost]
        public ActionResult DeleteArtist(int Id)
        {
            _artistService.DeleteArtist(Id);
            return RedirectToAction("ArtistIndex");
        }
    }
}