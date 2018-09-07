using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Panmedia.Artist
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                            // highest priority
                             new RouteDescriptor {   Priority = 8,
                                                     Route = new Route(
                                                         "Profile/SignUp",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"},
                                                                                      {"controller", "Artist"},
                                                                                      {"action", "SignUp"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },
                            // next highest priority
                             new RouteDescriptor {   Priority = 7,
                                                     Route = new Route(
                                                         "Profile/ImageEdit",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"},
                                                                                      {"controller", "Artist"},
                                                                                      {"action", "ImageEdit"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },

                             //// next highest priority
                             //new RouteDescriptor {   Priority = 7,
                             //                        Route = new Route(
                             //                            "Profile/SetUserProfilePicture/{username}",
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"},
                             //                                                         {"controller", "Artist"},
                             //                                                         {"action", "SetUserProfilePicture"},

                             //                            },
                             //                            new RouteValueDictionary (),
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"}
                             //                                                     },
                             //                            new MvcRouteHandler())

                             //},

                            

                            // higher priority than Profile/{username} as Edit could be interpreted as a username
                             new RouteDescriptor {   Priority = 6,
                                                     Route = new Route(
                                                         "Profile/Edit",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"},
                                                                                      {"controller", "Artist"},
                                                                                      {"action", "Edit"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },
                             new RouteDescriptor {   Priority = 5,
                                                     Route = new Route(
                                                         "Profile/{username}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"},
                                                                                      {"controller", "Artist"},
                                                                                      {"action", "Index"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },

                             // Route for Profile/{userId} - used in ProfileListing.cshtml
                             new RouteDescriptor {   Priority = 5,
                                                     Route = new Route(
                                                         "ShowProfile/{userId}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"},
                                                                                      {"controller", "Artist"},
                                                                                      {"action", "ViewProfile"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },

                             // For testing server side image cropping
                             new RouteDescriptor {   Priority = 4,
                                                     Route = new Route(
                                                         "Image/CropImage",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"},
                                                                                      {"controller", "Image"},
                                                                                      {"action", "CropImage"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },

                             // For Avatar upload test
                             new RouteDescriptor {   Priority = 3,
                                                     Route = new Route(
                                                         "Avatar/Save",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"},
                                                                                      {"controller", "Avatar"},
                                                                                      {"action", "Save"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },

                             // For Artist upload
                             //new RouteDescriptor {   Priority = 2,
                             //                        Route = new Route(
                             //                            "Artist/Import",
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"},
                             //                                                         {"controller", "Artist"},
                             //                                                         {"action", "ImportAllArtists"},

                             //                            },
                             //                            new RouteValueDictionary (),
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"}
                             //                                                     },
                             //                            new MvcRouteHandler())

                             //},

                             // For Password Generation / Send Welcome Email to all users
                             //new RouteDescriptor {   Priority = 2,
                             //                        Route = new Route(
                             //                            "Artist/PwdGen",
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"},
                             //                                                         {"controller", "Artist"},
                             //                                                         {"action", "GeneratePasswords"},

                             //                            },
                             //                            new RouteValueDictionary (),
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"}
                             //                                                     },
                             //                            new MvcRouteHandler())

                             //},

                             // For Welcome Email Testing to beta test team
                             //new RouteDescriptor {   Priority = 2,
                             //                        Route = new Route(
                             //                            "Artist/TestWelcomeEmail",
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"},
                             //                                                         {"controller", "Artist"},
                             //                                                         {"action", "TestWelcomeEmail"},

                             //                            },
                             //                            new RouteValueDictionary (),
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"}
                             //                                                     },
                             //                            new MvcRouteHandler())

                             //},

                             // For Welcome Email Login to beta test team and all users
                             //new RouteDescriptor {   Priority = 2,
                             //                        Route = new Route(
                             //                            "Artist/WelcomeEmailLogin", // (string userName, string pass) is passed w querystring
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"},
                             //                                                         {"controller", "Artist"},
                             //                                                         {"action", "WelcomeEmailLogin"},

                             //                            },
                             //                            new RouteValueDictionary (),
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.Artist"}
                             //                                                     },
                             //                            new MvcRouteHandler())

                             //},

                             // For Profile List
                             new RouteDescriptor {   Priority = 1,
                                                     Route = new Route(
                                                         "Artist/ProfileListing",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"},
                                                                                      {"controller", "Artist"},
                                                                                      {"action", "ProfileListing"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },
                             // Forgotten Password
                             new RouteDescriptor {   Priority = 1,
                                                     Route = new Route(
                                                         "Artist/ForgotPwd",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"},
                                                                                      {"controller", "Artist"},
                                                                                      {"action", "ForgotPwd"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.Artist"}
                                                                                  },
                                                         new MvcRouteHandler())

                             }
                         };
        }
    }
}