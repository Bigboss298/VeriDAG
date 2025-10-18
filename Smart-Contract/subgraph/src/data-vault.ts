import { BigInt } from "@graphprotocol/graph-ts"
import {
  DataUploaded,
  DataDeactivated,
  DataReactivated,
  DataMetadataUpdated
} from "../generated/DataVaultContract/DataVaultContract"
import { DataRecord, DataEvent, Institution, GlobalStatistics } from "../generated/schema"

export function handleDataUploaded(event: DataUploaded): void {
  let record = new DataRecord(event.params.recordId)
  
  record.recordId = event.params.recordId
  record.dataHash = event.params.dataHash
  record.owner = event.params.owner.toHexString()
  record.fileName = ""
  record.fileType = ""
  record.fileSize = BigInt.fromI32(0)
  record.ipfsHash = event.params.ipfsHash
  record.encryptionAlgorithm = "AES-256-GCM"
  record.uploadedAt = event.params.timestamp
  record.isActive = true
  record.metadataUri = ""
  record.category = ""
  record.totalPermissions = BigInt.fromI32(0)
  record.totalVerifications = BigInt.fromI32(0)
  record.updatedAt = event.block.timestamp
  
  record.save()
  
  // Update institution statistics
  let institution = Institution.load(event.params.owner.toHexString())
  if (institution) {
    institution.totalDataRecords = institution.totalDataRecords.plus(BigInt.fromI32(1))
    institution.save()
  }
  
  // Create data event
  let dataEvent = new DataEvent(
    event.transaction.hash.toHex() + "-" + event.logIndex.toString()
  )
  dataEvent.record = record.id
  dataEvent.eventType = "UPLOADED"
  dataEvent.timestamp = event.block.timestamp
  dataEvent.blockNumber = event.block.number
  dataEvent.transactionHash = event.transaction.hash
  dataEvent.save()
  
  // Update global statistics
  updateGlobalStatistics(event.block.timestamp)
}

export function handleDataDeactivated(event: DataDeactivated): void {
  let record = DataRecord.load(event.params.recordId)
  if (record) {
    record.isActive = false
    record.updatedAt = event.block.timestamp
    record.save()
    
    // Create event
    let dataEvent = new DataEvent(
      event.transaction.hash.toHex() + "-" + event.logIndex.toString()
    )
    dataEvent.record = record.id
    dataEvent.eventType = "DEACTIVATED"
    dataEvent.timestamp = event.block.timestamp
    dataEvent.blockNumber = event.block.number
    dataEvent.transactionHash = event.transaction.hash
    dataEvent.save()
  }
}

export function handleDataReactivated(event: DataReactivated): void {
  let record = DataRecord.load(event.params.recordId)
  if (record) {
    record.isActive = true
    record.updatedAt = event.block.timestamp
    record.save()
    
    // Create event
    let dataEvent = new DataEvent(
      event.transaction.hash.toHex() + "-" + event.logIndex.toString()
    )
    dataEvent.record = record.id
    dataEvent.eventType = "REACTIVATED"
    dataEvent.timestamp = event.block.timestamp
    dataEvent.blockNumber = event.block.number
    dataEvent.transactionHash = event.transaction.hash
    dataEvent.save()
  }
}

export function handleDataMetadataUpdated(event: DataMetadataUpdated): void {
  let record = DataRecord.load(event.params.recordId)
  if (record) {
    record.updatedAt = event.block.timestamp
    record.save()
    
    // Create event
    let dataEvent = new DataEvent(
      event.transaction.hash.toHex() + "-" + event.logIndex.toString()
    )
    dataEvent.record = record.id
    dataEvent.eventType = "METADATA_UPDATED"
    dataEvent.timestamp = event.block.timestamp
    dataEvent.blockNumber = event.block.number
    dataEvent.transactionHash = event.transaction.hash
    dataEvent.save()
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
  
  stats.totalDataRecords = stats.totalDataRecords.plus(BigInt.fromI32(1))
  stats.lastUpdated = timestamp
  stats.save()
}

