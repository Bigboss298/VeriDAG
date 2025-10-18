# 💻 DataTrust Nexus: BlockDAG Secure Data Exchange Platform

A full-stack decentralized application for secure, NIST/ISO-compliant data transfer and verification built on the BlockDAG chain.

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-61DAFB)](https://reactjs.org/)
[![Solidity](https://img.shields.io/badge/Solidity-0.8.20-363636)](https://soliditylang.org/)

## 🧠 Core Concept

DataTrust Nexus is a **cross-sector secure data exchange network** that enables organizations (universities, banks, hospitals, government agencies, etc.) to **upload, share, and verify data** safely and transparently using blockchain technology.

### Key Features

- 🔐 **End-to-End Encryption** - AES-256-GCM encryption with RSA-OAEP key exchange
- ⛓️ **Blockchain-Based** - Immutable data records on BlockDAG chain
- 🔑 **Wallet Authentication** - Secure wallet-based access control
- 📊 **Full Audit Trail** - Complete compliance logging
- ✅ **Data Verification** - SHA-256 hash-based integrity checking
- 🏢 **Multi-Organization** - Designed for institutional use

## 🔐 Compliance & Security

All logic complies with:

- **ISO 27001** — Secure data management
- **NIST SP 800-53** — Encryption, access control, and audit standards
- **GDPR/NDPR** — Data privacy regulations

### Security Implementation

- **AES-256-GCM** for file encryption at rest
- **RSA-OAEP** for key encryption and exchange
- **SHA-256** for cryptographic hashing
- **On-chain immutable audit logs**
- **Time-bound access permissions**

## 🏗️ Architecture Overview

### 3-Layer Architecture

```
┌─────────────────────────────────────────┐
│         Frontend (React + TS)           │
│  ┌──────────────────────────────────┐  │
│  │  Wallet Connect (MetaMask)       │  │
│  │  Zustand State Management        │  │
│  │  TailwindCSS UI                  │  │
│  └──────────────────────────────────┘  │
└─────────────────┬───────────────────────┘
                  │ HTTPS/REST API
┌─────────────────▼───────────────────────┐
│   Backend (ASP.NET Core 8 N-Tier)      │
│  ┌──────────────────────────────────┐  │
│  │  Controllers (API Layer)         │  │
│  │  Services (Business Logic)       │  │
│  │  Repositories (Data Access)      │  │
│  │  EF Core + PostgreSQL            │  │
│  └──────────────────────────────────┘  │
└─────────────────┬───────────────────────┘
                  │ Web3/RPC
┌─────────────────▼───────────────────────┐
│    Smart Contracts (Solidity 0.8.20)   │
│  ┌──────────────────────────────────┐  │
│  │  InstitutionRegistry             │  │
│  │  DataVaultContract               │  │
│  │  AccessControlContract           │  │
│  │  AuditTrailContract              │  │
│  └──────────────────────────────────┘  │
│         BlockDAG Blockchain             │
└─────────────────────────────────────────┘
```

## 📁 Project Structure

```
VeriDAG/
├── Smart-Contract/          # Solidity smart contracts
│   ├── InstitutionRegistry.sol
│   ├── DataVaultContract.sol
│   ├── AccessControlContract.sol
│   ├── AuditTrailContract.sol
│   └── README.md
├── Backend/                 # ASP.NET Core 8 API
│   ├── Models/             # Entity models and DTOs
│   ├── Interfaces/         # Service contracts
│   ├── Implementations/    # Business logic
│   ├── Controllers/        # API endpoints
│   ├── Program.cs
│   └── README.md
├── frontend/               # React + TypeScript UI
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── stores/
│   │   └── App.tsx
│   ├── package.json
│   └── README.md
└── README.md              # This file
```

## 🚀 Quick Start

### Prerequisites

- **Node.js** 18+ and npm
- **.NET 8 SDK**
- **PostgreSQL** 14+
- **MetaMask** browser extension
- **Solidity** development environment (Hardhat or Foundry)
- **(Optional)** IPFS node

### 1. Clone the Repository

```bash
git clone <repository-url>
cd VeriDAG
```

### 2. Set Up Smart Contracts

```bash
cd Smart-Contract
npm install --save-dev hardhat @nomicfoundation/hardhat-toolbox

# Compile contracts
npx hardhat compile

# Deploy to local network
npx hardhat run scripts/deploy.js --network localhost

# Note the deployed contract addresses
```

See [Smart-Contract/README.md](Smart-Contract/README.md) for detailed instructions.

### 3. Set Up Backend

```bash
cd Backend

# Restore packages
dotnet restore

# Update database connection in appsettings.Development.json
# Update blockchain contract addresses

# Create database
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run API
dotnet run
```

The API will be available at `http://localhost:5000`

See [Backend/README.md](Backend/README.md) for detailed instructions.

### 4. Set Up Frontend

```bash
cd frontend

# Install dependencies
npm install

# Create .env file (copy from .env.example)
# Update API URL and contract addresses

# Run development server
npm run dev
```

The frontend will be available at `http://localhost:3001`

See [frontend/README.md](frontend/README.md) for detailed instructions.

## 🔄 Integration Flow

### 1. Institution Registration

```
User → Connect Wallet → Register Institution → Blockchain Record
```

### 2. Data Upload

```
Select File → Encrypt (AES-256) → Upload to IPFS → Store Hash on Blockchain → Save to Database
```

### 3. Grant Access

```
Select Record → Enter Grantee Wallet → Set Permissions → Record on Blockchain → Update Database
```

### 4. Verify Data

```
Enter Record ID → Provide Hash → Compare with On-Chain Hash → Display Result
```

### 5. Audit Trail

```
All Actions → Logged to Database → Also Recorded on Blockchain → Queryable & Exportable
```

## 🧩 Tech Stack Summary

| Layer | Technology |
|-------|------------|
| **Frontend** | React 18, TypeScript, Zustand, TailwindCSS, Axios |
| **Backend** | ASP.NET Core 8, EF Core, PostgreSQL |
| **Blockchain** | Solidity 0.8.20, Hardhat, BlockDAG Chain |
| **Encryption** | AES-256-GCM, RSA-OAEP, SHA-256 |
| **Storage** | PostgreSQL, IPFS (optional) |
| **Auth** | Wallet-based (MetaMask) |

## 📊 Features Overview

### Dashboard
- Real-time statistics
- Activity charts (Recharts)
- Recent audit logs
- Data record overview

### Data Upload
- File selection
- Automatic encryption
- IPFS upload
- Blockchain recording
- Category tagging

### Access Control
- Grant permissions
- Revoke access
- Time-bound permissions
- Permission types (read, verify, full)
- View granted/received permissions

### Data Verification
- Hash-based verification
- On-chain comparison
- Detailed record information
- Verification history

### Audit Logs
- Complete audit trail
- Filter by action type, date
- Export to JSON
- NIST SP 800-53 compliant

## 🛡️ Security Best Practices

1. **Never expose private keys** in code or config files
2. **Use environment variables** for sensitive data
3. **Enable HTTPS** in production
4. **Implement rate limiting** on API endpoints
5. **Validate all inputs** on both frontend and backend
6. **Use prepared statements** to prevent SQL injection
7. **Keep dependencies updated** regularly
8. **Implement proper error handling** without exposing sensitive info
9. **Use strong password policies** for database access
10. **Regular security audits** of smart contracts

## 📈 Roadmap

- [x] Core platform implementation
- [x] Smart contract deployment
- [x] Basic UI with wallet integration
- [ ] Multi-chain support
- [ ] Mobile application
- [ ] Advanced analytics dashboard
- [ ] AI-powered data classification
- [ ] Integration with major cloud providers
- [ ] Advanced permission management
- [ ] Real-time notifications

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines

- Follow existing code style
- Write meaningful commit messages
- Add tests for new features
- Update documentation
- Ensure all tests pass

## 📝 API Documentation

Once the backend is running, access Swagger documentation at:
- Development: `http://localhost:5000`
- Production: `https://your-domain.com/swagger`

## 🧪 Testing

### Backend Tests

```bash
cd Backend
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm test
```

### Smart Contract Tests

```bash
cd Smart-Contract
npx hardhat test
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Team

- **Backend Development** - ASP.NET Core 8 N-Tier Architecture
- **Frontend Development** - React + TypeScript + Zustand
- **Smart Contract Development** - Solidity on BlockDAG
- **Security & Compliance** - ISO 27001, NIST SP 800-53

## 📞 Support

For support, email support@datatrustnexus.com or open an issue on GitHub.

## 🙏 Acknowledgments

- BlockDAG Chain team for blockchain infrastructure
- OpenZeppelin for smart contract libraries
- .NET team for ASP.NET Core framework
- React team for the frontend framework
- TailwindCSS for the styling framework

## ⚠️ Disclaimer

This software is provided "as is" without warranty. Use at your own risk. Always perform thorough testing before deploying to production environments.

---

**Built with ❤️ for secure data exchange**

