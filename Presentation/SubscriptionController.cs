using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Shared.ErrorModels;
using Shared.SubscriptionModels;
using System.Net;
using System.Security.Claims;

namespace Presentation
{
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ApiController
    {
        private readonly IServiceManager _serviceManager;

        public SubscriptionController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        /// <summary>
        /// Get all available packages
        /// </summary>
        /// <returns>List of all available packages</returns>
        [HttpGet("packages")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PackageDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllPackages()
        {
            var packages = await _serviceManager.SubscriptionService.GetAllPackagesAsync();
            return Ok(packages);
        }

        /// <summary>
        /// Get package details by ID
        /// </summary>
        /// <param name="packageId">Package ID</param>
        /// <returns>Package details</returns>
        [HttpGet("packages/{packageId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PackageDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPackageById(int packageId)
        {
            var package = await _serviceManager.SubscriptionService.GetPackageByIdAsync(packageId);
            
            if (package == null)
                return NotFound(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    ErrorMessage = "Package not found."
                });

            return Ok(package);
        }

        /// <summary>
        /// Subscribe to a package
        /// </summary>
        /// <param name="request">Subscription request with package ID</param>
        /// <returns>Subscription response</returns>
        [HttpPost("subscribe")]
        [ProducesResponseType(typeof(SubscriptionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorMessage = "User ID not found in token."
                });

            var result = await _serviceManager.SubscriptionService.SubscribeAsync(userId, request);
            
            if (!result.Success)
                return BadRequest(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorMessage = result.Message
                });

            return Ok(result);
        }

        /// <summary>
        /// Get current user's subscription details
        /// </summary>
        /// <returns>User subscription information</returns>
        [HttpGet("my-subscription")]
        [ProducesResponseType(typeof(UserSubscriptionInfoDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorDetails), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetMySubscription()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorMessage = "User ID not found in token."
                });

            var subscription = await _serviceManager.SubscriptionService.GetCurrentUserSubscriptionAsync(userId);
            
            if (subscription == null)
                return NotFound(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    ErrorMessage = "No active subscription found."
                });

            return Ok(subscription);
        }

        /// <summary>
        /// Check if user can create a property (within limits)
        /// </summary>
        /// <returns>Boolean indicating if user can create property</returns>
        [HttpGet("can-create-property")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CanCreateProperty()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorMessage = "User ID not found in token."
                });

            var canCreate = await _serviceManager.SubscriptionService.CanCreatePropertyAsync(userId);
            return Ok(canCreate);
        }

        /// <summary>
        /// Check if user has access to a specific feature
        /// </summary>
        /// <param name="featureName">Feature name to check</param>
        /// <returns>Boolean indicating if user has access to the feature</returns>
        [HttpGet("feature-access/{featureName}")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> HasFeatureAccess(string featureName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorMessage = "User ID not found in token."
                });

            var hasAccess = await _serviceManager.SubscriptionService.HasFeatureAccessAsync(userId, featureName);
            return Ok(hasAccess);
        }
    }
}