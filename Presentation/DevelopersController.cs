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

namespace Presentation
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Developer,Admin")]
    public class DevelopersController : ApiController
    {
        private readonly IServiceManager _serviceManager;
        private readonly ILogger<DevelopersController> _logger;

        public DevelopersController(IServiceManager serviceManager, ILogger<DevelopersController> logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<DeveloperResultDTO>> GetMyProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var developer = await _serviceManager.DeveloperService.GetDeveloperByUserIdAsync(userId);
                if (developer == null)
                    return NotFound("Developer profile not found.");

                return Ok(developer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving developer profile: {Message}", ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving the developer profile." });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<DeveloperResultDTO>> GetDeveloperById(int id)
        {
            try
            {
                var developer = await _serviceManager.DeveloperService.GetDeveloperByIdAsync(id);
                if (developer == null)
                    return NotFound($"Developer with ID {id} not found.");

                return Ok(developer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving developer by ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving the developer." });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DeveloperResultDTO>>> CreateDeveloper([FromBody] DeveloperCreateDTO developerDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<DeveloperResultDTO>.FailureResponse("User ID not found in token."));

                var developer = await _serviceManager.DeveloperService.CreateDeveloperAsync(developerDto, userId);
                return CreatedAtAction(nameof(GetDeveloperById), new { id = developer.Id }, 
                    ApiResponse<DeveloperResultDTO>.SuccessResponse(developer, "Developer profile created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating developer: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<DeveloperResultDTO>.FailureResponse("An error occurred while creating the developer profile"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DeveloperResultDTO>> UpdateDeveloper(int id, [FromBody] DeveloperUpdateDTO developerDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var developer = await _serviceManager.DeveloperService.UpdateDeveloperAsync(id, developerDto, userId);
                return Ok(developer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating developer {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Message = "An error occurred while updating the developer profile." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDeveloper(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var result = await _serviceManager.DeveloperService.DeleteDeveloperAsync(id, userId);
                if (!result)
                    return NotFound($"Developer with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting developer {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Message = "An error occurred while deleting the developer profile." });
            }
        }
    }
} 