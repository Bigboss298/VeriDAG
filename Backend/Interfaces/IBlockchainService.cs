namespace DataTrustNexus.Api.Interfaces;

/// <summary>
/// Service interface for blockchain interactions
/// </summary>
public interface IBlockchainService
{
    /// <summary>
    /// Register institution on blockchain
    /// </summary>
    Task<string> RegisterInstitutionAsync(string name, string institutionType, string registrationNumber, string metadataUri, string walletPrivateKey);
    
    /// <summary>
    /// Upload data record to blockchain
    /// </summary>
    Task<string> UploadDataToBlockchainAsync(
        string recordId,
        string dataHash,
        string fileName,
        string fileType,
        long fileSize,
        string ipfsHash,
        string encryptionAlgorithm,
        string category,
        string metadataUri,
        string walletPrivateKey);
    
    /// <summary>
    /// Grant access on blockchain
    /// </summary>
    Task<string> GrantAccessOnBlockchainAsync(
        string recordId,
        string granteeAddress,
        long expiresAt,
        string permissionType,
        string grantReason,
        string walletPrivateKey);
    
    /// <summary>
    /// Revoke access on blockchain
    /// </summary>
    Task<string> RevokeAccessOnBlockchainAsync(
        string recordId,
        string granteeAddress,
        string walletPrivateKey);
    
    /// <summary>
    /// Verify data on blockchain
    /// </summary>
    Task<bool> VerifyDataOnBlockchainAsync(string recordId, string providedHash);
    
    /// <summary>
    /// Check access on blockchain
    /// </summary>
    Task<bool> CheckAccessOnBlockchainAsync(string recordId, string walletAddress);
    
    /// <summary>
    /// Get data record from blockchain
    /// </summary>
    Task<BlockchainDataRecord?> GetDataRecordFromBlockchainAsync(string recordId);
    
    /// <summary>
    /// Log action to audit trail contract
    /// </summary>
    Task<string> LogToAuditTrailAsync(
        int actionType,
        string targetAddress,
        string recordId,
        string actionDetails,
        string dataHash,
        bool success,
        string ipAddress,
        string userAgent,
        string walletPrivateKey);
    
    /// <summary>
    /// Get institution from blockchain
    /// </summary>
    Task<BlockchainInstitution?> GetInstitutionFromBlockchainAsync(string walletAddress);
    
    /// <summary>
    /// Verify institution on blockchain
    /// </summary>
    Task<bool> VerifyInstitutionOnBlockchainAsync(string walletAddress);
}

/// <summary>
/// Blockchain data record model
/// </summary>
public class BlockchainDataRecord
{
    public string RecordId { get; set; } = string.Empty;
    public string DataHash { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string IpfsHash { get; set; } = string.Empty;
    public string EncryptionAlgorithm { get; set; } = string.Empty;
    public long UploadedAt { get; set; }
    public bool IsActive { get; set; }
    public string MetadataUri { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Blockchain institution model
/// </summary>
public class BlockchainInstitution
{
    public string Name { get; set; } = string.Empty;
    public string InstitutionType { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string WalletAddress { get; set; } = string.Empty;
    public long RegisteredAt { get; set; }
    public bool IsActive { get; set; }
    public string MetadataUri { get; set; } = string.Empty;
}

