using CorporateHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace CorporateHub.Controllers
{
	public class NewsletterSurfaceController : SurfaceController
	{
		private readonly ILogger<NewsletterSurfaceController> _logger;

		public NewsletterSurfaceController(
			IUmbracoContextAccessor umbracoContextAccessor,
			IUmbracoDatabaseFactory databaseFactory,
			ServiceContext services,
			AppCaches appCaches,
			IProfilingLogger profilingLogger,
			IPublishedUrlProvider publishedUrlProvider,
			ILogger<NewsletterSurfaceController> logger)
			: base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
		{
			_logger = logger;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult HandleSubscribe(NewsletterSignupViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return CurrentUmbracoPage();
			}

			// No real persistence yet - Milestone 6 step 8 adds a proper EF Core table
			// for this. For now, just confirm the signup was received.
			_logger.LogInformation("Newsletter signup received for {Email}", model.Email);

			return RedirectToCurrentUmbracoPage();
		}
	}
}
