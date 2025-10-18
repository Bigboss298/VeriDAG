import { BigInt } from "@graphprotocol/graph-ts"
import {
  InstitutionRegistered,
  InstitutionDeactivated,
  InstitutionReactivated,
  InstitutionUpdated
} from "../generated/InstitutionRegistry/InstitutionRegistry"
import { Institution, InstitutionEvent, GlobalStatistics } from "../generated/schema"

export function handleInstitutionRegistered(event: InstitutionRegistered): void {
  let institution = new Institution(event.params.wallet.toHexString())
  
  institution.name = event.params.name
  institution.institutionType = event.params.institutionType
  institution.registrationNumber = ""
  institution.walletAddress = event.params.wallet
  institution.registeredAt = event.params.timestamp
  institution.isActive = true
  institution.metadataUri = ""
  institution.totalDataRecords = BigInt.fromI32(0)
  institution.totalAccessGrants = BigInt.fromI32(0)
  institution.totalAuditLogs = BigInt.fromI32(0)
  institution.updatedAt = event.block.timestamp
  
  institution.save()
  
  // Create institution event
  let institutionEvent = new InstitutionEvent(
    event.transaction.hash.toHex() + "-" + event.logIndex.toString()
  )
  institutionEvent.institution = institution.id
  institutionEvent.eventType = "REGISTERED"
  institutionEvent.timestamp = event.block.timestamp
  institutionEvent.blockNumber = event.block.number
  institutionEvent.transactionHash = event.transaction.hash
  institutionEvent.save()
  
  // Update global statistics
  updateGlobalStatistics(event.block.timestamp)
}

export function handleInstitutionDeactivated(event: InstitutionDeactivated): void {
  let institution = Institution.load(event.params.wallet.toHexString())
  if (institution) {
    institution.isActive = false
    institution.updatedAt = event.block.timestamp
    institution.save()
    
    // Create event
    let institutionEvent = new InstitutionEvent(
      event.transaction.hash.toHex() + "-" + event.logIndex.toString()
    )
    institutionEvent.institution = institution.id
    institutionEvent.eventType = "DEACTIVATED"
    institutionEvent.timestamp = event.block.timestamp
    institutionEvent.blockNumber = event.block.number
    institutionEvent.transactionHash = event.transaction.hash
    institutionEvent.save()
  }
}

export function handleInstitutionReactivated(event: InstitutionReactivated): void {
  let institution = Institution.load(event.params.wallet.toHexString())
  if (institution) {
    institution.isActive = true
    institution.updatedAt = event.block.timestamp
    institution.save()
    
    // Create event
    let institutionEvent = new InstitutionEvent(
      event.transaction.hash.toHex() + "-" + event.logIndex.toString()
    )
    institutionEvent.institution = institution.id
    institutionEvent.eventType = "REACTIVATED"
    institutionEvent.timestamp = event.block.timestamp
    institutionEvent.blockNumber = event.block.number
    institutionEvent.transactionHash = event.transaction.hash
    institutionEvent.save()
  }
}

export function handleInstitutionUpdated(event: InstitutionUpdated): void {
  let institution = Institution.load(event.params.wallet.toHexString())
  if (institution) {
    institution.name = event.params.name
    institution.updatedAt = event.block.timestamp
    institution.save()
    
    // Create event
    let institutionEvent = new InstitutionEvent(
      event.transaction.hash.toHex() + "-" + event.logIndex.toString()
    )
    institutionEvent.institution = institution.id
    institutionEvent.eventType = "UPDATED"
    institutionEvent.timestamp = event.block.timestamp
    institutionEvent.blockNumber = event.block.number
    institutionEvent.transactionHash = event.transaction.hash
    institutionEvent.save()
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
  
  stats.totalInstitutions = stats.totalInstitutions.plus(BigInt.fromI32(1))
  stats.lastUpdated = timestamp
  stats.save()
}

