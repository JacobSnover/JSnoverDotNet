# Plan: Photo Gallery Feature with Security & Interactions

## TL;DR
Build a dedicated photo gallery page with carousel, browseable gallery, and detailed photo views. Support both standalone photos and existing blog photos (separate collections). Implement text comments (email-verified, admin-approved) and emoji reactions. Protect against bots via rate limiting + CAPTCHA, and AI scraping via robots.txt/meta tags. Architecture supports future authentication privilege levels. Target 85%+ test coverage with Playwright E2E validation.

## Multi-Phase Implementation

### Phase 1: Data Models & Database (Foundation) + Unit Tests
**Goal**: Extend database schema to support photo collections, comments, and reactions. Validate models are correct.

**Build Phase:**
1. Create [StandalonePhoto.cs](Models/) model:
   - PhotoId, Title, Description, Url, ThumbnailUrl, UploadDate, DisplayOrder
   - Collections for Comments and Reactions
   - Metadata for images (CreatedDate, IsPublished, Tags)

2. Create [PhotoComment.cs](Models/) model:
   - CommentId, PhotoId, Email, Name, Message, SubmitDate, IsVerified, IsApproved
   - Link to both StandalonePhoto and Blog photos

3. Create [PhotoReaction.cs](Models/) model:
   - ReactionId, PhotoId, ReactionType (👍, ❤️, 😂, etc.), SessionId, CreatedDate
   - Key: (PhotoId, SessionId, ReactionType) to prevent duplicate reactions per user

4. Create [RateLimitLog.cs](Models/) model:
   - LogId, IpAddress, Endpoint, Timestamp, RequestCount
   - Track guest access for rate limiting

5. Update [ApplicationDbContext.cs](Data/):
   - Add DbSet<StandalonePhoto>, DbSet<PhotoComment>, DbSet<PhotoReaction>, DbSet<RateLimitLog>
   - Configure relationships and constraints
   - Create migration

**Parallel with 1-5**: Review [existing Photos.cs](Models/) to confirm blog photo structure; ensure PhotoComment model can reference both blog photos and standalone photos via nullable foreign keys.

**Test Phase (Run After Build):**
1. Create [PhotoModelTests.cs](UnitTests/):
   - StandalonePhoto validation: required fields (Title, Url), max lengths, DisplayOrder >= 0
   - PhotoComment validation: email format, message not empty, max length (1000 chars)
   - PhotoReaction validation: valid reaction types, SessionId not empty
   - RateLimitLog validation: IpAddress format, Endpoint not empty

2. Create [DbContextTests.cs](UnitTests/):
   - Verify relationships: StandalonePhoto → Comments, StandalonePhoto → Reactions
   - Verify nullable FKs: PhotoComment can link to both Blog and StandalonePhoto
   - Verify constraints: unique (PhotoId, SessionId, ReactionType) on PhotoReaction

3. Create & apply database migration:
   - `dotnet ef migrations add AddPhotoGalleryTables`
   - Run: `dotnet test UnitTests/DbContextTests.cs --logger:"console;verbosity=detailed"`
   - Verify migration succeeds locally: `dotnet ef database update` on test DB

4. Code coverage check: Ensure models are 100% covered (simple data objects)

**Build Validation**: `dotnet build` should succeed with all model validations passing

### Phase 2: Data Services & Repository Layer + Unit & Integration Tests + Admin Dashboard
**Goal**: Implement business logic and data access patterns. Validate with comprehensive tests. Add admin dashboard for photo and comment management.

**Build Phase:**
1. Extend [JsnoRepo.cs](Infrastructure/SqlRepo/):
   - PhotoRepository methods: GetRandomPhotos(count), GetAllStandalonePhotos(), GetPhotoById()
   - CommentRepository methods: AddPhotoComment(), GetApprovedComments(), GetUnapprovedComments(), ApproveComment() [admin], RejectComment() [admin], DeleteComment() [admin]
   - ReactionRepository methods: AddReaction(), GetReactionCounts(), HasUserReacted()
   - RateLimitRepository methods: LogRequest(), IsRateLimited(ipAddress)

2. Create [PhotoGalleryService.cs](Infrastructure/Services/):
   - GetCarouselPhotos() - randomly picks from all photos
   - GetGalleryPhotos(filterBy: "all"|"blog"|"standalone") - paginated list
   - GetPhotoDetail(photoId) - includes comments + reactions
   - Business logic for comment moderation workflow
   - Photo filtering/searching

3. Create [RateLimitService.cs](Infrastructure/Services/):
   - CheckRateLimit(ipAddress, endpoint) → bool
   - LogRequest(ipAddress, endpoint)
   - Config: max 10 images/minute, max 5 reactions/minute per IP
   - Methods to bypass for authenticated users (stub for Phase 5)

4. Create [BotProtectionService.cs](Infrastructure/Services/):
   - GenerateCaptchaChallenge() - simple math CAPTCHA or reCAPTCHA v3
   - VerifyCaptcha(userResponse) → bool
   - Track failed CAPTCHA attempts
   - Trigger on suspicious activity (rate limit exceeded, etc.)

5. Create [ImageSecurityService.cs](Infrastructure/Services/):
   - GenerateRobotsMetaTags() - returns noindex directives
   - ApplyCatalogExclusionHeaders() - to prevent indexing
   - ValidateImageUrl() - basic security checks
   - Future: watermark/metadata stripping (stub methods)

6. Register new services in [Startup.cs](Startup.cs) as scoped/singleton

7. Create admin dashboard UI:
   - [Pages/Admin/PhotoManagement.razor](Pages/Admin/PhotoManagement.razor) - upload/edit/delete standalone photos
   - [Pages/Admin/CommentModeration.razor](Pages/Admin/CommentModeration.razor) - approve/reject/delete comments
   - [Components/AdminPhotoForm.razor](Components/) - form for uploading/editing photos
   - [Components/CommentModerationList.razor](Components/) - list of unapproved comments with approve/reject actions
   - Wire admin endpoints and authorization checks ([Authorize(Roles = "Admin")] attributes)

**Test Phase (Run After Build):**
1. Create [PhotoGalleryServiceTests.cs](UnitTests/) with mocked repository:
   - GetCarouselPhotos() returns random subset (run 5 times, verify randomness)
   - GetGalleryPhotos() filters correctly by collection type (all, blog-only, standalone-only)
   - GetGalleryPhotos() handles pagination (pages, 12-24 items per page)
   - GetPhotoDetail() includes all approved comments + reaction counts
   - Edge cases: empty collection, null photo ID, pagination boundaries

2. Create [RateLimitServiceTests.cs](UnitTests/) with mocked repository:
   - IsRateLimited(ip, endpoint) returns false under threshold (9 requests)
   - IsRateLimited(ip, endpoint) returns true over threshold (10+ requests)
   - LogRequest() increments counter correctly
   - Time-window reset logic works (10-min window, counter resets after window)
   - Multiple endpoints tracked separately

3. Create [BotProtectionServiceTests.cs](UnitTests/):
   - GenerateCaptchaChallenge() produces valid math problem with answer
   - VerifyCaptcha(correct answer) returns true
   - VerifyCaptcha(wrong answer) returns false
   - Failed attempt tracking increments
   - Challenge difficulty increases with failures (optional enhancement)

4. Create [PhotoGalleryIntegrationTests.cs](UnitTests/) with real test DB:
   - Repository + Service round-trip: add photo → retrieve via service → verify data
   - Add comment → service returns it in approved list (after admin approval)
   - Add reaction → service aggregates counts correctly
   - RateLimit logging → data persists to DB
   - Transaction rollback on error (simulate DB error, verify no partial write)

5. Create admin dashboard tests:
   - [AdminPhotoManagementTests.cs](UnitTests/) - upload/edit/delete photos via admin form
   - [CommentModerationTests.cs](UnitTests/) - approve/reject/delete comments
   - Verify [Authorize(Roles = "Admin")] enforcement on admin pages

6. Update [UnitTests.csproj](UnitTests/):
   - Add NUnit (if not present)
   - Add Moq for mocking
   - Add Microsoft.EntityFrameworkCore.InMemory for test DB context
   - Add code coverage tools (OpenCover or built-in)

**Build Validation**:
- Run: `dotnet test UnitTests/PhotoGalleryServiceTests.cs UnitTests/RateLimitServiceTests.cs UnitTests/BotProtectionServiceTests.cs --logger:"console;verbosity=detailed"`
- All tests must pass
- Run: `dotnet test UnitTests/PhotoGalleryIntegrationTests.cs` (uses test DB)
- Verify code coverage: `dotnet test /p:CollectCoverageReport=true` — target ≥85% on services layer

### Phase 3: UI Components & Pages + Component Tests
**Goal**: Build interactive Razor components with responsive design. Validate UI behavior with tests.

**Build Phase:**
1. Create [Photos.razor](Pages/) main page:
   - Layout with Bootstrap grid (existing design pattern)
   - Three sections: Carousel (top), Gallery Filter Tabs, Gallery Grid
   - Apply robots.txt meta tags + noindex here
   - Responsive mobile-first design matching existing site theme
   - Lazy-load images for performance optimization

2. Create [PhotoCarousel.razor](Components/):
   - Auto-rotating carousel with random photos from all collections on load
   - Navigation arrows, auto-play toggle
   - Display photo title/description overlay
   - CSS: [PhotoCarousel.razor.css](Components/) - match existing component styling

3. Create [PhotoGallery.razor](Components/):
   - Grid layout of photos with filter tabs: All | Blog Photos | Standalone
   - Pagination support (12-24 photos per page)
   - Click to view detail modal/page
   - Lazy-load images with `loading="lazy"` attribute for performance
   - CSS: [PhotoGallery.razor.css](Components/)

4. Create [PhotoDetail.razor](Components/):
   - Large photo display with title/description
   - Comments section (textarea, email input)
   - Quick reactions panel (emoji buttons)
   - Show existing approved comments + reaction counts
   - Form validation for comment input
   - Success message after comment submission
   - CSS: [PhotoDetail.razor.css](Components/)

5. Create [PhotoCommentSection.razor](Components/):
   - List of approved comments with timestamp
   - Comment form with email/name required fields
   - Admin approval badge for comments
   - CSS: [PhotoCommentSection.razor.css](Components/)

6. Create [PhotoReactionButtons.razor](Components/):
   - Row of emoji buttons for reactions (👍 ❤️ 😂 😮 😢)
   - Show current count for each reaction
   - Disable/grey-out reactions user already gave (tracked via SessionStorage)
   - OnClick triggers service to log reaction
   - CSS: [PhotoReactionButtons.razor.css](Components/)

7. Create [CaptchaComponent.razor](Components/):
   - Conditionally displayed modal for CAPTCHA challenge
   - Simple math problem or reCAPTCHA v3 integration
   - Verification logic client-side + server-side validation
   - CSS: [CaptchaComponent.razor.css](Components/)

**Test Phase (Run After Build):**
1. Create [PhotoCarouselComponentTests.cs](UnitTests/) — Test component behavior and rendering:
   - Component renders without error with mock PhotoGalleryService
   - Carousel displays random photo on load
   - Navigation arrows increment/decrement photo index
   - Auto-play timer advances carousel every 5 seconds
   - Auto-play toggle can pause/resume carousel

2. Create [PhotoGalleryComponentTests.cs](UnitTests/):
   - Gallery renders grid of photos from mock service
   - Filter tabs (All/Blog/Standalone) call service with correct filter
   - Pagination: next/previous buttons load correct page
   - Click on photo emits callback event to parent
   - Lazy-load attribute is set on images

3. Create [PhotoDetailComponentTests.cs](UnitTests/):
   - Component renders photo details from service
   - Comment form validates: email required, message not empty
   - Comment form submission calls service with correct data
   - Success message displays after submission
   - Reaction buttons display counts correctly
   - Reaction button click disables duplicate reaction type

4. Create [PhotoCommentSectionComponentTests.cs](UnitTests/):
   - Displays approved comments list correctly
   - Comment form has email/name fields
   - Form submission clears on success
   - Admin badge shows on admin comments (if user is admin)

5. Create [PhotoReactionButtonsComponentTests.cs](UnitTests/):
   - All 5 emoji buttons render
   - Counts display correctly
   - OnClick calls service log method
   - Previously-reacted button is disabled/greyed out (checked via SessionStorage)

6. Create [CaptchaComponentTests.cs](UnitTests/):
   - Component displays conditionally (only when needed)
   - Math problem renders with readable equation
   - Submit with correct answer calls onSuccess callback
   - Submit with wrong answer shows error message
   - Verification happens server-side as well

**Build Validation**:
- Run: `dotnet build` — All components compile without errors
- Run: `dotnet test UnitTests/*ComponentTests.cs --logger:"console;verbosity=detailed"`
- All component tests must pass

### Phase 4: Security & Anti-Scraping Implementation + Security Tests
**Goal**: Prevent bot/AI abuse without impacting real users. Validate security measures.

**Build Phase:**
1. Update [Photos.razor](Pages/):
   - Add `<meta name="robots" content="noindex, nofollow" />` via HeadContent component
   - Add comment meta tags for AI training protection
   - Render CaptchaComponent conditionally based on RateLimitService detection

2. Create/Update robots.txt in [wwwroot/](wwwroot/):
   - Disallow: /photos
   - User-agent: * rules to discourage scrapers

3. Middleware for rate limiting (Startup.cs ConfigureServices):
   - Extract IP from HttpContext.Connection.RemoteIpAddress
   - Check rate limits on API endpoints for photos, comments, reactions
   - Return 429 Too Many Requests + trigger CAPTCHA flow

4. Client-side rate limiting in photo components:
   - Debounce reaction clicks (500ms)
   - Disable reaction buttons during submission
   - Show "too many requests" message if rate limit hit
   - Trigger CAPTCHA modal programmatically

5. Comment spam prevention:
   - Email verification flow (send verification link using existing email service)
   - Admin approval workflow before display
   - Duplicate comment detection (same email, similar message content)

**Test Phase (Run After Build):**
1. Create [RateLimitSecurityTests.cs](UnitTests/):
   - RateLimitService blocks requests after threshold (10/min for images, 5/min for reactions)
   - Multiple IPs tracked separately
   - Rate limit resets after window expires (10 minutes)
   - Authenticated users (future): bypass rate limiting

2. Create [CaptchaSecurityTests.cs](UnitTests/):
   - CAPTCHA triggers when rate limit exceeded
   - Only valid answer unlocks further requests
   - Failed attempts tracked (max 3 failures, IP temporarily blacklisted)
   - Easy/medium/hard difficulty levels work correctly

3. Create [CommentSpamTests.cs](UnitTests/):
   - Duplicate comment detection: same email + similar message blocked
   - Email verification token generated and expires after 24 hours
   - Only verified comments appear in GetApprovedComments()
   - Admin can approve/reject comments

4. Create [ImageSecurityTests.cs](UnitTests/):
   - ValidateImageUrl() rejects invalid URLs
   - GenerateRobotsMetaTags() includes noindex for Photos.razor
   - robots.txt file exists and contains Disallow: /photos

5. Create [SecurityHeadersTests.cs](UnitTests/):
   - Response includes X-Frame-Options (prevent clickjacking)
   - Response includes X-Content-Type-Options: nosniff
   - CSP headers restrict image sources if possible

**Build Validation**:
- Run: `dotnet test UnitTests/*SecurityTests.cs --logger:"console;verbosity=detailed"`
- All security tests must pass
- Verify robots.txt file exists: `if (File.Exists("wwwroot/robots.txt")) { ✓ }`
- Verify meta tags in HTML (manual check): grep for `<meta name="robots"`

### Phase 5: Authentication Integration & Privilege Levels + Auth Tests
**Goal**: Architecture supports privilege escalation once login is available. Validate auth behavior.

**Build Phase:**
1. Add helper methods to [RateLimitService.cs](Infrastructure/Services/):
   - IsAuthenticated() parameter in CheckRateLimit()
   - AuthenticatedUsers bypass rate limiting method
   - Comments from authenticated users auto-approved (stub, requires identity integration)

2. Add extension to [RevalidatingIdentityAuthenticationStateProvider](Startup.cs):
   - Track authenticated user ID in rate limit checks
   - Store user reactions/comments linked to userId, not just sessionId

3. Add [PhotoGalleryPermission.cs](Models/) authorization policy:
   - Define roles: Admin (full access), AuthenticatedUser (no rate limits), Guest (limited)
   - Use [Authorize] attributes on detail/comment endpoints for future

**Test Phase (Run After Build):**
1. Create [AuthorizedRateLimitTests.cs](UnitTests/):
   - CheckRateLimit() with isAuthenticated=false enforces limits
   - CheckRateLimit() with isAuthenticated=true bypasses limits
   - Authenticated users can submit unlimited reactions
   - Admin users can bypass rate limits

2. Create [AuthorizedCommentTests.cs](UnitTests/):
   - Guest comments require email verification, admin approval
   - Authenticated user comments auto-approved (stub)
   - Admin can approve/reject any comment
   - Admin can delete comments

3. Create [PhotoGalleryPermissionTests.cs](UnitTests/):
   - Role-based access: Guest, AuthenticatedUser, Admin
   - Admin can view unapproved comments
   - Guest cannot see admin-only features
   - AuthenticatedUser can update their own reactions

**Build Validation**:
- Run: `dotnet test UnitTests/*AuthTests.cs --logger:"console;verbosity=detailed"`
- All auth tests must pass
- Verify [Authorize] attributes present on admin endpoints

### Phase 6: E2E Integration Tests & Coverage Validation
**Goal**: Run all tests together, verify browser behavior with Playwright, ensure 85%+ coverage.

**Before Running E2E Tests**:
1. Run all unit tests to ensure foundation is solid:
   ```
   dotnet test UnitTests/ --logger:"console;verbosity=detailed" --configuration Release
   ```
   - All tests from Phase 1-5 must pass
   - If any fail, fix in corresponding phase before proceeding

2. Generate code coverage report:
   ```
   dotnet test /p:CollectCoverageReport=true /p:CoverageReportOutputFormat=opencover
   ```
   - Verify ≥85% coverage on: PhotoGalleryService, RateLimitService, BotProtectionService, ImageSecurityService
   - Services layer must be ≥85% (UI/Razor components excluded from this calculation)
   - Identify any untested code paths and add tests

**E2E Tests using Playwright:**
1. Install Playwright test dependencies:
   ```
   npm install -D @playwright/test
   ```

2. Create [photo-gallery.e2e.spec.ts](wwwroot/) test suite:
   - Test carousel auto-rotation on page load with random photos
   - Test gallery tab filtering: All | Blog | Standalone tabs switch correctly
   - Test pagination: next/previous loads correct photos
   - Test click on photo opens PhotoDetail view
   - Test comment form: email validation required, success message after submit
   - Test reaction buttons: click increments count, duplicate prevented (greyed out)
   - Test rate limiting: rapid requests → CAPTCHA modal appears
   - Test CAPTCHA: wrong answer shows error, correct answer restores access
   - Test robots meta tag present: `<meta name="robots" content="noindex, nofollow" />`
   - Test mobile responsive: carousel/gallery render at 375px width
   - Test mobile responsive: gallery render at 768px width
   - Test mobile responsive: gallery render at 1024px width

3. Run Playwright tests:
   ```
   npx playwright test wwwroot/photo-gallery.e2e.spec.ts --headed
   ```
   - All tests must pass
   - Generate HTML report: `npx playwright show-report`

4. Optional: Visual regression testing with Playwright:
   - Capture baseline screenshots: `npx playwright test --update-snapshots`
   - Verify against baseline after changes: `npx playwright test`

**Build Validation**:
- All unit tests pass (Phase 1-5)
- Code coverage ≥85% on services layer
- All Playwright E2E tests pass
- No console errors in browser dev tools (Playwright captures these)

### Phase 7: Integration with Existing Site
**Goal**: Integrate photo gallery with existing navigation and prepare for future deployment.

1. Update navigation in [NavMenu.razor](Shared/):
   - Add "Photo Gallery" link in main nav or appropriate section
   - Update [Index.razor](Pages/) if needed to reference new gallery

2. Update layout if needed in [MainLayout.razor](Shared/):
   - Ensure PhotoDetail modal/pages fit existing grid

3. Update [Program.cs](Program.cs) (or [Startup.cs](Startup.cs)):
   - Register new services, repositories, DbContext migrations
   - Wire up middleware for rate limiting

4. Database migration:
   - `dotnet ef migrations add AddPhotoGalleryTables`
   - Review generated migration for correctness
   - Test local DB migration successfully applies

5. Final integration validation:
   - Load /photos page locally in browser
   - Test carousel rotation and photo display
   - Test all three filter tabs
   - Submit comment: verify email validation, success message
   - Add reaction: verify count updates, duplicate prevented
   - Verify robots meta tag present in page source
   - Test responsive design at 375px, 768px, 1024px viewports

## Relevant Files

**New Files to Create:**

*Models (Phase 1):*
- [Models/StandalonePhoto.cs](Models/StandalonePhoto.cs) — Standalone photo model
- [Models/PhotoComment.cs](Models/PhotoComment.cs) — Comment model for both photo types
- [Models/PhotoReaction.cs](Models/PhotoReaction.cs) — Emoji reaction tracking
- [Models/RateLimitLog.cs](Models/RateLimitLog.cs) — Rate limit tracking
- [Models/PhotoGalleryPermission.cs](Models/PhotoGalleryPermission.cs) — Authorization policy (Phase 5)

*Services (Phase 2):*
- [Infrastructure/Services/PhotoGalleryService.cs](Infrastructure/Services/PhotoGalleryService.cs) — Core gallery logic
- [Infrastructure/Services/RateLimitService.cs](Infrastructure/Services/RateLimitService.cs) — Rate limiting
- [Infrastructure/Services/BotProtectionService.cs](Infrastructure/Services/BotProtectionService.cs) — CAPTCHA handling
- [Infrastructure/Services/ImageSecurityService.cs](Infrastructure/Services/ImageSecurityService.cs) — AI/scraping protection

*Admin UI (Phase 2):*
- [Pages/Admin/PhotoManagement.razor](Pages/Admin/PhotoManagement.razor) — Admin photo upload/management
- [Pages/Admin/CommentModeration.razor](Pages/Admin/CommentModeration.razor) — Admin comment moderation
- [Components/AdminPhotoForm.razor](Components/AdminPhotoForm.razor) — Photo upload/edit form
- [Components/AdminPhotoForm.razor.css](Components/AdminPhotoForm.razor.css)
- [Components/CommentModerationList.razor](Components/CommentModerationList.razor) — Unapproved comments list
- [Components/CommentModerationList.razor.css](Components/CommentModerationList.razor.css)

*Pages & Components (Phase 3):*
- [Pages/Photos.razor](Pages/Photos.razor) — Main gallery page
- [Components/PhotoCarousel.razor](Components/PhotoCarousel.razor) — Auto-rotating carousel
- [Components/PhotoCarousel.razor.css](Components/PhotoCarousel.razor.css)
- [Components/PhotoGallery.razor](Components/PhotoGallery.razor) — Grid layout
- [Components/PhotoGallery.razor.css](Components/PhotoGallery.razor.css)
- [Components/PhotoDetail.razor](Components/PhotoDetail.razor) — Detail view modal
- [Components/PhotoDetail.razor.css](Components/PhotoDetail.razor.css)
- [Components/PhotoCommentSection.razor](Components/PhotoCommentSection.razor)
- [Components/PhotoCommentSection.razor.css](Components/PhotoCommentSection.razor.css)
- [Components/PhotoReactionButtons.razor](Components/PhotoReactionButtons.razor)
- [Components/PhotoReactionButtons.razor.css](Components/PhotoReactionButtons.razor.css)
- [Components/CaptchaComponent.razor](Components/CaptchaComponent.razor)
- [Components/CaptchaComponent.razor.css](Components/CaptchaComponent.razor.css)

*Unit Tests (Phases 1-5):*
- [UnitTests/PhotoModelTests.cs](UnitTests/PhotoModelTests.cs) — Model validation tests (Phase 1)
- [UnitTests/DbContextTests.cs](UnitTests/DbContextTests.cs) — EF Core relationship & migration tests (Phase 1)
- [UnitTests/PhotoGalleryServiceTests.cs](UnitTests/PhotoGalleryServiceTests.cs) — Service business logic (Phase 2)
- [UnitTests/RateLimitServiceTests.cs](UnitTests/RateLimitServiceTests.cs) — Rate limiting logic (Phase 2)
- [UnitTests/BotProtectionServiceTests.cs](UnitTests/BotProtectionServiceTests.cs) — CAPTCHA generation/verification (Phase 2)
- [UnitTests/PhotoGalleryIntegrationTests.cs](UnitTests/PhotoGalleryIntegrationTests.cs) — Service+Repo integration (Phase 2)
- [UnitTests/AdminPhotoManagementTests.cs](UnitTests/AdminPhotoManagementTests.cs) — Admin dashboard tests (Phase 2)
- [UnitTests/CommentModerationTests.cs](UnitTests/CommentModerationTests.cs) — Comment moderation tests (Phase 2)
- [UnitTests/PhotoCarouselComponentTests.cs](UnitTests/PhotoCarouselComponentTests.cs) — Carousel behavior (Phase 3)
- [UnitTests/PhotoGalleryComponentTests.cs](UnitTests/PhotoGalleryComponentTests.cs) — Gallery behavior (Phase 3)
- [UnitTests/PhotoDetailComponentTests.cs](UnitTests/PhotoDetailComponentTests.cs) — Detail view behavior (Phase 3)
- [UnitTests/PhotoCommentSectionComponentTests.cs](UnitTests/PhotoCommentSectionComponentTests.cs) — Comments behavior (Phase 3)
- [UnitTests/PhotoReactionButtonsComponentTests.cs](UnitTests/PhotoReactionButtonsComponentTests.cs) — Reactions behavior (Phase 3)
- [UnitTests/CaptchaComponentTests.cs](UnitTests/CaptchaComponentTests.cs) — CAPTCHA component behavior (Phase 3)
- [UnitTests/RateLimitSecurityTests.cs](UnitTests/RateLimitSecurityTests.cs) — Rate limit security (Phase 4)
- [UnitTests/CaptchaSecurityTests.cs](UnitTests/CaptchaSecurityTests.cs) — CAPTCHA security (Phase 4)
- [UnitTests/CommentSpamTests.cs](UnitTests/CommentSpamTests.cs) — Comment spam prevention (Phase 4)
- [UnitTests/ImageSecurityTests.cs](UnitTests/ImageSecurityTests.cs) — Image URL validation (Phase 4)
- [UnitTests/SecurityHeadersTests.cs](UnitTests/SecurityHeadersTests.cs) — Response security headers (Phase 4)
- [UnitTests/AuthorizedRateLimitTests.cs](UnitTests/AuthorizedRateLimitTests.cs) — Auth-based rate limit bypass (Phase 5)
- [UnitTests/AuthorizedCommentTests.cs](UnitTests/AuthorizedCommentTests.cs) — Auth-based comment approval (Phase 5)
- [UnitTests/PhotoGalleryPermissionTests.cs](UnitTests/PhotoGalleryPermissionTests.cs) — Role-based access (Phase 5)

*E2E Tests (Phase 6):*
- [wwwroot/photo-gallery.e2e.spec.ts](wwwroot/photo-gallery.e2e.spec.ts) — Playwright end-to-end tests

**Files to Modify:**
- [Data/ApplicationDbContext.cs](Data/ApplicationDbContext.cs) — Add DbSets for new models, configure relationships
- [Infrastructure/SqlRepo/JsnoRepo.cs](Infrastructure/SqlRepo/JsnoRepo.cs) — Add photo, comment, reaction repository methods
- [Startup.cs](Startup.cs) — Register new services, add rate limit middleware
- [Shared/NavMenu.razor](Shared/NavMenu.razor) — Add Photo Gallery link
- [Program.cs](Program.cs) (or Startup.cs) — If using .NET 6+ pattern
- [wwwroot/robots.txt](wwwroot/robots.txt) — Add /photos disallow rule
- [UnitTests.csproj](UnitTests/UnitTests.csproj) — Add required NuGet packages (Moq, etc.)

## Verification Strategy: Continuous Testing Throughout Development

Testing is integrated into **each phase** to catch issues early, rather than waiting until the end. Follow this pattern for every phase:

### Testing Cycle (repeat for each Phase 1-5):
1. **Write code** for the feature/component
2. **Write tests** for that code (same phase)
3. **Run tests locally**: `dotnet test UnitTests/[YourTestFile].cs --logger:"console;verbosity=detailed"`
4. **Fix any failing tests** before moving to next feature
5. **Build validation**: `dotnet build` must succeed with no warnings

### Running All Tests at Each Checkpoint:
After completing each phase, run all tests from that phase and previous phases:

**After Phase 1**:
```
dotnet test UnitTests/PhotoModelTests.cs UnitTests/DbContextTests.cs --logger:"console;verbosity=detailed"
```

**After Phase 2**:
```
dotnet test UnitTests/ --filter "Service|Integration|AdminPhotoManagement|CommentModeration" --logger:"console;verbosity=detailed"
```

**After Phase 3**:
```
dotnet test UnitTests/ --filter "Component" --logger:"console;verbosity=detailed"
```

**After Phase 4**:
```
dotnet test UnitTests/ --filter "Security" --logger:"console;verbosity=detailed"
```

**After Phase 5**:
```
dotnet test UnitTests/ --filter "Auth|Permission" --logger:"console;verbosity=detailed"
```

### Phase 6 Final Validation:
1. Run all unit tests together:
   ```
   dotnet test UnitTests/ --logger:"console;verbosity=detailed" --configuration Release
   ```

2. Generate code coverage report:
   ```
   dotnet test /p:CollectCoverageReport=true /p:CoverageReportOutputFormat=opencover
   ```
   - Verify ≥85% coverage on services layer (PhotoGalleryService, RateLimitService, BotProtectionService, ImageSecurityService)
   - UI/Razor components excluded from coverage calculation

3. Run Playwright E2E tests:
   ```
   npx playwright test wwwroot/photo-gallery.e2e.spec.ts --headed
   ```
   - All browser-based tests must pass
   - No console errors

### Quick Reference: Test Commands
| Phase | Command | Files |
|-------|---------|-------|
| 1 | `dotnet test UnitTests/PhotoModelTests.cs UnitTests/DbContextTests.cs` | Models, DB |
| 2 | `dotnet test UnitTests/ --filter "Service\|Integration\|AdminPhotoManagement\|CommentModeration"` | Services, Repos, Admin |
| 3 | `dotnet test UnitTests/ --filter "Component"` | UI Components |
| 4 | `dotnet test UnitTests/ --filter "Security"` | Security measures |
| 5 | `dotnet test UnitTests/ --filter "Auth\|Permission"` | Authorization |
| 6 | `dotnet test UnitTests/ && npx playwright test` | All + E2E |

### Recommended Development Workflow (Test-Driven)

#### Set Up Watch/Auto-Test Loop (Optional but Recommended):
To catch issues immediately as you code, use a watch/test loop:

**Terminal 1 - Auto-test on file changes:**
```
dotnet watch test UnitTests/ --logger:"console;verbosity=detailed"
```

**Terminal 2 - Auto-build on file changes:**
```
dotnet watch build
```

This way, every time you save a file, tests run automatically and you see failures immediately.

#### Phase-by-Phase Development Checklist:
For each phase:
1. ✅ Write feature code
2. ✅ Write + run corresponding test(s) in same phase
3. ✅ Tests pass locally before commit
4. ✅ Run `dotnet build` — no errors/warnings
5. ✅ Generate coverage: `dotnet test /p:CollectCoverageReport=true`
6. ✅ Review coverage report — identify gaps
7. ✅ Add tests for uncovered paths
8. ✅ Repeat until 85%+ on that feature

## Decisions

- **Data separation**: Standalone photos stored in separate table from blog photos; both queryable in unified gallery.
- **Comments**: Use single PhotoComment table with nullable ForeignKeys to support both blog and standalone photo comments. Email-verified + admin-approved workflow.
- **Reactions**: Simple emoji reactions stored in SessionStorage (client-side) and synced to DB (server-side). SessionId tracking prevents duplicate reactions per session/photo/emoji type.
- **Rate limiting scope**: Per IP address, 10 images/minute, 5 reactions/minute; authenticated users (future) bypass limits.
- **CAPTCHA**: Simple math CAPTCHA for MVP (no external dependency); stub for reCAPTCHA v3 integration later.
- **AI protection**: robots.txt + noindex meta tags (simplest, no performance impact). Future: watermarks, metadata stripping (Phase 2+).
- **Privilege escalation**: Design services now with IsAuthenticated parameters; full implementation when login is available.
- **Testing**: NUnit + Moq for unit/integration; Playwright for E2E. Target 85%+ coverage on business logic layer.
- **Test-Driven Development**: Tests are written and run **in parallel with** each feature, not after. This prevents build-up of issues.
- **Email service**: Reuse existing email service for comment verification emails.
- **Image optimization**: Implement lazy-loading with `loading="lazy"` attribute on images in gallery components.
- **Admin dashboard**: Add admin photo management (upload/edit/delete) and comment moderation (approve/reject/delete) in Phase 2.

## Further Considerations

1. **Session Management for Reactions**: 
   - Reactions stored in SessionStorage to track per-user reactions client-side.
   - Sync to DB server-side via ReactionRepository.
   - Recommendation: Use Blazored.SessionStorage (already referenced in project) for client-side tracking.

2. **Email Service Integration**: 
   - Existing EmailService used for contact form, will be reused for comment verification.
   - SMTP configuration already in place.
   - Recommendation: Create email template for comment verification link (24-hour expiration).

3. **Future Admin Dashboard Enhancements**:
   - Currently add basic CRUD in Phase 2.
   - Future: analytics dashboard (comment volume, popular photos, reaction trends).
   - Future: bulk photo upload, image optimization/resizing.
   - Future: scheduled photo rotation, featured photo selections.

4. **Database Considerations**:
   - RateLimitLog table will grow quickly; consider archival strategy (keep last 7 days, archive older records).
   - Recommendation: Add scheduled job to clean old rate limit logs weekly.
