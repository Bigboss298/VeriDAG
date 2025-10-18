using DataTrustNexus.Api.Models;
using DataTrustNexus.Api.Models.DTOs;

namespace DataTrustNexus.Api.Interfaces;

/// <summary>
/// Service interface for audit logging
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Create an audit log entry
    /// </summary>
    Task<AuditLog> CreateAuditLogAsync(
        string actionType,
        string actorId,
        string? targetWalletAddress,
        string? recordId,
        string? dataHash,
        string actionDetails,
        bool success,
        string? ipAddress,
        string? userAgent,
        string? errorMessage = null);
    
    /// <summary>
    /// Get audit logs by actor
    /// </summary>
    Task<IEnumerable<AuditLogDto>> GetLogsByActorAsync(string actorWalletAddress);
    
    /// <summary>
    /// Get audit logs by record
    /// </summary>
    Task<IEnumerable<AuditLogDto>> GetLogsByRecordAsync(string recordId);
    
    /// <summary>
    /// Get audit logs by action type
    /// </summary>
    Task<IEnumerable<AuditLogDto>> GetLogsByActionTypeAsync(string actionType);
    
    /// <summary>
    /// Query audit logs with filters
    /// </summary>
    Task<AuditLogResponseDto> QueryLogsAsync(AuditLogQueryDto query);
    
    /// <summary>
    /// Get recent audit logs
    /// </summary>
    Task<IEnumerable<AuditLogDto>> GetRecentLogsAsync(int count = 50);
    
    /// <summary>
    /// Get audit statistics
    /// </summary>
    Task<AuditStatisticsDto> GetAuditStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Log institution registration
    /// </summary>
    Task LogInstitutionRegistrationAsync(string institutionId, string walletAddress, string name, string ipAddress);
    
    /// <summary>
    /// Log data upload
    /// </summary>
    Task LogDataUploadAsync(string institutionId, string recordId, string dataHash, string fileName, string ipAddress);
    
    /// <summary>
    /// Log access grant
    /// </summary>
    Task LogAccessGrantAsync(string institutionId, string recordId, string granteeAddress, string ipAddress);
    
    /// <summary>
    /// Log access revocation
    /// </summary>
    Task LogAccessRevokeAsync(string institutionId, string recordId, string granteeAddress, string ipAddress);
    
    /// <summary>
    /// Log verification request
    /// </summary>
    Task LogVerificationRequestAsync(string institutionId, string recordId, string dataHash, string ipAddress);
    
    /// <summary>
    /// Log verification completion
    /// </summary>
    Task LogVerificationCompletionAsync(string institutionId, string recordId, string dataHash, bool success, string ipAddress);
    
    /// <summary>
    /// Log data access
    /// </summary>
    Task LogDataAccessAsync(string institutionId, string recordId, string ipAddress);
}

