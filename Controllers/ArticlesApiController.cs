using CorporateHub.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace CorporateHub.Controllers
{
	[ApiController]
	[Route("api/articles")]
	public class ArticlesApiController : ControllerBase
	{
		private readonly IPublishedContentQuery _contentQuery;

		public ArticlesApiController(IPublishedContentQuery contentQuery)
		{
			_contentQuery = contentQuery;
		}

		[HttpGet]
		public ActionResult<IEnumerable<ArticleDto>> Get()
		{
			// Plain IPublishedContent + alias checks, not the ContentModels.* strongly-typed
			// classes - those only exist for .cshtml files under ModelsMode "InMemoryAuto",
			// not in a plain .cs class like this controller.
			var home = _contentQuery.ContentAtRoot().FirstOrDefault(c => c.ContentType.Alias == "homePage");
			var newsLanding = home?.Children().FirstOrDefault(c => c.ContentType.Alias == "articlesLandingPage");
			var articles = newsLanding?.Children().Where(c => c.ContentType.Alias == "article") ?? Enumerable.Empty<IPublishedContent>();

			var dtos = articles.Select(a => new ArticleDto
			{
				Title = a.Value<string>("heading") ?? string.Empty,
				Url = a.Url(),
				PublishedDate = a.CreateDate,
			});

			return Ok(dtos);
		}
	}
}
