using DataTrustNexus.Api.Interfaces;
using DataTrustNexus.Api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DataTrustNexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstitutionController : ControllerBase
{
    private readonly IInstitutionService _institutionService;
    private readonly ILogger<InstitutionController> _logger;

    public InstitutionController(
        IInstitutionService institutionService,
        ILogger<InstitutionController> logger)
    {
        _institutionService = institutionService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new institution
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<InstitutionResponseDto>> RegisterInstitution(
        [FromBody] RegisterInstitutionDto dto)
    {
        try
        {
            var result = await _institutionService.RegisterInstitutionAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering institution");
            return StatusCode(500, new InstitutionResponseDto 
            { 
                Success = false, 
                Message = "Internal server error" 
            });
        }
    }

    /// <summary>
    /// Get institution by wallet address
    /// </summary>
    [HttpGet("wallet/{walletAddress}")]
    public async Task<ActionResult<InstitutionDto>> GetInstitutionByWallet(string walletAddress)
    {
        try
        {
            var institution = await _institutionService.GetInstitutionByWalletAsync(walletAddress);
            
            if (institution == null)
            {
                return NotFound(new { message = "Institution not found" });
            }

            return Ok(institution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving institution");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get institution by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InstitutionDto>> GetInstitutionById(string id)
    {
        try
        {
            var institution = await _institutionService.GetInstitutionByIdAsync(id);
            
            if (institution == null)
            {
                return NotFound(new { message = "Institution not found" });
            }

            return Ok(institution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving institution");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update institution details
    /// </summary>
    [HttpPut("{walletAddress}")]
    public async Task<ActionResult<InstitutionResponseDto>> UpdateInstitution(
        string walletAddress,
        [FromBody] UpdateInstitutionDto dto)
    {
        try
        {
            var result = await _institutionService.UpdateInstitutionAsync(walletAddress, dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating institution");
            return StatusCode(500, new InstitutionResponseDto 
            { 
                Success = false, 
                Message = "Internal server error" 
            });
        }
    }

    /// <summary>
    /// Verify institution status
    /// </summary>
    [HttpGet("verify/{walletAddress}")]
    public async Task<ActionResult<bool>> VerifyInstitution(string walletAddress)
    {
        try
        {
            var isValid = await _institutionService.VerifyInstitutionAsync(walletAddress);
            return Ok(new { walletAddress, isVerified = isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying institution");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all institutions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InstitutionDto>>> GetAllInstitutions()
    {
        try
        {
            var institutions = await _institutionService.GetAllInstitutionsAsync();
            return Ok(institutions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving institutions");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get institutions by type
    /// </summary>
    [HttpGet("type/{institutionType}")]
    public async Task<ActionResult<IEnumerable<InstitutionDto>>> GetInstitutionsByType(
        string institutionType)
    {
        try
        {
            var institutions = await _institutionService.GetInstitutionsByTypeAsync(institutionType);
            return Ok(institutions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving institutions by type");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Search institutions
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<InstitutionDto>>> SearchInstitutions(
        [FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { message = "Search term is required" });
            }

            var institutions = await _institutionService.SearchInstitutionsAsync(searchTerm);
            return Ok(institutions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching institutions");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

