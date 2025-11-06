using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Shared.AppointmentModels;
using Shared.ResponseModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Presentation
{
    [Route("api/[controller]")]
    public class AppointmentsController : ApiController
    {
        private readonly IServiceManager _serviceManager;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IServiceManager serviceManager, ILogger<AppointmentsController> logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ApiResponse<AppointmentResultDTO>>> BookAppointment([FromBody] AppointmentCreateDTO dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<AppointmentResultDTO>.FailureResponse("User ID not found in token."));

                var result = await _serviceManager.AppointmentService.BookAppointmentAsync(dto, userId);
                return Ok(ApiResponse<AppointmentResultDTO>.SuccessResponse(result, "Appointment booked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<AppointmentResultDTO>.FailureResponse("An error occurred while booking the appointment"));
            }
        }

        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResultDTO>>>> GetMyAppointments([FromQuery] AppointmentSpecificationsParameters parameters)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("User ID not found in token."));

                parameters.CustomerId = int.TryParse(userId, out var id) ? id : null;
                var result = await _serviceManager.AppointmentService.GetAppointmentsAsync(parameters);
                return Ok(ApiResponse<IEnumerable<AppointmentResultDTO>>.SuccessResponse(result, "Appointments retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("An error occurred while retrieving appointments"));
            }
        }

        [HttpGet("owner")]
        [Authorize(Roles = "Developer,Broker")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResultDTO>>>> GetOwnerAppointments([FromQuery] AppointmentSpecificationsParameters parameters)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return Unauthorized(ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("User ID or role not found in token."));

                if (userRole == "Developer")
                    parameters.DeveloperId = int.TryParse(userId, out var id) ? id : null;
                else if (userRole == "Broker")
                    parameters.BrokerId = int.TryParse(userId, out var id) ? id : null;

                var result = await _serviceManager.AppointmentService.GetAppointmentsAsync(parameters);
                return Ok(ApiResponse<IEnumerable<AppointmentResultDTO>>.SuccessResponse(result, "Owner appointments retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving owner appointments: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("An error occurred while retrieving owner appointments"));
            }
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResultDTO>>>> GetAllAppointments([FromQuery] AppointmentSpecificationsParameters parameters)
        {
            try
            {
                var result = await _serviceManager.AppointmentService.GetAppointmentsAsync(parameters);
                return Ok(ApiResponse<IEnumerable<AppointmentResultDTO>>.SuccessResponse(result, "All appointments retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all appointments: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("An error occurred while retrieving all appointments"));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<AppointmentResultDTO>>> GetAppointmentById(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return Unauthorized(ApiResponse<AppointmentResultDTO>.FailureResponse("User ID or role not found in token."));

                var result = await _serviceManager.AppointmentService.GetAppointmentByIdAsync(id, userId, userRole);
                if (result == null)
                    return NotFound(ApiResponse<AppointmentResultDTO>.FailureResponse($"Appointment with ID {id} not found"));

                return Ok(ApiResponse<AppointmentResultDTO>.SuccessResponse(result, "Appointment retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment {Id}: {Message}", id, ex.Message);
                return StatusCode(500, ApiResponse<AppointmentResultDTO>.FailureResponse("An error occurred while retrieving the appointment"));
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> UpdateAppointment(int id, [FromBody] AppointmentUpdateDTO dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return Unauthorized(ApiResponse.FailureResponse("User ID or role not found in token."));

                var result = await _serviceManager.AppointmentService.UpdateAppointmentAsync(id, dto, userId, userRole);
                if (!result)
                    return NotFound(ApiResponse.FailureResponse($"Appointment with ID {id} not found"));

                return Ok(ApiResponse.SuccessResponse("Appointment updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment {Id}: {Message}", id, ex.Message);
                return StatusCode(500, ApiResponse.FailureResponse("An error occurred while updating the appointment"));
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ApiResponse>> CancelAppointment(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse.FailureResponse("User ID not found in token."));

                var result = await _serviceManager.AppointmentService.CancelAppointmentAsync(id, userId);
                if (!result)
                    return NotFound(ApiResponse.FailureResponse($"Appointment with ID {id} not found"));

                return Ok(ApiResponse.SuccessResponse("Appointment cancelled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {Id}: {Message}", id, ex.Message);
                return StatusCode(500, ApiResponse.FailureResponse("An error occurred while cancelling the appointment"));
            }
        }

        [HttpPost("{id}/status")]
        [Authorize(Roles = "Developer,Broker,Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateAppointmentStatus(int id, [FromBody] AppointmentStatus status)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    return Unauthorized(ApiResponse.FailureResponse("User ID or role not found in token."));

                var result = await _serviceManager.AppointmentService.UpdateAppointmentStatusAsync(id, status, userId, userRole);
                if (!result)
                    return NotFound(ApiResponse.FailureResponse($"Appointment with ID {id} not found"));

                return Ok(ApiResponse.SuccessResponse("Appointment status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status {Id}: {Message}", id, ex.Message);
                return StatusCode(500, ApiResponse.FailureResponse("An error occurred while updating the appointment status"));
            }
        }

        /// <summary>
        /// Get all appointments for the authenticated broker with optional filtering
        /// </summary>
        [HttpGet("broker/my-appointments")]
        [Authorize(Roles = "Broker")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResultDTO>>>> GetBrokerAppointments([FromQuery] AppointmentSpecificationsParameters parameters)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("User ID not found in token."));

                var result = await _serviceManager.AppointmentService.GetBrokerAppointmentsAsync(userId, parameters);
                return Ok(ApiResponse<IEnumerable<AppointmentResultDTO>>.SuccessResponse(result, "Broker appointments retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("Access denied. Broker profile not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving broker appointments: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("An error occurred while retrieving broker appointments"));
            }
        }

        /// <summary>
        /// Get all appointments for the authenticated developer with optional filtering
        /// </summary>
        [HttpGet("developer/my-appointments")]
        [Authorize(Roles = "Developer")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResultDTO>>>> GetDeveloperAppointments([FromQuery] AppointmentSpecificationsParameters parameters)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("User ID not found in token."));

                var result = await _serviceManager.AppointmentService.GetDeveloperAppointmentsAsync(userId, parameters);
                return Ok(ApiResponse<IEnumerable<AppointmentResultDTO>>.SuccessResponse(result, "Developer appointments retrieved successfully"));
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("Access denied. Developer profile not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving developer appointments: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<IEnumerable<AppointmentResultDTO>>.FailureResponse("An error occurred while retrieving developer appointments"));
            }
        }
    }
} 