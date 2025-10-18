import { BigInt } from "@graphprotocol/graph-ts"
import {
  AuditLogCreated,
  InstitutionRegisteredLog,
  DataUploadedLog,
  AccessGrantedLog,
  AccessRevokedLog,
  VerificationRequestedLog,
  VerificationCompletedLog,
  DataAccessedLog
} from "../generated/AuditTrailContract/AuditTrailContract"
import { AuditLog, GlobalStatistics, Institution } from "../generated/schema"

export function handleAuditLogCreated(event: AuditLogCreated): void {
  let log = new AuditLog(event.params.logId.toString())
  
  log.logId = event.params.logId
  log.actionType = getActionType(event.params.actionType)
  log.actor = event.params.actor.toHexString()
  log.targetAddress = null
  log.record = event.params.recordId.length > 0 ? event.params.recordId : null
  log.actionDetails = ""
  log.dataHash = null
  log.success = event.params.success
  log.timestamp = event.params.timestamp
  log.ipAddress = ""
  log.userAgent = ""
  log.blockNumber = event.block.number
  log.transactionHash = event.transaction.hash
  
  log.save()
  
  // Update institution statistics
  let institution = Institution.load(event.params.actor.toHexString())
  if (institution) {
    institution.totalAuditLogs = institution.totalAuditLogs.plus(BigInt.fromI32(1))
    institution.save()
  }
  
  // Update global statistics
  updateGlobalStatistics(event.block.timestamp)
}

export function handleInstitutionRegisteredLog(event: InstitutionRegisteredLog): void {
  // Already handled in institution-registry.ts
}

export function handleDataUploadedLog(event: DataUploadedLog): void {
  // Already handled in data-vault.ts
}

export function handleAccessGrantedLog(event: AccessGrantedLog): void {
  // Already handled in access-control.ts
}

export function handleAccessRevokedLog(event: AccessRevokedLog): void {
  // Already handled in access-control.ts
}

export function handleVerificationRequestedLog(event: VerificationRequestedLog): void {
  let log = new AuditLog(
    event.transaction.hash.toHex() + "-" + event.logIndex.toString()
  )
  
  log.logId = BigInt.fromI32(0)
  log.actionType = "VERIFICATION_REQUESTED"
  log.actor = event.params.verifier.toHexString()
  log.targetAddress = null
  log.record = event.params.recordId
  log.actionDetails = "Verification requested"
  log.dataHash = event.params.hash
  log.success = true
  log.timestamp = event.params.timestamp
  log.ipAddress = ""
  log.userAgent = ""
  log.blockNumber = event.block.number
  log.transactionHash = event.transaction.hash
  
  log.save()
  
  // Update global statistics
  let stats = GlobalStatistics.load("1")
  if (stats) {
    stats.totalVerifications = stats.totalVerifications.plus(BigInt.fromI32(1))
    stats.lastUpdated = event.block.timestamp
    stats.save()
  }
}

export function handleVerificationCompletedLog(event: VerificationCompletedLog): void {
  let log = new AuditLog(
    event.transaction.hash.toHex() + "-" + event.logIndex.toString()
  )
  
  log.logId = BigInt.fromI32(0)
  log.actionType = "VERIFICATION_COMPLETED"
  log.actor = event.params.verifier.toHexString()
  log.targetAddress = null
  log.record = event.params.recordId
  log.actionDetails = event.params.success ? "Verification successful" : "Verification failed"
  log.dataHash = event.params.hash
  log.success = event.params.success
  log.timestamp = event.params.timestamp
  log.ipAddress = ""
  log.userAgent = ""
  log.blockNumber = event.block.number
  log.transactionHash = event.transaction.hash
  
  log.save()
  
  // Update global statistics
  let stats = GlobalStatistics.load("1")
  if (stats) {
    if (event.params.success) {
      stats.successfulVerifications = stats.successfulVerifications.plus(BigInt.fromI32(1))
    } else {
      stats.failedVerifications = stats.failedVerifications.plus(BigInt.fromI32(1))
    }
    stats.lastUpdated = event.block.timestamp
    stats.save()
  }
}

export function handleDataAccessedLog(event: DataAccessedLog): void {
  let log = new AuditLog(
    event.transaction.hash.toHex() + "-" + event.logIndex.toString()
  )
  
  log.logId = BigInt.fromI32(0)
  log.actionType = "DATA_ACCESSED"
  log.actor = event.params.accessor.toHexString()
  log.targetAddress = null
  log.record = event.params.recordId
  log.actionDetails = "Data accessed"
  log.dataHash = null
  log.success = true
  log.timestamp = event.params.timestamp
  log.ipAddress = ""
  log.userAgent = ""
  log.blockNumber = event.block.number
  log.transactionHash = event.transaction.hash
  
  log.save()
}

function getActionType(actionTypeInt: i32): string {
  if (actionTypeInt == 0) return "INSTITUTION_REGISTERED"
  if (actionTypeInt == 1) return "DATA_UPLOADED"
  if (actionTypeInt == 2) return "ACCESS_GRANTED"
  if (actionTypeInt == 3) return "ACCESS_REVOKED"
  if (actionTypeInt == 4) return "VERIFICATION_REQUESTED"
  if (actionTypeInt == 5) return "VERIFICATION_COMPLETED"
  if (actionTypeInt == 6) return "DATA_ACCESSED"
  if (actionTypeInt == 7) return "DATA_DOWNLOADED"
  if (actionTypeInt == 8) return "PERMISSION_UPDATED"
  if (actionTypeInt == 9) return "RECORD_DEACTIVATED"
  if (actionTypeInt == 10) return "RECORD_REACTIVATED"
  return "UNKNOWN"
}

function updateGlobalStatistics(timestamp: BigInt): void {
  let stats = GlobalStatistics.load("1")
  if (!stats) {
    stats = new GlobalStatistics("1")
    stats.totalInstitutions = BigInt.fromI32(0)
    stats.totalDataRecords = BigInt.fromI32(0)
    stats.totalAccessPermissions = BigInt.fromI32(0)
    stats.totalAuditLogs = BigInt.fromI32(0)
    stats.totalVerifications = BigInt.fromI32(0)
    stats.successfulVerifications = BigInt.fromI32(0)
    stats.failedVerifications = BigInt.fromI32(0)
  }
  
  stats.totalAuditLogs = stats.totalAuditLogs.plus(BigInt.fromI32(1))
  stats.lastUpdated = timestamp
  stats.save()
}

