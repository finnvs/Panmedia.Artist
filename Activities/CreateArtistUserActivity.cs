using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Panmedia.Artist.Activities {
    [OrchardFeature("Panmedia.Artist.Workflows")]
    public class CreateArtistUserActivity : Task {
        private readonly IUserService _userService;
        private readonly IMembershipService _membershipService;

        public CreateArtistUserActivity(IUserService userService, IMembershipService membershipService) {
            _userService = userService;
            _membershipService = membershipService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "CreateArtistUser"; }
        }

        public override LocalizedString Category {
            get { return T("ArtistUser"); }
        }

        public override LocalizedString Description {
            get { return T("Creates a new Artist User based on the specified values."); }
        }

        public override string Form {
            get { return "CreateUser"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {
                T("InvalidUserNameOrEmail"),
                T("InvalidPassword"),
                T("UserNameOrEmailNotUnique"),
                T("Done")
            };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {

            // TODO: code here to generate a new Artist activityContext
            var userName = activityContext.GetState<string>("UserName");
            var email = activityContext.GetState<string>("Email");
            var password = activityContext.GetState<string>("Password");
            var approved = activityContext.GetState<bool>("Approved");

            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(email)) {
                yield return T("InvalidUserNameOrEmail");
                yield break;
            }

            if (String.IsNullOrWhiteSpace(password)) {
                yield return T("InvalidPassword");
                yield break;
            }

            if (!_userService.VerifyUserUnicity(userName, email)) {
                yield return T("UserNameOrEmailNotUnique");
                yield break;
            }

            // TODO: code here to set up an Artist and the ProfilePart with data gathered in form post

            var user = _membershipService.CreateUser(
                new CreateUserParams(
                    userName,
                    password,
                    email,
                    isApproved: approved,
                    passwordQuestion: null,
                    passwordAnswer: null));

            workflowContext.Content = user;

            yield return T("Done");
        }
    }
}