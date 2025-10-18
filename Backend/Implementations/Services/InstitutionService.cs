using DataTrustNexus.Api.Interfaces;
using DataTrustNexus.Api.Models;
using DataTrustNexus.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DataTrustNexus.Api.Implementations.Services;

/// <summary>
/// Service for institution operations
/// </summary>
public class InstitutionService : IInstitutionService
{
    private readonly IRepository<Institution> _repository;
    private readonly IBlockchainService _blockchainService;
    private readonly IAuditService _auditService;
    private readonly ILogger<InstitutionService> _logger;

    public InstitutionService(
        IRepository<Institution> repository,
        IBlockchainService blockchainService,
        IAuditService auditService,
        ILogger<InstitutionService> logger)
    {
        _repository = repository;
        _blockchainService = blockchainService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<InstitutionResponseDto> RegisterInstitutionAsync(RegisterInstitutionDto dto)
    {
        try
        {
            // Check if institution already exists
            var existing = await _repository.FindAsync(i => i.WalletAddress == dto.WalletAddress);
            if (existing.Any())
            {
                return new InstitutionResponseDto
                {
                    Success = false,
                    Message = "Institution with this wallet address already exists"
                };
            }

            var institution = new Institution
            {
                Name = dto.Name,
                InstitutionType = dto.InstitutionType,
                RegistrationNumber = dto.RegistrationNumber,
                WalletAddress = dto.WalletAddress,
                MetadataUri = dto.MetadataUri,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Country = dto.Country,
                RegisteredAt = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _repository.AddAsync(institution);

            // Register on blockchain (mock - in production, need private key from secure source)
            string txHash = "";
            try
            {
                txHash = await _blockchainService.RegisterInstitutionAsync(
                    dto.Name,
                    dto.InstitutionType,
                    dto.RegistrationNumber,
                    dto.MetadataUri ?? "",
                    "" // Private key would come from secure key management
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to register on blockchain, but database entry created");
            }

            // Create audit log
            await _auditService.LogInstitutionRegistrationAsync(
                created.Id,
                created.WalletAddress,
                created.Name,
                "");

            return new InstitutionResponseDto
            {
                Success = true,
                Message = "Institution registered successfully",
                Institution = MapToDto(created),
                TransactionHash = txHash
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register institution");
            return new InstitutionResponseDto
            {
                Success = false,
                Message = $"Registration failed: {ex.Message}"
            };
        }
    }

    public async Task<InstitutionDto?> GetInstitutionByWalletAsync(string walletAddress)
    {
        var institutions = await _repository.FindAsync(i => i.WalletAddress == walletAddress);
        var institution = institutions.FirstOrDefault();
        return institution != null ? MapToDto(institution) : null;
    }

    public async Task<InstitutionDto?> GetInstitutionByIdAsync(string id)
    {
        var institution = await _repository.GetByIdAsync(id);
        return institution != null ? MapToDto(institution) : null;
    }

    public async Task<InstitutionResponseDto> UpdateInstitutionAsync(string walletAddress, UpdateInstitutionDto dto)
    {
        try
        {
            var institutions = await _repository.FindAsync(i => i.WalletAddress == walletAddress);
            var institution = institutions.FirstOrDefault();
            
            if (institution == null)
            {
                return new InstitutionResponseDto
                {
                    Success = false,
                    Message = "Institution not found"
                };
            }

            institution.Name = dto.Name;
            institution.MetadataUri = dto.MetadataUri;
            institution.ContactEmail = dto.ContactEmail;
            institution.ContactPhone = dto.ContactPhone;

            var updated = await _repository.UpdateAsync(institution);

            return new InstitutionResponseDto
            {
                Success = true,
                Message = "Institution updated successfully",
                Institution = MapToDto(updated)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update institution");
            return new InstitutionResponseDto
            {
                Success = false,
                Message = $"Update failed: {ex.Message}"
            };
        }
    }

    public async Task<bool> DeactivateInstitutionAsync(string walletAddress)
    {
        var institutions = await _repository.FindAsync(i => i.WalletAddress == walletAddress);
        var institution = institutions.FirstOrDefault();
        
        if (institution == null) return false;

        institution.IsActive = false;
        await _repository.UpdateAsync(institution);
        return true;
    }

    public async Task<bool> ReactivateInstitutionAsync(string walletAddress)
    {
        var institutions = await _repository.FindAsync(i => i.WalletAddress == walletAddress);
        var institution = institutions.FirstOrDefault();
        
        if (institution == null) return false;

        institution.IsActive = true;
        await _repository.UpdateAsync(institution);
        return true;
    }

    public async Task<bool> VerifyInstitutionAsync(string walletAddress)
    {
        var institutions = await _repository.FindAsync(i => 
            i.WalletAddress == walletAddress && i.IsActive);
        return institutions.Any();
    }

    public async Task<IEnumerable<InstitutionDto>> GetAllInstitutionsAsync()
    {
        var institutions = await _repository.GetAllAsync();
        return institutions.Select(MapToDto);
    }

    public async Task<IEnumerable<InstitutionDto>> GetInstitutionsByTypeAsync(string institutionType)
    {
        var institutions = await _repository.FindAsync(i => i.InstitutionType == institutionType);
        return institutions.Select(MapToDto);
    }

    public async Task<IEnumerable<InstitutionDto>> SearchInstitutionsAsync(string searchTerm)
    {
        var institutions = await _repository.FindAsync(i => 
            i.Name.Contains(searchTerm) || 
            i.RegistrationNumber.Contains(searchTerm) ||
            i.InstitutionType.Contains(searchTerm));
        return institutions.Select(MapToDto);
    }

    private InstitutionDto MapToDto(Institution institution)
    {
        return new InstitutionDto
        {
            Id = institution.Id,
            Name = institution.Name,
            InstitutionType = institution.InstitutionType,
            RegistrationNumber = institution.RegistrationNumber,
            WalletAddress = institution.WalletAddress,
            RegisteredAt = institution.RegisteredAt,
            IsActive = institution.IsActive,
            MetadataUri = institution.MetadataUri,
            ContactEmail = institution.ContactEmail,
            ContactPhone = institution.ContactPhone,
            Country = institution.Country,
            TotalDataRecords = institution.DataRecords?.Count ?? 0,
            TotalAccessGrants = institution.GrantedAccess?.Count ?? 0
        };
    }
}

