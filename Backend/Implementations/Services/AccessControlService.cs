using DataTrustNexus.Api.Interfaces;
using DataTrustNexus.Api.Models;
using DataTrustNexus.Api.Models.DTOs;

namespace DataTrustNexus.Api.Implementations.Services;

/// <summary>
/// Service for access control operations
/// </summary>
public class AccessControlService : IAccessControlService
{
    private readonly IRepository<AccessRequest> _accessRepository;
    private readonly IRepository<DataRecord> _dataRepository;
    private readonly IRepository<Institution> _institutionRepository;
    private readonly IBlockchainService _blockchainService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AccessControlService> _logger;

    public AccessControlService(
        IRepository<AccessRequest> accessRepository,
        IRepository<DataRecord> dataRepository,
        IRepository<Institution> institutionRepository,
        IBlockchainService blockchainService,
        IAuditService auditService,
        ILogger<AccessControlService> logger)
    {
        _accessRepository = accessRepository;
        _dataRepository = dataRepository;
        _institutionRepository = institutionRepository;
        _blockchainService = blockchainService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<AccessControlResponseDto> GrantAccessAsync(GrantAccessDto dto, string ownerWalletAddress)
    {
        try
        {
            // Verify owner institution
            var owners = await _institutionRepository.FindAsync(i => i.WalletAddress == ownerWalletAddress);
            var owner = owners.FirstOrDefault();
            
            if (owner == null || !owner.IsActive)
            {
                return new AccessControlResponseDto
                {
                    Success = false,
                    Message = "Owner institution not found or inactive"
                };
            }

            // Verify record exists and belongs to owner
            var records = await _dataRepository.FindAsync(d => d.RecordId == dto.RecordId);
            var record = records.FirstOrDefault();
            
            if (record == null)
            {
                return new AccessControlResponseDto
                {
                    Success = false,
                    Message = "Record not found"
                };
            }

            if (record.OwnerId != owner.Id)
            {
                return new AccessControlResponseDto
                {
                    Success = false,
                    Message = "You do not own this record"
                };
            }

            // Verify grantee institution exists
            var grantees = await _institutionRepository.FindAsync(i => i.WalletAddress == dto.GranteeWalletAddress);
            if (!grantees.Any())
            {
                return new AccessControlResponseDto
                {
                    Success = false,
                    Message = "Grantee institution not found"
                };
            }

            // Check if access already exists
            var existing = await _accessRepository.FindAsync(a => 
                a.RecordId == record.Id && 
                a.GranteeWalletAddress == dto.GranteeWalletAddress &&
                a.IsActive);
            
            if (existing.Any())
            {
                return new AccessControlResponseDto
                {
                    Success = false,
                    Message = "Access already granted to this institution"
                };
            }

            var accessRequest = new AccessRequest
            {
                RecordId = record.Id,
                GranterId = owner.Id,
                GranteeWalletAddress = dto.GranteeWalletAddress,
                GrantedAt = DateTime.UtcNow,
                ExpiresAt = dto.ExpiresAt,
                IsActive = true,
                PermissionType = dto.PermissionType,
                GrantReason = dto.GrantReason
            };

            var created = await _accessRepository.AddAsync(accessRequest);

            // Grant access on blockchain
            string txHash = "";
            try
            {
                long expiresAt = dto.ExpiresAt?.Ticks ?? 0;
                txHash = await _blockchainService.GrantAccessOnBlockchainAsync(
                    dto.RecordId,
                    dto.GranteeWalletAddress,
                    expiresAt,
                    dto.PermissionType,
                    dto.GrantReason ?? "",
                    "");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to grant access on blockchain");
            }

            // Log access grant
            await _auditService.LogAccessGrantAsync(
                owner.Id,
                dto.RecordId,
                dto.GranteeWalletAddress,
                "");

            return new AccessControlResponseDto
            {
                RecordId = dto.RecordId,
                GranteeWalletAddress = dto.GranteeWalletAddress,
                Success = true,
                Message = "Access granted successfully",
                TransactionHash = txHash,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to grant access");
            return new AccessControlResponseDto
            {
                Success = false,
                Message = $"Grant access failed: {ex.Message}"
            };
        }
    }

    public async Task<AccessControlResponseDto> RevokeAccessAsync(RevokeAccessDto dto, string ownerWalletAddress)
    {
        try
        {
            // Verify owner institution
            var owners = await _institutionRepository.FindAsync(i => i.WalletAddress == ownerWalletAddress);
            var owner = owners.FirstOrDefault();
            
            if (owner == null || !owner.IsActive)
            {
                return new AccessControlResponseDto
                {
                    Success = false,
                    Message = "Owner institution not found or inactive"
                };
            }

            // Verify record exists and belongs to owner
            var records = await _dataRepository.FindAsync(d => d.RecordId == dto.RecordId);
            var record = records.FirstOrDefault();
            
            if (record == null || record.OwnerId != owner.Id)
            {
                return new AccessControlResponseDto
                {
                    Success = false,
                    Message = "Record not found or you do not own this record"
                };
            }

            // Find access request
            var accessRequests = await _accessRepository.FindAsync(a => 
                a.RecordId == record.Id && 
                a.GranteeWalletAddress == dto.GranteeWalletAddress &&
                a.IsActive);
            
            var accessRequest = accessRequests.FirstOrDefault();
            
            if (accessRequest == null)
            {
                return new AccessControlResponseDto
                {
                    Success = false,
                    Message = "No active access found for this grantee"
                };
            }

            accessRequest.IsActive = false;
            accessRequest.RevokedAt = DateTime.UtcNow;
            accessRequest.RevokeReason = dto.RevokeReason;
            
            await _accessRepository.UpdateAsync(accessRequest);

            // Revoke access on blockchain
            string txHash = "";
            try
            {
                txHash = await _blockchainService.RevokeAccessOnBlockchainAsync(
                    dto.RecordId,
                    dto.GranteeWalletAddress,
                    "");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to revoke access on blockchain");
            }

            // Log access revocation
            await _auditService.LogAccessRevokeAsync(
                owner.Id,
                dto.RecordId,
                dto.GranteeWalletAddress,
                "");

            return new AccessControlResponseDto
            {
                RecordId = dto.RecordId,
                GranteeWalletAddress = dto.GranteeWalletAddress,
                Success = true,
                Message = "Access revoked successfully",
                TransactionHash = txHash,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke access");
            return new AccessControlResponseDto
            {
                Success = false,
                Message = $"Revoke access failed: {ex.Message}"
            };
        }
    }

    public async Task<CheckAccessResponseDto> CheckAccessAsync(string recordId, string walletAddress)
    {
        try
        {
            var records = await _dataRepository.FindAsync(d => d.RecordId == recordId);
            var record = records.FirstOrDefault();
            
            if (record == null)
            {
                return new CheckAccessResponseDto
                {
                    RecordId = recordId,
                    WalletAddress = walletAddress,
                    HasAccess = false
                };
            }

            // Owner always has access
            if (record.Owner?.WalletAddress == walletAddress)
            {
                return new CheckAccessResponseDto
                {
                    RecordId = recordId,
                    WalletAddress = walletAddress,
                    HasAccess = true,
                    PermissionType = "owner"
                };
            }

            // Check access requests
            var accessRequests = await _accessRepository.FindAsync(a => 
                a.RecordId == record.Id && 
                a.GranteeWalletAddress == walletAddress &&
                a.IsActive);
            
            var accessRequest = accessRequests.FirstOrDefault();
            
            if (accessRequest == null)
            {
                return new CheckAccessResponseDto
                {
                    RecordId = recordId,
                    WalletAddress = walletAddress,
                    HasAccess = false
                };
            }

            bool hasAccess = accessRequest.HasValidAccess;

            return new CheckAccessResponseDto
            {
                RecordId = recordId,
                WalletAddress = walletAddress,
                HasAccess = hasAccess,
                PermissionType = accessRequest.PermissionType,
                GrantedAt = accessRequest.GrantedAt,
                ExpiresAt = accessRequest.ExpiresAt,
                IsExpired = accessRequest.IsExpired
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check access");
            return new CheckAccessResponseDto
            {
                RecordId = recordId,
                WalletAddress = walletAddress,
                HasAccess = false
            };
        }
    }

    public async Task<IEnumerable<AccessPermissionDto>> GetRecordPermissionsAsync(string recordId)
    {
        var records = await _dataRepository.FindAsync(d => d.RecordId == recordId);
        var record = records.FirstOrDefault();
        
        if (record == null) return Enumerable.Empty<AccessPermissionDto>();

        var accessRequests = await _accessRepository.FindAsync(a => a.RecordId == record.Id);
        return accessRequests.Select(MapToDto);
    }

    public async Task<IEnumerable<string>> GetAccessibleRecordsAsync(string walletAddress)
    {
        var accessRequests = await _accessRepository.FindAsync(a => 
            a.GranteeWalletAddress == walletAddress && 
            a.IsActive);
        
        var recordIds = new List<string>();
        foreach (var access in accessRequests)
        {
            if (access.DataRecord != null)
            {
                recordIds.Add(access.DataRecord.RecordId);
            }
        }
        
        return recordIds;
    }

    public async Task<bool> UpdatePermissionAsync(string recordId, string granteeAddress, DateTime? newExpiresAt, string newPermissionType)
    {
        var records = await _dataRepository.FindAsync(d => d.RecordId == recordId);
        var record = records.FirstOrDefault();
        
        if (record == null) return false;

        var accessRequests = await _accessRepository.FindAsync(a => 
            a.RecordId == record.Id && 
            a.GranteeWalletAddress == granteeAddress &&
            a.IsActive);
        
        var accessRequest = accessRequests.FirstOrDefault();
        
        if (accessRequest == null) return false;

        accessRequest.ExpiresAt = newExpiresAt;
        accessRequest.PermissionType = newPermissionType;
        
        await _accessRepository.UpdateAsync(accessRequest);
        return true;
    }

    public async Task<int> ExpireOldPermissionsAsync()
    {
        var expiredRequests = await _accessRepository.FindAsync(a => 
            a.IsActive && 
            a.ExpiresAt.HasValue && 
            a.ExpiresAt.Value < DateTime.UtcNow);

        int count = 0;
        foreach (var request in expiredRequests)
        {
            request.IsActive = false;
            await _accessRepository.UpdateAsync(request);
            count++;
        }

        return count;
    }

    public async Task<IEnumerable<AccessPermissionDto>> GetGrantedPermissionsAsync(string ownerWalletAddress)
    {
        var owners = await _institutionRepository.FindAsync(i => i.WalletAddress == ownerWalletAddress);
        var owner = owners.FirstOrDefault();
        
        if (owner == null) return Enumerable.Empty<AccessPermissionDto>();

        var accessRequests = await _accessRepository.FindAsync(a => a.GranterId == owner.Id);
        return accessRequests.Select(MapToDto);
    }

    public async Task<IEnumerable<AccessPermissionDto>> GetReceivedPermissionsAsync(string granteeWalletAddress)
    {
        var accessRequests = await _accessRepository.FindAsync(a => 
            a.GranteeWalletAddress == granteeWalletAddress);
        return accessRequests.Select(MapToDto);
    }

    private AccessPermissionDto MapToDto(AccessRequest accessRequest)
    {
        return new AccessPermissionDto
        {
            Id = accessRequest.Id,
            RecordId = accessRequest.DataRecord?.RecordId ?? "",
            RecordFileName = accessRequest.DataRecord?.FileName ?? "",
            GranteeWalletAddress = accessRequest.GranteeWalletAddress,
            GrantedAt = accessRequest.GrantedAt,
            ExpiresAt = accessRequest.ExpiresAt,
            IsActive = accessRequest.IsActive,
            PermissionType = accessRequest.PermissionType,
            GrantReason = accessRequest.GrantReason,
            IsExpired = accessRequest.IsExpired,
            HasValidAccess = accessRequest.HasValidAccess
        };
    }
}

