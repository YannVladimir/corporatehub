using CorporateHub.Models;
using Microsoft.AspNetCore.Hosting;
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
	public class JobApplicationSurfaceController : SurfaceController
	{
		private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };
		private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IEmailSender _emailSender;
		private readonly ILogger<JobApplicationSurfaceController> _logger;

		public JobApplicationSurfaceController(
			IUmbracoContextAccessor umbracoContextAccessor,
			IUmbracoDatabaseFactory databaseFactory,
			ServiceContext services,
			AppCaches appCaches,
			IProfilingLogger profilingLogger,
			IPublishedUrlProvider publishedUrlProvider,
			IWebHostEnvironment webHostEnvironment,
			IEmailSender emailSender,
			ILogger<JobApplicationSurfaceController> logger)
			: base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
		{
			_webHostEnvironment = webHostEnvironment;
			_emailSender = emailSender;
			_logger = logger;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HandleSubmit(JobApplicationViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return CurrentUmbracoPage();
			}

			var resume = model.Resume!;
			var extension = Path.GetExtension(resume.FileName).ToLowerInvariant();

			if (!AllowedExtensions.Contains(extension))
			{
				ModelState.AddModelError(string.Empty, "Resume must be a PDF, DOC, or DOCX file.");
				return CurrentUmbracoPage();
			}

			if (resume.Length > MaxFileSizeBytes)
			{
				ModelState.AddModelError(string.Empty, "Resume must be smaller than 5 MB.");
				return CurrentUmbracoPage();
			}

			// Saved outside wwwroot entirely - not web-servable at all, unlike
			// wwwroot/ or the Media Library, which would both give a sensitive
			// personal document a guessable, publicly downloadable URL.
			var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "JobApplications");
			Directory.CreateDirectory(uploadsFolder);

			var storedFileName = $"{Guid.NewGuid()}{extension}";
			var filePath = Path.Combine(uploadsFolder, storedFileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await resume.CopyToAsync(stream);
			}

			// IFormFile.OpenReadStream() can be called again here for a fresh read -
			// the one above (for saving to disk) was already consumed and disposed.
			using (var attachmentStream = resume.OpenReadStream())
			{
				// The attachment-supporting constructor takes string[] recipients, not a
				// single string - different overload than the simple one Contact uses.
				var message = new EmailMessage(
					from: "noreply@corporatehub.local",
					to: new[] { "careers@corporatehub.local" },
					cc: Array.Empty<string>(),
					bcc: Array.Empty<string>(),
					replyTo: Array.Empty<string>(),
					subject: $"New job application from {model.Name}",
					body: $"From: {model.Name} ({model.Email})\n\n{model.CoverLetter}",
					isBodyHtml: false,
					attachments: new[] { new EmailMessageAttachment(attachmentStream, resume.FileName) });

				try
				{
					await _emailSender.SendAsync(message, "JobApplicationNotification");
					_logger.LogInformation("Job application email sent for {Email}", model.Email);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send job application email for {Email}", model.Email);
				}
			}

			return RedirectToCurrentUmbracoPage();
		}
	}
}
