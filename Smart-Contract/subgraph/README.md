# DataTrust Nexus Subgraph

The Graph Protocol subgraph for indexing DataTrust Nexus smart contracts.

## 📋 Overview

This subgraph indexes all events from the DataTrust Nexus smart contracts, providing fast GraphQL queries for:
- Institutions
- Data records
- Access permissions
- Audit logs
- Statistics

## 🚀 Quick Start

### Prerequisites

- Node.js 18+
- Graph CLI: `npm install -g @graphprotocol/graph-cli`
- Deployed smart contracts

### 1. Install Dependencies

```bash
npm install
```

### 2. Update Contract Addresses

Edit `subgraph.yaml` and update:
- Contract addresses (all 4 contracts)
- Network name (mainnet, polygon, arbitrum, etc.)
- Start block numbers

```yaml
source:
  address: "0xYourContractAddress"  # Update this
  startBlock: 12345678               # Update this
```

### 3. Generate Code

```bash
npm run codegen
```

This generates TypeScript types from your GraphQL schema and smart contract ABIs.

### 4. Build Subgraph

```bash
npm run build
```

### 5. Deploy

#### Option A: The Graph Studio (Recommended)

```bash
# Authenticate
graph auth --studio YOUR_DEPLOY_KEY

# Deploy
npm run deploy
```

#### Option B: Local Graph Node

```bash
# Start Graph Node (separate terminal)
docker-compose up

# Create subgraph
npm run create-local

# Deploy
npm run deploy-local
```

## 📊 GraphQL Schema

### Entities

#### Institution
```graphql
type Institution @entity {
  id: ID!
  name: String!
  institutionType: String!
  walletAddress: Bytes!
  registeredAt: BigInt!
  isActive: Boolean!
  totalDataRecords: BigInt!
  totalAccessGrants: BigInt!
  dataRecords: [DataRecord!]!
  grantedPermissions: [AccessPermission!]!
}
```

#### DataRecord
```graphql
type DataRecord @entity {
  id: ID!
  recordId: String!
  dataHash: Bytes!
  owner: Institution!
  fileName: String!
  ipfsHash: String!
  uploadedAt: BigInt!
  isActive: Boolean!
  category: String!
  accessPermissions: [AccessPermission!]!
}
```

#### AccessPermission
```graphql
type AccessPermission @entity {
  id: ID!
  record: DataRecord!
  granter: Institution!
  granteeAddress: Bytes!
  grantedAt: BigInt!
  expiresAt: BigInt
  isActive: Boolean!
  permissionType: String!
  hasValidAccess: Boolean!
}
```

#### AuditLog
```graphql
type AuditLog @entity {
  id: ID!
  actionType: ActionType!
  actor: Institution!
  record: DataRecord
  success: Boolean!
  timestamp: BigInt!
}
```

#### GlobalStatistics
```graphql
type GlobalStatistics @entity {
  id: ID!
  totalInstitutions: BigInt!
  totalDataRecords: BigInt!
  totalAccessPermissions: BigInt!
  totalVerifications: BigInt!
  successfulVerifications: BigInt!
  failedVerifications: BigInt!
}
```

## 🔍 Example Queries

### Get All Institutions

```graphql
query {
  institutions(first: 10, orderBy: registeredAt, orderDirection: desc) {
    id
    name
    institutionType
    walletAddress
    totalDataRecords
    totalAccessGrants
    isActive
  }
}
```

### Get Institution with Data Records

```graphql
query {
  institution(id: "0x123...") {
    id
    name
    institutionType
    dataRecords(first: 10) {
      recordId
      fileName
      dataHash
      uploadedAt
      category
    }
  }
}
```

### Get Data Record with Permissions

```graphql
query {
  dataRecord(id: "REC-123") {
    recordId
    fileName
    dataHash
    owner {
      name
      walletAddress
    }
    accessPermissions(where: { isActive: true }) {
      granteeAddress
      permissionType
      grantedAt
      expiresAt
      hasValidAccess
    }
  }
}
```

### Get Recent Audit Logs

```graphql
query {
  auditLogs(
    first: 50
    orderBy: timestamp
    orderDirection: desc
    where: { success: true }
  ) {
    id
    actionType
    actor {
      name
    }
    record {
      recordId
    }
    timestamp
    success
  }
}
```

### Get Global Statistics

```graphql
query {
  globalStatistics(id: "1") {
    totalInstitutions
    totalDataRecords
    totalAccessPermissions
    totalVerifications
    successfulVerifications
    failedVerifications
  }
}
```

### Search Data Records

```graphql
query {
  dataRecords(
    where: { 
      category: "Academic"
      isActive: true
    }
    first: 20
    orderBy: uploadedAt
    orderDirection: desc
  ) {
    recordId
    fileName
    dataHash
    category
    owner {
      name
    }
  }
}
```

### Get Active Permissions for an Address

```graphql
query {
  accessPermissions(
    where: {
      granteeAddress: "0x123..."
      hasValidAccess: true
    }
  ) {
    id
    record {
      recordId
      fileName
    }
    permissionType
    grantedAt
    expiresAt
  }
}
```

## 📂 Project Structure

```
subgraph/
├── src/
│   ├── institution-registry.ts  # Institution event handlers
│   ├── data-vault.ts           # Data record event handlers
│   ├── access-control.ts       # Access permission handlers
│   └── audit-trail.ts          # Audit log handlers
├── schema.graphql              # GraphQL schema
├── subgraph.yaml              # Subgraph configuration
├── package.json
└── README.md
```

## 🔄 Local Development

### Start Local Graph Node

```bash
# Clone Graph Node
git clone https://github.com/graphprotocol/graph-node
cd graph-node/docker

# Start services
docker-compose up
```

This starts:
- Graph Node (GraphQL server)
- PostgreSQL (data storage)
- IPFS (metadata storage)

### Deploy to Local Node

```bash
# In your subgraph directory
npm run create-local
npm run deploy-local
```

Access GraphiQL at `http://localhost:8000/subgraphs/name/datatrust-nexus`

## 🌐 Deploying to The Graph Studio

1. **Create Subgraph**: https://thegraph.com/studio
2. **Get Deploy Key**: Copy from Studio
3. **Authenticate**:
```bash
graph auth --studio <DEPLOY_KEY>
```
4. **Deploy**:
```bash
npm run deploy
```

## 📝 Updating After Contract Changes

If you modify smart contracts:

1. Update contract addresses in `subgraph.yaml`
2. Copy new ABI files to project
3. Run `npm run codegen`
4. Update mapping files if events changed
5. Run `npm run build`
6. Deploy: `npm run deploy`

## 🧪 Testing

```bash
npm run test
```

Create tests in `tests/` directory.

## 🐛 Troubleshooting

### Subgraph Failed to Deploy

- Check contract addresses are correct
- Verify start block is before first transaction
- Ensure ABIs are up to date

### Missing Data

- Check if events are being emitted
- Verify network is correct
- Check start block number

### Slow Queries

- Add indexes in schema (automatically done)
- Use pagination (first, skip)
- Filter data early in queries

## 📊 Monitoring

Once deployed, monitor your subgraph:
- **Graph Studio**: https://thegraph.com/studio
- **GraphiQL**: Query interface
- **Logs**: Check for indexing errors

## 🔗 Resources

- [The Graph Docs](https://thegraph.com/docs)
- [AssemblyScript Book](https://www.assemblyscript.org/)
- [GraphQL Documentation](https://graphql.org/)

## 📄 License

MIT

