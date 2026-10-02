using jsnover.net.blazor.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Controllers
{
    [ApiController]
    [Route("api/jobs/family-photo-digest")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public sealed class FamilyPhotoDigestController : ControllerBase
    {
        private readonly FamilyPhotoDigestService _digestService;
        private readonly ILogger<FamilyPhotoDigestController> _logger;

        public FamilyPhotoDigestController(
            FamilyPhotoDigestService digestService,
            ILogger<FamilyPhotoDigestController> logger)
        {
            _digestService = digestService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Run(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _digestService.RunAsync(cancellationToken);
                return result == FamilyPhotoDigestRunResult.AlreadyRunning
                    ? Conflict()
                    : Ok(new { status = result.ToString() });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Family photo digest job failed.");
                return StatusCode(500, "Family photo digest job failed.");
            }
        }
    }
}