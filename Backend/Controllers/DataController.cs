using DataTrustNexus.Api.Interfaces;
using DataTrustNexus.Api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DataTrustNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly IDataService _dataService;
    private readonly ILogger<DataController> _logger;

    public DataController(
        IDataService dataService,
        ILogger<DataController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a new data record
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<UploadDataResponseDto>> UploadData(
        [FromBody] UploadDataDto dto,
        [FromHeader(Name = "X-Wallet-Address")] string walletAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return BadRequest(new { message = "Wallet address is required in X-Wallet-Address header" });
            }

            var result = await _dataService.UploadDataAsync(dto, walletAddress);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading data");
            return StatusCode(500, new UploadDataResponseDto 
            { 
                Success = false, 
                Message = "Internal server error" 
            });
        }
    }

    /// <summary>
    /// Verify data integrity
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<VerifyDataResponseDto>> VerifyData(
        [FromBody] VerifyDataDto dto,
        [FromHeader(Name = "X-Wallet-Address")] string walletAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return BadRequest(new { message = "Wallet address is required in X-Wallet-Address header" });
            }

            var result = await _dataService.VerifyDataAsync(dto, walletAddress);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying data");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get data record by record ID
    /// </summary>
    [HttpGet("{recordId}")]
    public async Task<ActionResult<DataRecordDetailsDto>> GetDataRecord(string recordId)
    {
        try
        {
            var record = await _dataService.GetDataRecordAsync(recordId);
            
            if (record == null)
            {
                return NotFound(new { message = "Record not found" });
            }

            return Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data record");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all data records for an institution
    /// </summary>
    [HttpGet("institution/{walletAddress}")]
    public async Task<ActionResult<IEnumerable<DataRecordDetailsDto>>> GetInstitutionRecords(
        string walletAddress)
    {
        try
        {
            var records = await _dataService.GetInstitutionDataRecordsAsync(walletAddress);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving institution records");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Deactivate a data record
    /// </summary>
    [HttpPost("{recordId}/deactivate")]
    public async Task<ActionResult> DeactivateRecord(
        string recordId,
        [FromHeader(Name = "X-Wallet-Address")] string walletAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return BadRequest(new { message = "Wallet address is required in X-Wallet-Address header" });
            }

            var success = await _dataService.DeactivateRecordAsync(recordId, walletAddress);
            
            if (!success)
            {
                return NotFound(new { message = "Record not found or you do not have permission" });
            }

            return Ok(new { message = "Record deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating record");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Reactivate a data record
    /// </summary>
    [HttpPost("{recordId}/reactivate")]
    public async Task<ActionResult> ReactivateRecord(
        string recordId,
        [FromHeader(Name = "X-Wallet-Address")] string walletAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return BadRequest(new { message = "Wallet address is required in X-Wallet-Address header" });
            }

            var success = await _dataService.ReactivateRecordAsync(recordId, walletAddress);
            
            if (!success)
            {
                return NotFound(new { message = "Record not found or you do not have permission" });
            }

            return Ok(new { message = "Record reactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating record");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get records by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<DataRecordDetailsDto>>> GetRecordsByCategory(
        string category)
    {
        try
        {
            var records = await _dataService.GetRecordsByCategoryAsync(category);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving records by category");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Search data records
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<DataRecordDetailsDto>>> SearchRecords(
        [FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { message = "Search term is required" });
            }

            var records = await _dataService.SearchRecordsAsync(searchTerm);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching records");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

