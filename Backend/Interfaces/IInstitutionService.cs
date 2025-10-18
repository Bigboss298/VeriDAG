using DataTrustNexus.Api.Models;
using DataTrustNexus.Api.Models.DTOs;

namespace DataTrustNexus.Api.Interfaces;

/// <summary>
/// Service interface for institution operations
/// </summary>
public interface IInstitutionService
{
    /// <summary>
    /// Register a new institution
    /// </summary>
    Task<InstitutionResponseDto> RegisterInstitutionAsync(RegisterInstitutionDto dto);
    
    /// <summary>
    /// Get institution by wallet address
    /// </summary>
    Task<InstitutionDto?> GetInstitutionByWalletAsync(string walletAddress);
    
    /// <summary>
    /// Get institution by ID
    /// </summary>
    Task<InstitutionDto?> GetInstitutionByIdAsync(string id);
    
    /// <summary>
    /// Update institution details
    /// </summary>
    Task<InstitutionResponseDto> UpdateInstitutionAsync(string walletAddress, UpdateInstitutionDto dto);
    
    /// <summary>
    /// Deactivate institution
    /// </summary>
    Task<bool> DeactivateInstitutionAsync(string walletAddress);
    
    /// <summary>
    /// Reactivate institution
    /// </summary>
    Task<bool> ReactivateInstitutionAsync(string walletAddress);
    
    /// <summary>
    /// Verify institution exists and is active
    /// </summary>
    Task<bool> VerifyInstitutionAsync(string walletAddress);
    
    /// <summary>
    /// Get all institutions
    /// </summary>
    Task<IEnumerable<InstitutionDto>> GetAllInstitutionsAsync();
    
    /// <summary>
    /// Get institutions by type
    /// </summary>
    Task<IEnumerable<InstitutionDto>> GetInstitutionsByTypeAsync(string institutionType);
    
    /// <summary>
    /// Search institutions
    /// </summary>
    Task<IEnumerable<InstitutionDto>> SearchInstitutionsAsync(string searchTerm);
}

