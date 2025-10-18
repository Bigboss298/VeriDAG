import { BigInt } from "@graphprotocol/graph-ts"
import {
  AccessGranted,
  AccessRevoked,
  AccessExpired,
  PermissionUpdated
} from "../generated/AccessControlContract/AccessControlContract"
import { AccessPermission, AccessEvent, DataRecord, Institution, GlobalStatistics } from "../generated/schema"

export function handleAccessGranted(event: AccessGranted): void {
  let permissionId = event.params.recordId + "-" + event.params.grantee.toHexString()
  let permission = new AccessPermission(permissionId)
  
  permission.record = event.params.recordId
  permission.granter = event.params.owner.toHexString()
  permission.granteeAddress = event.params.grantee
  permission.grantedAt = event.params.timestamp
  permission.expiresAt = event.params.expiresAt
  permission.isActive = true
  permission.permissionType = event.params.permissionType
  permission.grantReason = ""
  permission.revokedAt = BigInt.fromI32(0)
  permission.updatedAt = event.block.timestamp
  
  // Calculate computed fields
  permission.isExpired = event.params.expiresAt.gt(BigInt.fromI32(0)) && 
                          event.block.timestamp.gt(event.params.expiresAt)
  permission.hasValidAccess = permission.isActive && !permission.isExpired
  
  permission.save()
  
  // Update record statistics
  let record = DataRecord.load(event.params.recordId)
  if (record) {
    record.totalPermissions = record.totalPermissions.plus(BigInt.fromI32(1))
    record.save()
  }
  
  // Update institution statistics
  let institution = Institution.load(event.params.owner.toHexString())
  if (institution) {
    institution.totalAccessGrants = institution.totalAccessGrants.plus(BigInt.fromI32(1))
    institution.save()
  }
  
  // Create access event
  let accessEvent = new AccessEvent(
    event.transaction.hash.toHex() + "-" + event.logIndex.toString()
  )
  accessEvent.permission = permission.id
  accessEvent.eventType = "GRANTED"
  accessEvent.timestamp = event.block.timestamp
  accessEvent.blockNumber = event.block.number
  accessEvent.transactionHash = event.transaction.hash
  accessEvent.save()
  
  // Update global statistics
  updateGlobalStatistics(event.block.timestamp)
}

export function handleAccessRevoked(event: AccessRevoked): void {
  let permissionId = event.params.recordId + "-" + event.params.grantee.toHexString()
  let permission = AccessPermission.load(permissionId)
  
  if (permission) {
    permission.isActive = false
    permission.revokedAt = event.block.timestamp
    permission.hasValidAccess = false
    permission.updatedAt = event.block.timestamp
    permission.save()
    
    // Create event
    let accessEvent = new AccessEvent(
      event.transaction.hash.toHex() + "-" + event.logIndex.toString()
    )
    accessEvent.permission = permission.id
    accessEvent.eventType = "REVOKED"
    accessEvent.timestamp = event.block.timestamp
    accessEvent.blockNumber = event.block.number
    accessEvent.transactionHash = event.transaction.hash
    accessEvent.save()
  }
}

export function handleAccessExpired(event: AccessExpired): void {
  let permissionId = event.params.recordId + "-" + event.params.grantee.toHexString()
  let permission = AccessPermission.load(permissionId)
  
  if (permission) {
    permission.isActive = false
    permission.isExpired = true
    permission.hasValidAccess = false
    permission.updatedAt = event.block.timestamp
    permission.save()
    
    // Create event
    let accessEvent = new AccessEvent(
      event.transaction.hash.toHex() + "-" + event.logIndex.toString()
    )
    accessEvent.permission = permission.id
    accessEvent.eventType = "EXPIRED"
    accessEvent.timestamp = event.block.timestamp
    accessEvent.blockNumber = event.block.number
    accessEvent.transactionHash = event.transaction.hash
    accessEvent.save()
  }
}

export function handlePermissionUpdated(event: PermissionUpdated): void {
  let permissionId = event.params.recordId + "-" + event.params.grantee.toHexString()
  let permission = AccessPermission.load(permissionId)
  
  if (permission) {
    permission.updatedAt = event.block.timestamp
    permission.save()
    
    // Create event
    let accessEvent = new AccessEvent(
      event.transaction.hash.toHex() + "-" + event.logIndex.toString()
    )
    accessEvent.permission = permission.id
    accessEvent.eventType = "UPDATED"
    accessEvent.timestamp = event.block.timestamp
    accessEvent.blockNumber = event.block.number
    accessEvent.transactionHash = event.transaction.hash
    accessEvent.save()
  }
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
  
  stats.totalAccessPermissions = stats.totalAccessPermissions.plus(BigInt.fromI32(1))
  stats.lastUpdated = timestamp
  stats.save()
}

