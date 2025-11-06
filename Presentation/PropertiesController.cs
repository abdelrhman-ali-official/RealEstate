using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Shared.DeveloperModels;
using Shared.ResponseModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared;

namespace Presentation
{
    [Route("api/[controller]")]
    public class PropertiesController : ApiController
    {
        private readonly IServiceManager _serviceManager;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(IServiceManager serviceManager, ILogger<PropertiesController> logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResult<PropertyResultDTO>>> GetAllProperties([FromQuery] PropertySpecificationsParameters parameters)
        {
            try
            {
                var properties = await _serviceManager.PropertyService.GetAllPropertiesAsync(parameters);
                return Ok(properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving properties: {Message}", ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving properties." });
            }
        }

        [HttpGet("for-sale")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResult<PropertyResultDTO>>> GetPropertiesForSale([FromQuery] PropertySpecificationsParameters parameters)
        {
            try
            {
                var properties = await _serviceManager.PropertyService.GetPropertiesForSaleAsync(parameters);
                return Ok(properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving properties for sale: {Message}", ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving properties for sale." });
            }
        }

        [HttpGet("for-rent")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResult<PropertyResultDTO>>> GetPropertiesForRent([FromQuery] PropertySpecificationsParameters parameters)
        {
            try
            {
                var properties = await _serviceManager.PropertyService.GetPropertiesForRentAsync(parameters);
                return Ok(properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving properties for rent: {Message}", ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving properties for rent." });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PropertyResultDTO>> GetPropertyById(int id)
        {
            try
            {
                var property = await _serviceManager.PropertyService.GetPropertyByIdAsync(id);
                if (property == null)
                    return NotFound($"Property with ID {id} not found.");

                // Automatically track property view for authenticated users
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation("Recording property view for Property {PropertyId} by User {UserId}", id, userId);
                    try
                    {
                        await _serviceManager.PropertyViewHistoryService.AddPropertyViewAsync(id, userId);
                        _logger.LogInformation("Successfully recorded property view for Property {PropertyId} by User {UserId}", id, userId);
                    }
                    catch (Exception viewEx)
                    {
                        _logger.LogError(viewEx, "Failed to record property view for Property {PropertyId} by User {UserId}", id, userId);
                        // Don't fail the main request if view tracking fails
                    }
                }
                else
                {
                    _logger.LogInformation("No authenticated user found for property view tracking");
                }

                return Ok(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property by ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving the property." });
            }
        }

        [HttpGet("my-properties")]
        [Authorize(Roles = "Developer,Broker,Admin")]
        public async Task<ActionResult<IEnumerable<PropertyResultDTO>>> GetMyProperties()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                // Check if user is a developer
                var developer = await _serviceManager.DeveloperService.GetDeveloperByUserIdAsync(userId);
                if (developer != null)
                {
                    var properties = await _serviceManager.PropertyService.GetPropertiesByDeveloperAsync(developer.Id, userId);
                    return Ok(properties);
                }

                // Check if user is a broker
                var broker = await _serviceManager.BrokerService.GetBrokerByUserIdAsync(userId);
                if (broker != null)
                {
                    var properties = await _serviceManager.PropertyService.GetPropertiesByBrokerAsync(broker.Id, userId);
                    return Ok(properties);
                }

                return NotFound("Developer or broker profile not found. Please create a profile first.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user properties: {Message}", ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving your properties." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Developer,Broker,Admin")]
        public async Task<ActionResult<ApiResponse<PropertyResultDTO>>> CreateProperty([FromBody] PropertyCreateDTO propertyDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<PropertyResultDTO>.FailureResponse("User ID not found in token."));

                var property = await _serviceManager.PropertyService.CreatePropertyAsync(propertyDto, userId, _serviceManager.SubscriptionService);
                return CreatedAtAction(nameof(GetPropertyById), new { id = property.Id }, 
                    ApiResponse<PropertyResultDTO>.SuccessResponse(property, "Property created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating property: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<PropertyResultDTO>.FailureResponse("An error occurred while creating the property"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Developer,Broker,Admin")]
        public async Task<ActionResult<PropertyResultDTO>> UpdateProperty(int id, [FromBody] PropertyUpdateDTO propertyDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var property = await _serviceManager.PropertyService.UpdatePropertyAsync(id, propertyDto, userId);
                return Ok(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating property {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Message = "An error occurred while updating the property." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Developer,Broker,Admin")]
        public async Task<ActionResult> DeleteProperty(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var result = await _serviceManager.PropertyService.DeletePropertyAsync(id, userId, _serviceManager.SubscriptionService);
                if (!result)
                    return NotFound($"Property with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting property {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Message = "An error occurred while deleting the property." });
            }
        }

        [HttpGet("governments")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetGovernments()
        {
            try
            {
                var governments = await _serviceManager.PropertyService.GetGovernmentsAsync();
                return Ok(governments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving governments");
                return StatusCode(500, new { Message = "An error occurred while retrieving governments." });
            }
        }

        [HttpGet("cities/{government}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetCitiesByGovernment(string government)
        {
            try
            {
                var cities = await _serviceManager.PropertyService.GetCitiesByGovernmentAsync(government);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cities for government {Government}", government);
                return StatusCode(500, new { Message = "An error occurred while retrieving cities." });
            }
        }

        [HttpGet("recent-views")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetRecentViewedProperties([FromQuery] int count = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");
            var recent = await _serviceManager.PropertyViewHistoryService.GetRecentViewedPropertiesAsync(userId, count);
            return Ok(new { Success = true, Data = recent });
        }

        [HttpGet("{propertyId}/view-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPropertyViewCount(int propertyId)
        {
            var count = await _serviceManager.PropertyViewHistoryService.GetPropertyViewCountAsync(propertyId);
            return Ok(new { PropertyId = propertyId, ViewCount = count });
        }

        [HttpGet("most-viewed")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMostViewedProperties([FromQuery] int top = 1)
        {
            try
            {
                var mostViewed = await _serviceManager.PropertyViewHistoryService.GetMostViewedPropertiesAsync(top);
                return Ok(new { Success = true, Data = mostViewed, Count = mostViewed.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most viewed properties: {Message}", ex.Message);
                return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving most viewed properties." });
            }
        }
    }
} 