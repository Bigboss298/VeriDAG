using System.Text;
using System.Text.Json;

namespace DataTrustNexus.Api.Services;

/// <summary>
/// Service for querying The Graph Protocol
/// </summary>
public class GraphQLService
{
    private readonly HttpClient _httpClient;
    private readonly string _subgraphUrl;
    private readonly ILogger<GraphQLService> _logger;

    public GraphQLService(HttpClient httpClient, IConfiguration configuration, ILogger<GraphQLService> logger)
    {
        _httpClient = httpClient;
        _subgraphUrl = configuration["TheGraph:SubgraphUrl"] ?? 
            "http://localhost:8000/subgraphs/name/datatrust-nexus";
        _logger = logger;
    }

    public async Task<T?> QueryAsync<T>(string query, object? variables = null)
    {
        try
        {
            var request = new
            {
                query,
                variables
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_subgraphUrl, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GraphQLResponse<T>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Errors != null && result.Errors.Any())
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Message));
                _logger.LogError("GraphQL query errors: {Errors}", errors);
                throw new Exception($"GraphQL errors: {errors}");
            }

            return result != null ? result.Data : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute GraphQL query");
            throw;
        }
    }

    private class GraphQLResponse<T>
    {
        public T? Data { get; set; }
        public List<GraphQLError>? Errors { get; set; }
    }

    private class GraphQLError
    {
        public string Message { get; set; } = string.Empty;
    }
}

/// <summary>
/// GraphQL query builder helper
/// </summary>
public static class GraphQLQueries
{
    // Institution Queries
    public const string GetInstitutionByWallet = @"
        query GetInstitution($wallet: String!) {
            institution(id: $wallet) {
                id
                name
                institutionType
                registrationNumber
                walletAddress
                registeredAt
                isActive
                metadataUri
                totalDataRecords
                totalAccessGrants
                totalAuditLogs
            }
        }";

    public const string GetAllInstitutions = @"
        query GetInstitutions($first: Int!, $skip: Int!) {
            institutions(first: $first, skip: $skip, orderBy: registeredAt, orderDirection: desc) {
                id
                name
                institutionType
                registrationNumber
                walletAddress
                registeredAt
                isActive
                totalDataRecords
                totalAccessGrants
            }
        }";

    // Data Record Queries
    public const string GetDataRecord = @"
        query GetDataRecord($recordId: String!) {
            dataRecord(id: $recordId) {
                id
                recordId
                dataHash
                fileName
                fileType
                fileSize
                ipfsHash
                encryptionAlgorithm
                uploadedAt
                isActive
                category
                metadataUri
                owner {
                    id
                    name
                    walletAddress
                }
            }
        }";

    public const string GetInstitutionRecords = @"
        query GetInstitutionRecords($ownerId: String!, $first: Int!, $skip: Int!) {
            dataRecords(
                where: { owner: $ownerId }
                first: $first
                skip: $skip
                orderBy: uploadedAt
                orderDirection: desc
            ) {
                id
                recordId
                dataHash
                fileName
                fileType
                fileSize
                ipfsHash
                encryptionAlgorithm
                uploadedAt
                isActive
                category
            }
        }";

    // Access Control Queries
    public const string CheckAccess = @"
        query CheckAccess($permissionId: String!) {
            accessPermission(id: $permissionId) {
                id
                isActive
                hasValidAccess
                permissionType
                grantedAt
                expiresAt
                isExpired
            }
        }";

    public const string GetGrantedPermissions = @"
        query GetGrantedPermissions($granterId: String!, $first: Int!) {
            accessPermissions(
                where: { granter: $granterId }
                first: $first
                orderBy: grantedAt
                orderDirection: desc
            ) {
                id
                granteeAddress
                permissionType
                grantedAt
                expiresAt
                isActive
                hasValidAccess
                record {
                    recordId
                    fileName
                }
            }
        }";

    public const string GetReceivedPermissions = @"
        query GetReceivedPermissions($granteeAddress: String!, $first: Int!) {
            accessPermissions(
                where: { granteeAddress: $granteeAddress }
                first: $first
                orderBy: grantedAt
                orderDirection: desc
            ) {
                id
                permissionType
                grantedAt
                expiresAt
                isActive
                hasValidAccess
                record {
                    recordId
                    fileName
                }
                granter {
                    name
                }
            }
        }";

    // Audit Log Queries
    public const string GetRecentLogs = @"
        query GetRecentLogs($first: Int!) {
            auditLogs(first: $first, orderBy: timestamp, orderDirection: desc) {
                id
                logId
                actionType
                success
                timestamp
                actionDetails
                dataHash
                actor {
                    id
                    name
                    walletAddress
                }
                record {
                    recordId
                }
                targetAddress
            }
        }";

    public const string GetLogsByActor = @"
        query GetLogsByActor($actorId: String!, $first: Int!) {
            auditLogs(
                where: { actor: $actorId }
                first: $first
                orderBy: timestamp
                orderDirection: desc
            ) {
                id
                logId
                actionType
                success
                timestamp
                actionDetails
                record {
                    recordId
                }
            }
        }";

    public const string GetLogsByRecord = @"
        query GetLogsByRecord($recordId: String!, $first: Int!) {
            auditLogs(
                where: { record: $recordId }
                first: $first
                orderBy: timestamp
                orderDirection: desc
            ) {
                id
                logId
                actionType
                success
                timestamp
                actionDetails
                actor {
                    name
                }
            }
        }";

    public const string GetGlobalStatistics = @"
        query GetGlobalStatistics {
            globalStatistics(id: ""1"") {
                totalInstitutions
                totalDataRecords
                totalAccessPermissions
                totalAuditLogs
                totalVerifications
                successfulVerifications
                failedVerifications
            }
        }";
}

