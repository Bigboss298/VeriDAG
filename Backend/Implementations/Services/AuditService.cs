using DataTrustNexus.Api.Interfaces;
using DataTrustNexus.Api.Models;
using DataTrustNexus.Api.Models.DTOs;

namespace DataTrustNexus.Api.Implementations.Services;

/// <summary>
/// Service for audit logging operations
/// </summary>
public class AuditService : IAuditService
{
    private readonly IRepository<AuditLog> _auditRepository;
    private readonly IRepository<Institution> _institutionRepository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IRepository<AuditLog> auditRepository,
        IRepository<Institution> institutionRepository,
        ILogger<AuditService> logger)
    {
        _auditRepository = auditRepository;
        _institutionRepository = institutionRepository;
        _logger = logger;
    }

    public async Task<AuditLog> CreateAuditLogAsync(
        string actionType,
        string actorId,
        string? targetWalletAddress,
        string? recordId,
        string? dataHash,
        string actionDetails,
        bool success,
        string? ipAddress,
        string? userAgent,
        string? errorMessage = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                ActionType = actionType,
                ActorId = actorId,
                TargetWalletAddress = targetWalletAddress,
                RecordId = recordId,
                DataHash = dataHash,
                ActionDetails = actionDetails,
                Success = success,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ErrorMessage = errorMessage
            };

            return await _auditRepository.AddAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log");
            throw;
        }
    }

    public async Task<IEnumerable<AuditLogDto>> GetLogsByActorAsync(string actorWalletAddress)
    {
        var actors = await _institutionRepository.FindAsync(i => i.WalletAddress == actorWalletAddress);
        var actor = actors.FirstOrDefault();
        
        if (actor == null) return Enumerable.Empty<AuditLogDto>();

        var logs = await _auditRepository.FindAsync(a => a.ActorId == actor.Id);
        return logs.OrderByDescending(l => l.Timestamp).Select(MapToDto);
    }

    public async Task<IEnumerable<AuditLogDto>> GetLogsByRecordAsync(string recordId)
    {
        var logs = await _auditRepository.FindAsync(a => a.RecordId == recordId);
        return logs.OrderByDescending(l => l.Timestamp).Select(MapToDto);
    }

    public async Task<IEnumerable<AuditLogDto>> GetLogsByActionTypeAsync(string actionType)
    {
        var logs = await _auditRepository.FindAsync(a => a.ActionType == actionType);
        return logs.OrderByDescending(l => l.Timestamp).Select(MapToDto);
    }

    public async Task<AuditLogResponseDto> QueryLogsAsync(AuditLogQueryDto query)
    {
        var allLogs = await _auditRepository.GetAllAsync();
        var filteredLogs = allLogs.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.ActorWalletAddress))
        {
            var actors = await _institutionRepository.FindAsync(i => i.WalletAddress == query.ActorWalletAddress);
            var actor = actors.FirstOrDefault();
            if (actor != null)
            {
                filteredLogs = filteredLogs.Where(l => l.ActorId == actor.Id);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.RecordId))
        {
            filteredLogs = filteredLogs.Where(l => l.RecordId == query.RecordId);
        }

        if (!string.IsNullOrWhiteSpace(query.ActionType))
        {
            filteredLogs = filteredLogs.Where(l => l.ActionType == query.ActionType);
        }

        if (query.StartDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(l => l.Timestamp >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(l => l.Timestamp <= query.EndDate.Value);
        }

        var totalCount = filteredLogs.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var pagedLogs = filteredLogs
            .OrderByDescending(l => l.Timestamp)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToDto)
            .ToList();

        return new AuditLogResponseDto
        {
            Logs = pagedLogs,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<IEnumerable<AuditLogDto>> GetRecentLogsAsync(int count = 50)
    {
        var allLogs = await _auditRepository.GetAllAsync();
        return allLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .Select(MapToDto);
    }

    public async Task<AuditStatisticsDto> GetAuditStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var allLogs = await _auditRepository.GetAllAsync();
        var filteredLogs = allLogs.AsQueryable();

        if (startDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(l => l.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            filteredLogs = filteredLogs.Where(l => l.Timestamp <= endDate.Value);
        }

        var logsList = filteredLogs.ToList();

        var stats = new AuditStatisticsDto
        {
            TotalUploads = logsList.Count(l => l.ActionType == AuditActionTypes.DataUploaded),
            TotalVerifications = logsList.Count(l => 
                l.ActionType == AuditActionTypes.VerificationRequested || 
                l.ActionType == AuditActionTypes.VerificationCompleted),
            SuccessfulVerifications = logsList.Count(l => 
                l.ActionType == AuditActionTypes.VerificationCompleted && l.Success),
            FailedVerifications = logsList.Count(l => 
                l.ActionType == AuditActionTypes.VerificationCompleted && !l.Success),
            TotalAccessGrants = logsList.Count(l => l.ActionType == AuditActionTypes.AccessGranted),
            TotalAccessRevocations = logsList.Count(l => l.ActionType == AuditActionTypes.AccessRevoked),
            TotalInstitutions = logsList.Count(l => l.ActionType == AuditActionTypes.InstitutionRegistered),
            ActionTypeCounts = logsList.GroupBy(l => l.ActionType)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentActivities = logsList
                .OrderByDescending(l => l.Timestamp)
                .Take(10)
                .Select(l => new RecentActivityDto
                {
                    ActionType = l.ActionType,
                    ActorName = l.Actor?.Name ?? "Unknown",
                    Details = l.ActionDetails,
                    Timestamp = l.Timestamp,
                    Success = l.Success
                })
                .ToList()
        };

        return stats;
    }

    public async Task LogInstitutionRegistrationAsync(string institutionId, string walletAddress, string name, string ipAddress)
    {
        await CreateAuditLogAsync(
            AuditActionTypes.InstitutionRegistered,
            institutionId,
            walletAddress,
            null,
            null,
            $"Institution '{name}' registered with wallet {walletAddress}",
            true,
            ipAddress,
            null);
    }

    public async Task LogDataUploadAsync(string institutionId, string recordId, string dataHash, string fileName, string ipAddress)
    {
        await CreateAuditLogAsync(
            AuditActionTypes.DataUploaded,
            institutionId,
            null,
            recordId,
            dataHash,
            $"Data file '{fileName}' uploaded with record ID {recordId}",
            true,
            ipAddress,
            null);
    }

    public async Task LogAccessGrantAsync(string institutionId, string recordId, string granteeAddress, string ipAddress)
    {
        await CreateAuditLogAsync(
            AuditActionTypes.AccessGranted,
            institutionId,
            granteeAddress,
            recordId,
            null,
            $"Access granted to {granteeAddress} for record {recordId}",
            true,
            ipAddress,
            null);
    }

    public async Task LogAccessRevokeAsync(string institutionId, string recordId, string granteeAddress, string ipAddress)
    {
        await CreateAuditLogAsync(
            AuditActionTypes.AccessRevoked,
            institutionId,
            granteeAddress,
            recordId,
            null,
            $"Access revoked from {granteeAddress} for record {recordId}",
            true,
            ipAddress,
            null);
    }

    public async Task LogVerificationRequestAsync(string institutionId, string recordId, string dataHash, string ipAddress)
    {
        await CreateAuditLogAsync(
            AuditActionTypes.VerificationRequested,
            institutionId,
            null,
            recordId,
            dataHash,
            $"Verification requested for record {recordId}",
            true,
            ipAddress,
            null);
    }

    public async Task LogVerificationCompletionAsync(string institutionId, string recordId, string dataHash, bool success, string ipAddress)
    {
        await CreateAuditLogAsync(
            AuditActionTypes.VerificationCompleted,
            institutionId,
            null,
            recordId,
            dataHash,
            $"Verification {(success ? "successful" : "failed")} for record {recordId}",
            success,
            ipAddress,
            null);
    }

    public async Task LogDataAccessAsync(string institutionId, string recordId, string ipAddress)
    {
        await CreateAuditLogAsync(
            AuditActionTypes.DataAccessed,
            institutionId,
            null,
            recordId,
            null,
            $"Data accessed for record {recordId}",
            true,
            ipAddress,
            null);
    }

    private AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            ActionType = log.ActionType,
            ActorId = log.ActorId,
            ActorName = log.Actor?.Name ?? "Unknown",
            ActorWalletAddress = log.Actor?.WalletAddress ?? "",
            TargetWalletAddress = log.TargetWalletAddress,
            RecordId = log.RecordId,
            DataHash = log.DataHash,
            ActionDetails = log.ActionDetails,
            Success = log.Success,
            Timestamp = log.Timestamp,
            IpAddress = log.IpAddress,
            ErrorMessage = log.ErrorMessage,
            BlockchainTransactionHash = log.BlockchainTransactionHash
        };
    }
}

