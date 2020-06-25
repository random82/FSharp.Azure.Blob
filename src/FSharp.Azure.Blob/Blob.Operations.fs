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
                        | None -> failwith "No blobName provided"

        match result with
        | Some result ->
            result
        | None -> failwith "Unable to download the blob"

