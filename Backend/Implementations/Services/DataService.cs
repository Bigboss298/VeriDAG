using DataTrustNexus.Api.Interfaces;
using DataTrustNexus.Api.Models;
using DataTrustNexus.Api.Models.DTOs;

namespace DataTrustNexus.Api.Implementations.Services;

/// <summary>
/// Service for data operations
/// </summary>
public class DataService : IDataService
{
    private readonly IRepository<DataRecord> _dataRepository;
    private readonly IRepository<Institution> _institutionRepository;
    private readonly IBlockchainService _blockchainService;
    private readonly IAuditService _auditService;
    private readonly ILogger<DataService> _logger;

    public DataService(
        IRepository<DataRecord> dataRepository,
        IRepository<Institution> institutionRepository,
        IBlockchainService blockchainService,
        IAuditService auditService,
        ILogger<DataService> logger)
    {
        _dataRepository = dataRepository;
        _institutionRepository = institutionRepository;
        _blockchainService = blockchainService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<UploadDataResponseDto> UploadDataAsync(UploadDataDto dto, string ownerWalletAddress)
    {
        try
        {
            // Verify institution exists
            var owners = await _institutionRepository.FindAsync(i => i.WalletAddress == ownerWalletAddress);
            var owner = owners.FirstOrDefault();
            
            if (owner == null || !owner.IsActive)
            {
                return new UploadDataResponseDto
                {
                    Success = false,
                    Message = "Institution not found or inactive"
                };
            }

            // Check if record ID already exists
            var existing = await _dataRepository.FindAsync(d => d.RecordId == dto.RecordId);
            if (existing.Any())
            {
                return new UploadDataResponseDto
                {
                    Success = false,
                    Message = "Record ID already exists"
                };
            }

            var record = new DataRecord
            {
                RecordId = dto.RecordId,
                DataHash = dto.DataHash,
                OwnerId = owner.Id,
                FileName = dto.FileName,
                FileType = dto.FileType,
                FileSize = dto.FileSize,
                IpfsHash = dto.IpfsHash,
                EncryptionAlgorithm = dto.EncryptionAlgorithm,
                Category = dto.Category,
                Description = dto.Description,
                MetadataUri = dto.MetadataUri,
                EncryptedSymmetricKey = dto.EncryptedSymmetricKey,
                UploadedAt = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _dataRepository.AddAsync(record);

            // Upload to blockchain
            string txHash = "";
            try
            {
                txHash = await _blockchainService.UploadDataToBlockchainAsync(
                    dto.RecordId,
                    dto.DataHash,
                    dto.FileName,
                    dto.FileType,
                    dto.FileSize,
                    dto.IpfsHash,
                    dto.EncryptionAlgorithm,
                    dto.Category,
                    dto.MetadataUri ?? "",
                    "" // Private key from secure source
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to upload to blockchain");
            }

            // Log upload
            await _auditService.LogDataUploadAsync(
                owner.Id,
                dto.RecordId,
                dto.DataHash,
                dto.FileName,
                "");

            return new UploadDataResponseDto
            {
                RecordId = created.RecordId,
                DataHash = created.DataHash,
                IpfsHash = created.IpfsHash,
                UploadedAt = created.UploadedAt,
                TransactionHash = txHash,
                Success = true,
                Message = "Data uploaded successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload data");
            return new UploadDataResponseDto
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}"
            };
        }
    }

    public async Task<VerifyDataResponseDto> VerifyDataAsync(VerifyDataDto dto, string verifierWalletAddress)
    {
        try
        {
            // Verify institution exists
            var verifiers = await _institutionRepository.FindAsync(i => i.WalletAddress == verifierWalletAddress);
            var verifier = verifiers.FirstOrDefault();
            
            if (verifier == null || !verifier.IsActive)
            {
                return new VerifyDataResponseDto
                {
                    Success = false,
                    Message = "Verifier institution not found or inactive"
                };
            }

            // Get record from database
            var records = await _dataRepository.FindAsync(d => d.RecordId == dto.RecordId);
            var record = records.FirstOrDefault();
            
            if (record == null)
            {
                return new VerifyDataResponseDto
                {
                    RecordId = dto.RecordId,
                    IsValid = false,
                    Message = "Record not found",
                    ProvidedHash = dto.ProvidedHash
                };
            }

            // Log verification request
            await _auditService.LogVerificationRequestAsync(
                verifier.Id,
                dto.RecordId,
                dto.ProvidedHash,
                "");

            // Compare hashes
            bool isValid = string.Equals(record.DataHash, dto.ProvidedHash, StringComparison.OrdinalIgnoreCase);

            // Verify on blockchain
            try
            {
                var blockchainVerified = await _blockchainService.VerifyDataOnBlockchainAsync(
                    dto.RecordId,
                    dto.ProvidedHash);
                
                isValid = isValid && blockchainVerified;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to verify on blockchain");
            }

            // Log verification completion
            await _auditService.LogVerificationCompletionAsync(
                verifier.Id,
                dto.RecordId,
                dto.ProvidedHash,
                isValid,
                "");

            return new VerifyDataResponseDto
            {
                RecordId = dto.RecordId,
                IsValid = isValid,
                OnChainHash = record.DataHash,
                ProvidedHash = dto.ProvidedHash,
                VerifiedAt = DateTime.UtcNow,
                Message = isValid ? "Data verified successfully" : "Verification failed - hash mismatch",
                RecordDetails = MapToDetailsDto(record)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify data");
            return new VerifyDataResponseDto
            {
                RecordId = dto.RecordId,
                IsValid = false,
                Message = $"Verification error: {ex.Message}",
                ProvidedHash = dto.ProvidedHash
            };
        }
    }

    public async Task<DataRecordDetailsDto?> GetDataRecordAsync(string recordId)
    {
        var records = await _dataRepository.FindAsync(d => d.RecordId == recordId);
        var record = records.FirstOrDefault();
        return record != null ? MapToDetailsDto(record) : null;
    }

    public async Task<IEnumerable<DataRecordDetailsDto>> GetInstitutionDataRecordsAsync(string walletAddress)
    {
        var owners = await _institutionRepository.FindAsync(i => i.WalletAddress == walletAddress);
        var owner = owners.FirstOrDefault();
        
        if (owner == null) return Enumerable.Empty<DataRecordDetailsDto>();

        var records = await _dataRepository.FindAsync(d => d.OwnerId == owner.Id);
        return records.Select(MapToDetailsDto);
    }

    public async Task<bool> DeactivateRecordAsync(string recordId, string ownerWalletAddress)
    {
        var owners = await _institutionRepository.FindAsync(i => i.WalletAddress == ownerWalletAddress);
        var owner = owners.FirstOrDefault();
        
        if (owner == null) return false;

        var records = await _dataRepository.FindAsync(d => d.RecordId == recordId && d.OwnerId == owner.Id);
        var record = records.FirstOrDefault();
        
        if (record == null) return false;

        record.IsActive = false;
        await _dataRepository.UpdateAsync(record);
        return true;
    }

    public async Task<bool> ReactivateRecordAsync(string recordId, string ownerWalletAddress)
    {
        var owners = await _institutionRepository.FindAsync(i => i.WalletAddress == ownerWalletAddress);
        var owner = owners.FirstOrDefault();
        
        if (owner == null) return false;

        var records = await _dataRepository.FindAsync(d => d.RecordId == recordId && d.OwnerId == owner.Id);
        var record = records.FirstOrDefault();
        
        if (record == null) return false;

        record.IsActive = true;
        await _dataRepository.UpdateAsync(record);
        return true;
    }

    public async Task<IEnumerable<DataRecordDetailsDto>> GetRecordsByCategoryAsync(string category)
    {
        var records = await _dataRepository.FindAsync(d => d.Category == category && d.IsActive);
        return records.Select(MapToDetailsDto);
    }

    public async Task<IEnumerable<DataRecordDetailsDto>> SearchRecordsAsync(string searchTerm)
    {
        var records = await _dataRepository.FindAsync(d => 
            (d.FileName.Contains(searchTerm) || 
             d.Category.Contains(searchTerm) ||
             (d.Description != null && d.Description.Contains(searchTerm))) &&
            d.IsActive);
        return records.Select(MapToDetailsDto);
    }

    private DataRecordDetailsDto MapToDetailsDto(DataRecord record)
    {
        return new DataRecordDetailsDto
        {
            RecordId = record.RecordId,
            DataHash = record.DataHash,
            OwnerWalletAddress = record.Owner?.WalletAddress ?? "",
            OwnerName = record.Owner?.Name ?? "",
            FileName = record.FileName,
            FileType = record.FileType,
            FileSize = record.FileSize,
            IpfsHash = record.IpfsHash,
            EncryptionAlgorithm = record.EncryptionAlgorithm,
            UploadedAt = record.UploadedAt,
            IsActive = record.IsActive,
            Category = record.Category,
            Description = record.Description
        };
    }
}

