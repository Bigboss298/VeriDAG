using DataTrustNexus.Api.Interfaces;
using DataTrustNexus.Api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DataTrustNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditService auditService,
        ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get audit logs by actor wallet address
    /// </summary>
    [HttpGet("actor/{walletAddress}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetLogsByActor(
        string walletAddress)
    {
        try
        {
            var logs = await _auditService.GetLogsByActorAsync(walletAddress);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs by actor");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get audit logs by record ID
    /// </summary>
    [HttpGet("record/{recordId}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetLogsByRecord(
        string recordId)
    {
        try
        {
            var logs = await _auditService.GetLogsByRecordAsync(recordId);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs by record");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get audit logs by action type
    /// </summary>
    [HttpGet("action/{actionType}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetLogsByActionType(
        string actionType)
    {
        try
        {
            var logs = await _auditService.GetLogsByActionTypeAsync(actionType);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs by action type");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Query audit logs with filters
    /// </summary>
    [HttpPost("query")]
    public async Task<ActionResult<AuditLogResponseDto>> QueryLogs(
        [FromBody] AuditLogQueryDto query)
    {
        try
        {
            var result = await _auditService.QueryLogsAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying logs");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get recent audit logs
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetRecentLogs(
        [FromQuery] int count = 50)
    {
        try
        {
            if (count <= 0 || count > 500)
            {
                return BadRequest(new { message = "Count must be between 1 and 500" });
            }

            var logs = await _auditService.GetRecentLogsAsync(count);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent logs");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get audit statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<AuditStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _auditService.GetAuditStatisticsAsync(startDate, endDate);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit statistics");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get audit logs (paginated)
    /// </summary>
    [HttpGet("logs")]
    public async Task<ActionResult<AuditLogResponseDto>> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 500)
            {
                return BadRequest(new { message = "Invalid page or pageSize parameters" });
            }

            var query = new AuditLogQueryDto
            {
                Page = page,
                PageSize = pageSize
            };

            var result = await _auditService.QueryLogsAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

