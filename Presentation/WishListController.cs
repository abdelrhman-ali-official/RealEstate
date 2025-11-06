using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Shared.WishListModels;
using Shared.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Presentation
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishListController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly ILogger<WishListController> _logger;

        public WishListController(IServiceManager serviceManager, ILogger<WishListController> logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }

       
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ApiResponse<bool>>> AddToWishList([FromBody] AddToWishListDTO request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<bool>.FailureResponse("User ID not found in token."));

                var result = await _serviceManager.WishListService.AddToWishListAsync(request.PropertyId, userId);
                
                if (result)
                {
                    return Ok(ApiResponse<bool>.SuccessResponse(true, "Property added to wishlist successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<bool>.FailureResponse("Property is already in your wishlist"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding property {PropertyId} to wishlist: {Message}", request.PropertyId, ex.Message);
                return StatusCode(500, ApiResponse<bool>.FailureResponse("An error occurred while adding property to wishlist"));
            }
        }

        
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<WishListItemDTO>>> GetMyWishList()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var wishList = await _serviceManager.WishListService.GetUserWishListAsync(userId);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Wishlist retrieved successfully",
                    Data = wishList,
                    Count = wishList.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wishlist: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving wishlist"
                });
            }
        }

        [HttpDelete("{propertyId}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> RemoveFromWishList(int propertyId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var result = await _serviceManager.WishListService.RemoveFromWishListAsync(propertyId, userId);
                
                if (result)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Property removed from wishlist successfully",
                        PropertyId = propertyId
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Property not found in your wishlist"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing property {PropertyId} from wishlist: {Message}", propertyId, ex.Message);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while removing property from wishlist"
                });
            }
        }

     
        [HttpGet("count")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<WishListCountDTO>> GetWishListCount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var count = await _serviceManager.WishListService.GetWishListCountAsync(userId);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Wishlist count retrieved successfully",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wishlist count: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving wishlist count"
                });
            }
        }

        [HttpGet("check/{propertyId}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> CheckPropertyInWishList(int propertyId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var isInWishList = await _serviceManager.WishListService.IsPropertyInWishListAsync(propertyId, userId);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Property wishlist status checked successfully",
                    Data = new
                    {
                        PropertyId = propertyId,
                        IsInWishList = isInWishList
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking property {PropertyId} in wishlist: {Message}", propertyId, ex.Message);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while checking property in wishlist"
                });
            }
        }

       
        [HttpGet("admin/most-wished")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<MostWishedPropertyDTO>>> GetMostWishedProperties(
            [FromQuery] WishListFilterDTO filter)
        {
            try
            {
                var mostWishedProperties = await _serviceManager.WishListService.GetMostWishedPropertiesAsync(filter);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Most wished properties retrieved successfully",
                    Data = mostWishedProperties,
                    Count = mostWishedProperties.Count(),
                    Filters = filter
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most wished properties: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving most wished properties"
                });
            }
        }

        [HttpGet("admin/property/{propertyId}/users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PropertyWishListUsersDTO>> GetPropertyWishListUsers(int propertyId)
        {
            try
            {
                var propertyWishListUsers = await _serviceManager.WishListService.GetPropertyWishListUsersAsync(propertyId);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Property wishlist users retrieved successfully",
                    Data = propertyWishListUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property {PropertyId} wishlist users: {Message}", propertyId, ex.Message);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving property wishlist users"
                });
            }
        }

        [HttpGet("admin/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetWishListStatistics()
        {
            try
            {
                // Get overall statistics
                var allWishListItems = await _serviceManager.WishListService.GetMostWishedPropertiesAsync(new WishListFilterDTO());
                
                var statistics = new
                {
                    TotalWishListItems = allWishListItems.Sum(p => p.WishListCount),
                    TotalUniqueProperties = allWishListItems.Count(),
                    MostWishedProperty = allWishListItems.FirstOrDefault(),
                    TopWishedProperties = allWishListItems.Take(10).ToList(),
                    GeneratedAt = DateTime.UtcNow
                };
                
                return Ok(new
                {
                    Success = true,
                    Message = "Wishlist statistics retrieved successfully",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wishlist statistics: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving wishlist statistics"
                });
            }
        }

        /// <summary>
        /// Get wishlist analytics for authenticated broker (Pro/Premium only)
        /// Shows all properties owned by the broker and users who wishlisted them
        /// </summary>
        [HttpGet("broker/analytics")]
        [Authorize(Roles = "Broker")]
        public async Task<ActionResult<ApiResponse<OwnerWishlistSummaryDTO>>> GetBrokerWishlistAnalytics()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<OwnerWishlistSummaryDTO>.FailureResponse("User ID not found in token."));

                var analytics = await _serviceManager.WishListService.GetBrokerWishlistAnalyticsAsync(userId);
                
                return Ok(ApiResponse<OwnerWishlistSummaryDTO>.SuccessResponse(analytics, "Broker wishlist analytics retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Broker wishlist analytics access denied: {Message}", ex.Message);
                return StatusCode(403, ApiResponse<OwnerWishlistSummaryDTO>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving broker wishlist analytics: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<OwnerWishlistSummaryDTO>.FailureResponse("An error occurred while retrieving wishlist analytics"));
            }
        }

        /// <summary>
        /// Get wishlist analytics for authenticated developer (Pro/Premium only)
        /// Shows all properties owned by the developer and users who wishlisted them
        /// </summary>
        [HttpGet("developer/analytics")]
        [Authorize(Roles = "Developer")]
        public async Task<ActionResult<ApiResponse<OwnerWishlistSummaryDTO>>> GetDeveloperWishlistAnalytics()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<OwnerWishlistSummaryDTO>.FailureResponse("User ID not found in token."));

                var analytics = await _serviceManager.WishListService.GetDeveloperWishlistAnalyticsAsync(userId);
                
                return Ok(ApiResponse<OwnerWishlistSummaryDTO>.SuccessResponse(analytics, "Developer wishlist analytics retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Developer wishlist analytics access denied: {Message}", ex.Message);
                return StatusCode(403, ApiResponse<OwnerWishlistSummaryDTO>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving developer wishlist analytics: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<OwnerWishlistSummaryDTO>.FailureResponse("An error occurred while retrieving wishlist analytics"));
            }
        }
    }
} 