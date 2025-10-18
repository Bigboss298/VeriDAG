using DataTrustNexus.Api.Models;
using DataTrustNexus.Api.Models.DTOs;

namespace DataTrustNexus.Api.Interfaces;

/// <summary>
/// Service interface for data operations
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Upload a new data record
    /// </summary>
    Task<UploadDataResponseDto> UploadDataAsync(UploadDataDto dto, string ownerWalletAddress);
    
    /// <summary>
    /// Verify data integrity
    /// </summary>
    Task<VerifyDataResponseDto> VerifyDataAsync(VerifyDataDto dto, string verifierWalletAddress);
    
    /// <summary>
    /// Get data record by ID
    /// </summary>
    Task<DataRecordDetailsDto?> GetDataRecordAsync(string recordId);
    
    /// <summary>
    /// Get all data records for an institution
    /// </summary>
    Task<IEnumerable<DataRecordDetailsDto>> GetInstitutionDataRecordsAsync(string walletAddress);
    
    /// <summary>
    /// Deactivate a data record
    /// </summary>
    Task<bool> DeactivateRecordAsync(string recordId, string ownerWalletAddress);
    
    /// <summary>
    /// Reactivate a data record
    /// </summary>
    Task<bool> ReactivateRecordAsync(string recordId, string ownerWalletAddress);
    
    /// <summary>
    /// Get data records by category
    /// </summary>
    Task<IEnumerable<DataRecordDetailsDto>> GetRecordsByCategoryAsync(string category);
    
    /// <summary>
    /// Search data records
    /// </summary>
    Task<IEnumerable<DataRecordDetailsDto>> SearchRecordsAsync(string searchTerm);
}

