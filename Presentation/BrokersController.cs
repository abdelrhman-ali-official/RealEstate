using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Shared.BrokerModels;
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
    [Authorize(Roles = "Broker,Admin")]
    public class BrokersController : ApiController
    {
        private readonly IServiceManager _serviceManager;
        private readonly ILogger<BrokersController> _logger;

        public BrokersController(IServiceManager serviceManager, ILogger<BrokersController> logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResult<BrokerResultDTO>>> GetAllBrokers([FromQuery] BrokerSpecificationsParameters parameters)
        {
            try
            {
                var brokers = await _serviceManager.BrokerService.GetAllBrokersAsync(parameters);
                return Ok(brokers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving brokers: {Message}", ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving brokers." });
            }
        }

        [HttpGet("profile")]
        public async Task<ActionResult<BrokerResultDTO>> GetMyProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var broker = await _serviceManager.BrokerService.GetBrokerByUserIdAsync(userId);
                if (broker == null)
                    return NotFound("Broker profile not found.");

                return Ok(broker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving broker profile: {Message}", ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving the broker profile." });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BrokerResultDTO>> GetBrokerById(int id)
        {
            try
            {
                var broker = await _serviceManager.BrokerService.GetBrokerByIdAsync(id);
                if (broker == null)
                    return NotFound($"Broker with ID {id} not found.");

                return Ok(broker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving broker by ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving the broker." });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BrokerResultDTO>>> CreateBroker([FromBody] BrokerCreateDTO brokerDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BrokerResultDTO>.FailureResponse("User ID not found in token."));

                var broker = await _serviceManager.BrokerService.CreateBrokerAsync(brokerDto, userId);
                return CreatedAtAction(nameof(GetBrokerById), new { id = broker.Id }, 
                    ApiResponse<BrokerResultDTO>.SuccessResponse(broker, "Broker profile created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating broker: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<BrokerResultDTO>.FailureResponse("An error occurred while creating the broker profile"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BrokerResultDTO>> UpdateBroker(int id, [FromBody] BrokerUpdateDTO brokerDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var broker = await _serviceManager.BrokerService.UpdateBrokerAsync(id, brokerDto, userId);
                return Ok(broker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating broker {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Message = "An error occurred while updating the broker profile." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBroker(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token.");

                var result = await _serviceManager.BrokerService.DeleteBrokerAsync(id, userId);
                if (!result)
                    return NotFound($"Broker with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting broker {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { Message = "An error occurred while deleting the broker profile." });
            }
        }

        [HttpGet("governments")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetGovernments()
        {
            try
            {
                var governments = await _serviceManager.BrokerService.GetGovernmentsAsync();
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
                var cities = await _serviceManager.BrokerService.GetCitiesByGovernmentAsync(government);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cities for government {Government}", government);
                return StatusCode(500, new { Message = "An error occurred while retrieving cities." });
            }
        }

        [HttpGet("agency-names")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetAgencyNames()
        {
            try
            {
                var agencyNames = await _serviceManager.BrokerService.GetAgencyNamesAsync();
                return Ok(agencyNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agency names");
                return StatusCode(500, new { Message = "An error occurred while retrieving agency names." });
            }
        }
    }
} 