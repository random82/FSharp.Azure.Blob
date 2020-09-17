namespace FSharp.Azure.Blob

open Azure.Storage.Blobs
open Azure.Storage.Blobs.Models
open FSharp.Control

[<RequireQualifiedAccess>]
module internal BlobOperations =
    let execUpload (getClient: ConnectionOperation -> BlobContainerClient) (op: UploadOp) = 
        let connInfo = op.Connection
        let client = getClient connInfo

        let blobClient = client.GetBlobClient op.BlobName
        let (CreateContainer createContainer) = op.CreateContainer
        let result = 
            match createContainer with
            | true ->
                maybe {
                    let (OverwriteBlob overwriteBlob) = op.OverwriteBlob 
                    let publicAccess = PublicAccessType.None
                    let creationResult = client.CreateIfNotExistsAsync publicAccess |> Async.AwaitTask
                    return blobClient.UploadAsync(op.Item, overwriteBlob) |> Async.AwaitTask
                }
            | false -> 
                maybe {
                    let (OverwriteBlob overwriteBlob) = op.OverwriteBlob 
                    return blobClient.UploadAsync(op.Item, overwriteBlob) |> Async.AwaitTask
                }

        match result with
        | Some result -> 
            result
        | None -> failwith "Unable to upload the file to blob"

    let execDownload (getClient: ConnectionOperation -> BlobContainerClient) (op: DownloadOp) = 
        let connInfo = op.Connection
        let client = getClient connInfo

        let result = match op.BlobName with
                        | Some blobName ->
                                maybe {
                                    let blobClient = client.GetBlobClient blobName
                                    return blobClient.DownloadAsync() |> Async.AwaitTask
                                }
                        | None -> failwith "No blob name provided"

        match result with
        | Some result ->
            result
        | None -> failwith "Unable to download the blob"

    let createDeleteSnapshotOptions op =
        let (InludeSnapshots includeSnapshots) = op.InludeSnapshots 
        match includeSnapshots with
        | true -> DeleteSnapshotsOption.IncludeSnapshots
        | _ -> DeleteSnapshotsOption.None

    let execDelete (getClient: ConnectionOperation -> BlobContainerClient) (op: DeleteOp) = 
        let connInfo = op.Connection
        let client = getClient connInfo

        let result = match op.BlobName with
                        | Some blobName ->
                                maybe {
                                    let blobClient = client.GetBlobClient blobName
                                    let options = createDeleteSnapshotOptions op
                                    return blobClient.DeleteIfExistsAsync(options) |> Async.AwaitTask
                                }
                        | None -> failwith "No blob name provided"

        match result with
        | Some result ->
            result
        | None -> failwith "Unable to delete the blob"


    let execDeleteSnapshots (getClient: ConnectionOperation -> BlobContainerClient) (op: DeleteOp) = 
        let connInfo = op.Connection
        let client = getClient connInfo

        let result = match op.BlobName with
                        | Some blobName ->
                                maybe {
                                    let blobClient = client.GetBlobClient blobName
                                    return blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.OnlySnapshots) |> Async.AwaitTask
                                }
                        | None -> failwith "No blob name provided"

        match result with
        | Some result ->
            result
        | None -> failwith "Unable to delete the blob snapshots"

    let execExists (getClient: ConnectionOperation -> BlobContainerClient) (op: ExistsOp) =
        let connInfo = op.Connection
        let client = getClient connInfo

        let result = match op.BlobName with
                        | Some blobName ->
                                maybe {
                                    let blobClient = client.GetBlobClient blobName
                                    return blobClient.ExistsAsync() |> Async.AwaitTask
                                }
                        | None -> failwith "No blob name provided"

        match result with
        | Some result ->
            result
        | None -> failwith "Unable to verify if the blob exists"

