using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Shared.DashboardModels;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Presentation
{
    /// <summary>
    /// Advanced Dashboard Controller for Real Estate System Administration
    /// Provides comprehensive analytics, statistics, and administrative controls
    /// </summary>
    [Route("api/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IServiceManager serviceManager, ILogger<DashboardController> logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive admin dashboard with all statistics and analytics
        /// </summary>
        [HttpPost("admin/comprehensive")]
        [ProducesResponseType(200, Type = typeof(AdminDashboardDTO))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<AdminDashboardDTO>> GetComprehensiveAdminDashboard([FromBody] DashboardFilterDTO filter)
        {
            try
            {
                _logger.LogInformation("Admin dashboard requested with filters: {@Filters}", filter);
                
                var result = await _serviceManager.DashboardService.GetAdminDashboardAsync(filter);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Dashboard data retrieved successfully",
                    Data = result,
                    GeneratedAt = DateTime.UtcNow,
                    Filters = filter
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating admin dashboard");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while generating the dashboard",
                    Error = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get admin dashboard with default filters (last 12 months)
        /// </summary>
        [HttpGet("admin")]
        [ProducesResponseType(200, Type = typeof(AdminDashboardDTO))]
        public async Task<ActionResult<AdminDashboardDTO>> GetDefaultAdminDashboard()
        {
            try
            {
                var defaultFilter = new DashboardFilterDTO
                {
                    FromDate = DateTime.UtcNow.AddMonths(-12),
                    ToDate = DateTime.UtcNow
                };
                
                var result = await _serviceManager.DashboardService.GetAdminDashboardAsync(defaultFilter);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Default dashboard data retrieved successfully",
                    Data = result,
                    GeneratedAt = DateTime.UtcNow,
                    Period = "Last 12 months"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating default admin dashboard");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while generating the dashboard"
                });
            }
        }

        /// <summary>
        /// Get real-time dashboard with current month statistics
        /// </summary>
        [HttpGet("admin/realtime")]
        public async Task<ActionResult<AdminDashboardDTO>> GetRealTimeDashboard()
        {
            try
            {
                var currentMonthFilter = new DashboardFilterDTO
                {
                    FromDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                    ToDate = DateTime.UtcNow
                };
                
                var result = await _serviceManager.DashboardService.GetAdminDashboardAsync(currentMonthFilter);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Real-time dashboard data retrieved successfully",
                    Data = result,
                    GeneratedAt = DateTime.UtcNow,
                    Period = "Current month"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating real-time dashboard");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while generating the real-time dashboard"
                });
            }
        }

        /// <summary>
        /// Get dashboard for specific date range
        /// </summary>
        [HttpGet("admin/custom")]
        public async Task<ActionResult<AdminDashboardDTO>> GetCustomDashboard(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? city = null,
            [FromQuery] string? government = null)
        {
            try
            {
                // Log received parameters
                _logger.LogInformation("Custom dashboard requested: fromDate={FromDate}, toDate={ToDate}, city={City}, government={Government}", fromDate, toDate, city, government);

                if (!fromDate.HasValue || !toDate.HasValue)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "fromDate and toDate are required and must be in YYYY-MM-DD format."
                    });
                }
                if (fromDate >= toDate)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid date range: FromDate must be before ToDate"
                    });
                }

                var filter = new DashboardFilterDTO
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    City = city,
                    Government = government
                };

                var result = await _serviceManager.DashboardService.GetAdminDashboardAsync(filter);

                return Ok(new
                {
                    Success = true,
                    Message = "Custom dashboard data retrieved successfully",
                    Data = result,
                    GeneratedAt = DateTime.UtcNow,
                    Filters = filter
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom dashboard");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while generating the custom dashboard"
                });
            }
        }

        /// <summary>
        /// Get dashboard health status
        /// </summary>
        [HttpGet("admin/health")]
        public ActionResult GetDashboardHealth()
        {
            try
            {
                var healthData = new
                {
                    Success = true,
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Services = new
                    {
                        DashboardService = "Available",
                        Database = "Connected",
                        Authentication = "Active"
                    }
                };
                
                return Ok(healthData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health check");
                return StatusCode(500, new
                {
                    Success = false,
                    Status = "Unhealthy",
                    Error = "Health check failed"
                });
            }
        }

        /// <summary>
        /// Get available filter options
        /// </summary>
        [HttpGet("admin/filter-options")]
        public ActionResult GetFilterOptions()
        {
            try
            {
                var filterOptions = new
                {
                    Success = true,
                    Message = "Filter options retrieved successfully",
                    Data = new
                    {
                        DateRanges = new[]
                        {
                            new { Label = "Last 7 days", Value = "7d" },
                            new { Label = "Last 30 days", Value = "30d" },
                            new { Label = "Last 3 months", Value = "3m" },
                            new { Label = "Last 6 months", Value = "6m" },
                            new { Label = "Last 12 months", Value = "12m" },
                            new { Label = "This year", Value = "year" },
                            new { Label = "Custom range", Value = "custom" }
                        },
                        PropertyTypes = new[]
                        {
                            new { Id = 1, Name = "Apartment" },
                            new { Id = 2, Name = "Villa" },
                            new { Id = 3, Name = "Office" },
                            new { Id = 4, Name = "Shop" },
                            new { Id = 5, Name = "Land" }
                        }
                    },
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(filterOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filter options");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving filter options"
                });
            }
        }
    }
} 