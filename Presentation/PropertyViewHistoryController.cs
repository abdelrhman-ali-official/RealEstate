using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Shared.PropertyViewHistoryModels;
using Shared.ResponseModels;
using System.Security.Claims;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PropertyViewHistoryController : ApiController
    {
        private readonly IServiceManager _serviceManager;

        public PropertyViewHistoryController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("recent")]
        public async Task<ActionResult<ApiResponse<RecentViewedPropertiesDTO>>> GetRecentViewedProperties([FromQuery] int count = 10)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recentViews = await _serviceManager.PropertyViewHistoryService.GetRecentViewedPropertiesAsync(userId, count);
            var result = new RecentViewedPropertiesDTO { RecentViews = recentViews };
            
            return Ok(ApiResponse<RecentViewedPropertiesDTO>.SuccessResponse(result, "Recent viewed properties retrieved successfully."));
        }

        [HttpGet("property/{propertyId}/count")]
        public async Task<ActionResult<ApiResponse<int>>> GetPropertyViewCount(int propertyId)
        {
            var count = await _serviceManager.PropertyViewHistoryService.GetPropertyViewCountAsync(propertyId);
            return Ok(ApiResponse<int>.SuccessResponse(count, "Property view count retrieved successfully."));
        }

        [HttpGet("most-viewed")]
        public async Task<ActionResult<ApiResponse<List<MostViewedPropertyDTO>>>> GetMostViewedProperties([FromQuery] int top = 10)
        {
            var mostViewed = await _serviceManager.PropertyViewHistoryService.GetMostViewedPropertiesAsync(top);
            return Ok(ApiResponse<List<MostViewedPropertyDTO>>.SuccessResponse(mostViewed, "Most viewed properties retrieved successfully."));
        }

        // New endpoints for Pro/Premium users
        [HttpGet("property/{propertyId}/viewers")]
        public async Task<ActionResult<ApiResponse<PropertyViewersAnalyticsDTO>>> GetPropertyViewers(
            int propertyId, 
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? limit = 50)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var request = new PropertyViewAnalyticsRequestDTO
            {
                PropertyId = propertyId,
                FromDate = fromDate,
                ToDate = toDate,
                Limit = limit
            };

            var analytics = await _serviceManager.PropertyViewHistoryService.GetPropertyViewersAsync(propertyId, userId, request);
            
            if (analytics == null)
            {
                return Forbid("You don't have permission to view analytics for this property. This feature requires a Pro or Premium subscription and property ownership.");
            }

            return Ok(ApiResponse<PropertyViewersAnalyticsDTO>.SuccessResponse(analytics, "Property viewers analytics retrieved successfully."));
        }

        [HttpGet("my-properties/analytics")]
        public async Task<ActionResult<ApiResponse<List<PropertyViewersAnalyticsDTO>>>> GetMyPropertiesViewAnalytics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var analytics = await _serviceManager.PropertyViewHistoryService.GetMyPropertiesViewAnalyticsAsync(userId, fromDate, toDate);
            
            return Ok(ApiResponse<List<PropertyViewersAnalyticsDTO>>.SuccessResponse(analytics, "Properties view analytics retrieved successfully."));
        }

        [HttpGet("can-view-analytics/{propertyId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CanViewPropertyAnalytics(int propertyId)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var canView = await _serviceManager.PropertyViewHistoryService.CanUserViewPropertyAnalyticsAsync(userId, propertyId);
            return Ok(ApiResponse<bool>.SuccessResponse(canView, "Permission check completed."));
        }
    }
}