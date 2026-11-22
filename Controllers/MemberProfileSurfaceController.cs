using CorporateHub.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace CorporateHub.Controllers
{
	public class MemberProfileSurfaceController : SurfaceController
	{
		private readonly IMemberManager _memberManager;
		private readonly IMemberService _memberService;

		public MemberProfileSurfaceController(
			IUmbracoContextAccessor umbracoContextAccessor,
			IUmbracoDatabaseFactory databaseFactory,
			ServiceContext services,
			AppCaches appCaches,
			IProfilingLogger profilingLogger,
			IPublishedUrlProvider publishedUrlProvider,
			IMemberManager memberManager,
			IMemberService memberService)
			: base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
		{
			_memberManager = memberManager;
			_memberService = memberService;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HandleUpdateProfile(EditProfileViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return CurrentUmbracoPage();
			}

			var currentUser = await _memberManager.GetCurrentMemberAsync();
			if (currentUser is null)
			{
				return RedirectToCurrentUmbracoPage();
			}

			var members = await _memberService.GetByKeysAsync(new[] { currentUser.Key });
			var member = members.FirstOrDefault();
			if (member is null)
			{
				return RedirectToCurrentUmbracoPage();
			}

			member.SetValue("fullName", model.FullName);
			member.SetValue("address", model.Address);
			_memberService.Save(member);

			return RedirectToCurrentUmbracoPage();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HandleChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return CurrentUmbracoPage();
			}

			var currentUser = await _memberManager.GetCurrentMemberAsync();
			if (currentUser is null)
			{
				return RedirectToCurrentUmbracoPage();
			}

			var result = await _memberManager.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return CurrentUmbracoPage();
			}

			return RedirectToCurrentUmbracoPage();
		}
	}
}
