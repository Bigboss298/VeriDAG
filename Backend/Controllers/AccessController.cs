using DataTrustNexus.Api.Interfaces;
using DataTrustNexus.Api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DataTrustNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccessController : ControllerBase
{
    private readonly IAccessControlService _accessControlService;
    private readonly ILogger<AccessController> _logger;

    public AccessController(
        IAccessControlService accessControlService,
        ILogger<AccessController> logger)
    {
        _accessControlService = accessControlService;
        _logger = logger;
    }

    /// <summary>
    /// Grant access to a data record
    /// </summary>
    [HttpPost("grant")]
    public async Task<ActionResult<AccessControlResponseDto>> GrantAccess(
        [FromBody] GrantAccessDto dto,
        [FromHeader(Name = "X-Wallet-Address")] string walletAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return BadRequest(new { message = "Wallet address is required in X-Wallet-Address header" });
            }

            var result = await _accessControlService.GrantAccessAsync(dto, walletAddress);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting access");
            return StatusCode(500, new AccessControlResponseDto 
            { 
                Success = false, 
                Message = "Internal server error" 
            });
        }
    }

    /// <summary>
    /// Revoke access to a data record
    /// </summary>
    [HttpPost("revoke")]
    public async Task<ActionResult<AccessControlResponseDto>> RevokeAccess(
        [FromBody] RevokeAccessDto dto,
        [FromHeader(Name = "X-Wallet-Address")] string walletAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return BadRequest(new { message = "Wallet address is required in X-Wallet-Address header" });
            }

            var result = await _accessControlService.RevokeAccessAsync(dto, walletAddress);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking access");
            return StatusCode(500, new AccessControlResponseDto 
            { 
                Success = false, 
                Message = "Internal server error" 
            });
        }
    }

    /// <summary>
    /// Check if a wallet has access to a record
    /// </summary>
    [HttpGet("check")]
    public async Task<ActionResult<CheckAccessResponseDto>> CheckAccess(
        [FromQuery] string recordId,
        [FromQuery] string walletAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordId) || string.IsNullOrWhiteSpace(walletAddress))
            {
                return BadRequest(new { message = "Both recordId and walletAddress are required" });
            }

            var result = await _accessControlService.CheckAccessAsync(recordId, walletAddress);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking access");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all access permissions for a record
    /// </summary>
    [HttpGet("record/{recordId}")]
    public async Task<ActionResult<IEnumerable<AccessPermissionDto>>> GetRecordPermissions(
        string recordId)
    {
        try
        {
            var permissions = await _accessControlService.GetRecordPermissionsAsync(recordId);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving record permissions");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all records accessible by a wallet
    /// </summary>
    [HttpGet("accessible/{walletAddress}")]
    public async Task<ActionResult<IEnumerable<string>>> GetAccessibleRecords(
        string walletAddress)
    {
        try
        {
            var recordIds = await _accessControlService.GetAccessibleRecordsAsync(walletAddress);
            return Ok(recordIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accessible records");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get permissions granted by an institution
    /// </summary>
    [HttpGet("granted/{walletAddress}")]
    public async Task<ActionResult<IEnumerable<AccessPermissionDto>>> GetGrantedPermissions(
        string walletAddress)
    {
        try
        {
            var permissions = await _accessControlService.GetGrantedPermissionsAsync(walletAddress);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving granted permissions");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get permissions received by an institution
    /// </summary>
    [HttpGet("received/{walletAddress}")]
    public async Task<ActionResult<IEnumerable<AccessPermissionDto>>> GetReceivedPermissions(
        string walletAddress)
    {
        try
        {
            var permissions = await _accessControlService.GetReceivedPermissionsAsync(walletAddress);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving received permissions");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an access permission
    /// </summary>
    [HttpPut("update")]
    public async Task<ActionResult> UpdatePermission(
        [FromQuery] string recordId,
        [FromQuery] string granteeAddress,
        [FromQuery] DateTime? newExpiresAt,
        [FromQuery] string newPermissionType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordId) || 
                string.IsNullOrWhiteSpace(granteeAddress) || 
                string.IsNullOrWhiteSpace(newPermissionType))
            {
                return BadRequest(new { message = "recordId, granteeAddress, and newPermissionType are required" });
            }

            var success = await _accessControlService.UpdatePermissionAsync(
                recordId, 
                granteeAddress, 
                newExpiresAt, 
                newPermissionType);
            
            if (!success)
            {
                return NotFound(new { message = "Permission not found" });
            }

            return Ok(new { message = "Permission updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permission");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Expire old permissions
    /// </summary>
    [HttpPost("expire-old")]
    public async Task<ActionResult> ExpireOldPermissions()
    {
        try
        {
            var count = await _accessControlService.ExpireOldPermissionsAsync();
            return Ok(new { message = $"Expired {count} permissions" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring permissions");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

