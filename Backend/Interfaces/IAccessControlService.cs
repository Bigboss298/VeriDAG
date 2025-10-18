using DataTrustNexus.Api.Models;
using DataTrustNexus.Api.Models.DTOs;

namespace DataTrustNexus.Api.Interfaces;

/// <summary>
/// Service interface for access control operations
/// </summary>
public interface IAccessControlService
{
    /// <summary>
    /// Grant access to a data record
    /// </summary>
    Task<AccessControlResponseDto> GrantAccessAsync(GrantAccessDto dto, string ownerWalletAddress);
    
    /// <summary>
    /// Revoke access to a data record
    /// </summary>
    Task<AccessControlResponseDto> RevokeAccessAsync(RevokeAccessDto dto, string ownerWalletAddress);
    
    /// <summary>
    /// Check if a wallet has access to a record
    /// </summary>
    Task<CheckAccessResponseDto> CheckAccessAsync(string recordId, string walletAddress);
    
    /// <summary>
    /// Get all access permissions for a record
    /// </summary>
    Task<IEnumerable<AccessPermissionDto>> GetRecordPermissionsAsync(string recordId);
    
    /// <summary>
    /// Get all records accessible by a wallet
    /// </summary>
    Task<IEnumerable<string>> GetAccessibleRecordsAsync(string walletAddress);
    
    /// <summary>
    /// Update access permission
    /// </summary>
    Task<bool> UpdatePermissionAsync(string recordId, string granteeAddress, DateTime? newExpiresAt, string newPermissionType);
    
    /// <summary>
    /// Expire old permissions
    /// </summary>
    Task<int> ExpireOldPermissionsAsync();
    
    /// <summary>
    /// Get permissions granted by an institution
    /// </summary>
    Task<IEnumerable<AccessPermissionDto>> GetGrantedPermissionsAsync(string ownerWalletAddress);
    
    /// <summary>
    /// Get permissions received by an institution
    /// </summary>
    Task<IEnumerable<AccessPermissionDto>> GetReceivedPermissionsAsync(string granteeWalletAddress);
}

