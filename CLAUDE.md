# CorporateHub — Umbraco Learning Project

## Purpose

This is **not** a product build — it's a deliberate learning vehicle. The user wants to
deeply learn Umbraco CMS (document types, templates, members, custom .NET dev, APIs,
deployment) well enough to confidently join an existing Umbraco codebase, not just
follow isolated tutorials. We picked one realistic project — a corporate website where
a marketing team manages everything through the backoffice — and are using it to touch
every major Umbraco concept in sequence.

**Working style — read this before writing any code:**
- Act as a guide, not an autocomplete. Explain *why* a concept exists in Umbraco before
  implementing it.
- Many steps are backoffice/UI-driven, not code — walk the user through precise
  click-paths rather than doing it for them.
- For code steps: scaffold a minimal example, explain the Umbraco API/pattern being used
  (e.g. `IPublishedContent`, `UmbracoViewPage`, DI-injected Services), then give the user
  a concrete exercise to extend it themselves, and review what they write.
- No full-site code dumps. The point is the user builds the muscle memory.

## Decisions already made

- Stack: **Visual Studio 2022 + ASP.NET Core + Razor** (traditional MVC rendering —
  not headless/React; that's an optional later phase once fundamentals are solid)
- Database: **SQLite** (Umbraco's zero-config default) — SQL Server LocalDB
  (`MSSQLLocalDB`) is available on the machine as a fallback if we want to practice a
  real connection string later
- Umbraco version: **18.3.2** templates, targeting **.NET 10** (SDK 10.0.404 already
  installed alongside an older 7.0.306)
- Solution format: `dotnet new sln` now generates `CorporateHub.slnx` (new XML solution
  format, not the old `.sln`) — Visual Studio 2022 17.13+ opens it fine

## Curriculum roadmap (learning target, not a deadline)

1. Site structure & content mgmt — Document Types, Compositions, Data Types, Content
   Tree, Media Library, Templates, Partial Views, navigation/breadcrumbs
2. Dynamic content — News/Articles, Categories/Tags, Element Types, Block List, Block
   Grid, reusable components, search/pagination
3. Advanced content — Multilingual (EN/FR/RW), scheduling, drafts/versioning, content
   permissions, SEO metadata
4. Members & auth — Member Types/Groups, registration, login, protected pages
5. Forms & integrations — Contact/job forms, validation, email notifications, file
   uploads, external + custom APIs
6. Custom dev — Controllers/ViewComponents, DI, Umbraco Services, Notifications/events,
   custom dashboards, custom property editors, EF Core tables
7. Professional practices — Git, environments, logging, testing, caching, security,
   CI/CD, deployment
8. (Optional, later) Rebuild frontend headless via Content Delivery API + React

## Milestone 1 — complete: Foundation

Goal: a running Umbraco site with real Document Types and dynamic Razor rendering —
deliberately not a starter-kit site, so content ↔ template wiring has to be understood,
not copied.

- [x] 1. Confirm Umbraco version + .NET SDK — done, see Decisions above
- [x] 2. Update `Umbraco.Templates` to latest (18.3.2) — done
- [x] 3. Scaffold `CorporateHub` solution (`dotnet new umbraco`, SQLite) — done, project
      created at the repo root alongside `CorporateHub.slnx`
- [x] 4. First run: install wizard — ran `dotnet run`, chose SQLite, created admin
      user, confirmed landing in `/umbraco` — done
- [x] 5. Walk the backoffice sections (Content tree, Media library, Settings, Users)
      — done; also found a top-level **Library** section (holds Element Types,
      separate from page Document Types in Settings — relevant once we build
      Block List/Grid in Milestone 2)
- [x] 6. Create `HomePage` Document Type (with a Compositions-based reusable "SEO" group)
      — done. `HomePage` has a `Content` tab (Heading, Intro Text) + a composed `SEO`
      tab (Browser Title, Meta Description, Open Graph Image). Allowed at root.
- [x] 7. Create `AboutPage` and `ServicePage` Document Types, demonstrating Compositions
      reuse — done. Also added a `ServicesLandingPage` type (not in the original plan,
      added to support a nested nav: `Home > About, Services > Web Design, Consulting`).
      All three compose the `SEO` group. Content tree built and published to match.
- [x] 8. Shared Razor layout + Homepage template rendering real content from the tree
      — done. `Views/_ViewStart.cshtml` + `Views/Shared/_Layout.cshtml` wrap every
      page; `HomePage`/`AboutPage`/`ServicesLandingPage`/`ServicePage` views render
      their real properties via Models Builder (`Model.Heading`, etc.)
- [x] 9. Dynamic navigation menu driven by the content tree — done. Built as a
      reusable Partial View (`Views/Partials/Navigation.cshtml`), called from
      `_Layout.cshtml` via `<partial name="Partials/Navigation" model="Model" />`.
      Walks `Model.Root().Children()` for top-level links, and each item's own
      `.Children()` for a nested dropdown (currently just under "Services").

**Milestone 1 (Foundation) is fully complete.** Running site, real Document Types,
Compositions reused across four page types, a shared layout, and a tree-driven nested
nav — all built by hand, not from a starter kit.

## Milestone 2 — complete: Dynamic Content

Goal: give the marketing team an ongoing stream of content (articles/news) instead of
fixed one-off pages — the listing/detail/tree pattern, Block List for structured body
content, and search/pagination once volume grows.

- [x] 1. Create `ArticlesLandingPage` (listing) + `Article` (detail) Document Types
      — done. Simple Richtext `Body` for now (deliberate choice — block-based body
      is its own step, #3 below, so this step stayed focused on listing/detail/tree
      mechanics). Both compose `SEO`. Tree: `Home > News > My first article` (+ 1 more).
      `ArticlesLandingPage.cshtml` lists children via `Model.Children<ContentModels.Article>()`.
- [x] 2. Add Categories & Tags and filter articles by them — done. `Category` is a
      bare Document Type (no template, no custom properties — the built-in `Name`
      suffices), "Allow at root" so instances live as root-level siblings of `Home`
      (keeps them out of the nav, since `Navigation.cshtml` only walks `Home`'s own
      children). A `Category Picker` Data Type (Content Picker / MultiNodeTreePicker,
      "Accepted types" restricted to `Category`) powers `Article.Categories`.
      `ArticlesLandingPage.cshtml` filters via `Umbraco.ContentAtRoot()` +
      `Context.Request.Query["category"]`.
- [x] 3. Convert `Article`'s `Body` to a Block List property — done. `Text Block`
      Element Type (one `Text` Richtext property) + `Article Body Blocks` Data Type
      (Block List, available block: Text Block). Umbraco does NOT auto-create a
      per-Element-Type render partial (unlike full page templates) — had to hand-write
      `Views/Partials/blocklist/Components/textBlock.cshtml`, rendered from
      `Article.cshtml` via `@await Html.PartialAsync("blocklist/default", Model.Body)`.
- [x] 4. Build a Block Grid property for flexible, marketing-editable page sections —
      done. Added `Image Block` Element Type (Media Picker) alongside the existing
      `Text Block`. `HomePage Blocks` Data Type (Block Grid, both blocks available),
      property `Page Sections` on `HomePage`. Needed its own
      `Views/Partials/blockgrid/Components/{alias}.cshtml` per block (separate from
      the `blocklist/Components/` ones — different model type, `BlockGridItem` vs
      `BlockListItem` — even for the same `Text Block` Element Type). Not using
      Areas (nested block containers) yet — flat row of blocks only.
- [x] 5. Extract a reusable article teaser card as a Partial View — done.
      `Views/Partials/ArticleTeaser.cshtml` (`@inherits UmbracoViewPage<ContentModels.Article>`),
      called from `ArticlesLandingPage.cshtml`'s loop via
      `<partial name="Partials/ArticleTeaser" model="article" />`. Same motivation as
      Compositions in Milestone 1 — edit the teaser once, every caller picks it up.
- [x] 6. Real pagination on the article listing — done, in `ArticlesLandingPage.cshtml`.
      `pageSize` deliberately small (1) for now so paging is visible with only 2 test
      articles — bump it up once there's more real content. `?page=N` combines with
      the existing `?category=X` filter via a local `PageUrl(int)` helper function so
      Previous/Next preserve the active category.
- [x] 7. Site search across articles and pages (Examine) — done. `SearchResultsPage`
      Document Type (child of `HomePage`, composes `SEO`, no custom properties — all
      content is dynamic), published as `Search` under `Home` (shows up in the nav
      automatically, no `Navigation.cshtml` changes needed). View uses `@inject
      IExamineManager`, queries the `ExternalIndex` (published-only) by `nodeName`
      with a wildcard match, and resolves each hit's ID back to real content via
      `Umbraco.Content(id)`.

**Milestone 2 (Dynamic Content) is fully complete.** Article listing/detail with
Categories, Block List + Block Grid, a reusable teaser partial, pagination, and
site search — all built from scratch, same "understand the wiring" standard as
Milestone 1.

## Milestone 3 — current focus: Advanced Content

Goal: the editorial realities of a real site — multiple languages, scheduling,
versioning, who's allowed to edit what, and how pages get found on Google.

- [x] 1. Enable multilingual setup — done. English (default) + French + Kinyarwanda
      all exist in Settings → Languages (Kinyarwanda was confirmed present in this
      install's culture list — not something I was certain of in advance).
- [ ] 2. Mark properties as culture-variant vs invariant; translate Home/About content
      — **not done yet, this is the next step**
- [ ] 3. Add a language switcher to the shared layout
- [ ] 4. Explore scheduling — publish/unpublish a node at a future date/time
- [ ] 5. Explore drafts & versioning — edit a published page, compare versions, roll back
- [ ] 6. Set up content permissions — restrict a content branch to a user group
- [ ] 7. Add the remaining SEO metadata (meta description, Open Graph tags) to
      `_Layout.cshtml` — note: the `<title>` itself is already wired from the `SEO`
      composition since Milestone 1; this step just fills in what's still missing.

## Next step when you pick this back up

Mark properties as culture-variant vs invariant, then translate Home/About content —
see Milestone 3 step 2 above.

Git workflow: one checklist item = one branch = one push = one PR, branched fresh off
`main` each time. Never push straight to `main`.
