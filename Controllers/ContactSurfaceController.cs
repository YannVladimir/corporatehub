using CorporateHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace CorporateHub.Controllers
{
	public class ContactSurfaceController : SurfaceController
	{
		private readonly IEmailSender _emailSender;
		private readonly ILogger<ContactSurfaceController> _logger;

		public ContactSurfaceController(
			IUmbracoContextAccessor umbracoContextAccessor,
			IUmbracoDatabaseFactory databaseFactory,
			ServiceContext services,
			AppCaches appCaches,
			IProfilingLogger profilingLogger,
			IPublishedUrlProvider publishedUrlProvider,
			IEmailSender emailSender,
			ILogger<ContactSurfaceController> logger)
			: base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
		{
			_emailSender = emailSender;
			_logger = logger;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HandleSubmit(ContactMessageViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return CurrentUmbracoPage();
			}

			var message = new EmailMessage(
				from: "noreply@corporatehub.local",
				to: "contact@corporatehub.local",
				subject: $"New contact message from {model.Name}",
				body: $"From: {model.Name} ({model.Email})\n\n{model.Message}",
				isBodyHtml: false);

			try
			{
				await _emailSender.SendAsync(message, "ContactFormNotification");
				_logger.LogInformation("Contact form email sent for {Email}", model.Email);
			}
			catch (Exception ex)
			{
				// Don't let a failed email send break the visitor's experience - log it
				// and still confirm submission. No SMTP is configured in dev yet, so
				// this is expected to fail here until real credentials are plugged in.
				_logger.LogError(ex, "Failed to send contact form email for {Email}", model.Email);
			}

			return RedirectToCurrentUmbracoPage();
		}
	}
}
