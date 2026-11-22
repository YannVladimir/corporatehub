using CorporateHub.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;

namespace CorporateHub.Controllers
{
	public class MemberRegistrationSurfaceController : SurfaceController
	{
		private readonly IMemberManager _memberManager;
		private readonly MemberSignInManager _memberSignInManager;

		public MemberRegistrationSurfaceController(
			IUmbracoContextAccessor umbracoContextAccessor,
			IUmbracoDatabaseFactory databaseFactory,
			ServiceContext services,
			AppCaches appCaches,
			IProfilingLogger profilingLogger,
			IPublishedUrlProvider publishedUrlProvider,
			IMemberManager memberManager,
			MemberSignInManager memberSignInManager)
			: base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
		{
			_memberManager = memberManager;
			_memberSignInManager = memberSignInManager;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HandleRegister(RegisterMemberViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return CurrentUmbracoPage();
			}

			var identityUser = MemberIdentityUser.CreateNew(
				username: model.Email,
				email: model.Email,
				memberTypeAlias: "websiteMember",
				isApproved: true,
				name: model.FullName);

			var result = await _memberManager.CreateAsync(identityUser, model.Password);

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return CurrentUmbracoPage();
			}

			await _memberSignInManager.SignInAsync(identityUser, isPersistent: false);

			return RedirectToCurrentUmbracoPage();
		}
	}
}
