using CorporateHub.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;

namespace CorporateHub.Controllers
{
	public class MemberLoginSurfaceController : SurfaceController
	{
		private readonly MemberSignInManager _memberSignInManager;

		public MemberLoginSurfaceController(
			IUmbracoContextAccessor umbracoContextAccessor,
			IUmbracoDatabaseFactory databaseFactory,
			ServiceContext services,
			AppCaches appCaches,
			IProfilingLogger profilingLogger,
			IPublishedUrlProvider publishedUrlProvider,
			MemberSignInManager memberSignInManager)
			: base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
		{
			_memberSignInManager = memberSignInManager;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HandleLogin(LoginMemberViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return CurrentUmbracoPage();
			}

			var result = await _memberSignInManager.PasswordSignInAsync(
				model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

			if (!result.Succeeded)
			{
				ModelState.AddModelError(string.Empty, "Invalid email or password.");
				return CurrentUmbracoPage();
			}

			return RedirectToCurrentUmbracoPage();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HandleLogout()
		{
			await _memberSignInManager.SignOutAsync();
			return RedirectToCurrentUmbracoPage();
		}
	}
}
