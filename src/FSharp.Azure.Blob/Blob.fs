namespace FSharp.Azure.Blob
open Azure.Storage.Blobs
open System.IO
open Azure.Storage.Blobs.Models
open Azure

[<RequireQualifiedAccess>]
module Blob =
    let private defaultConnectionOp() = 
        {
            Options = None
            Endpoint = None
            AccessKey = None
            Type = NotDefined
            Uri = None
            ConnectionString = None
            ContainerName = None
        }

    let fromConnectionString connectionString = 
        { defaultConnectionOp() with
            Type = FromConnectionString
            ConnectionString = Some connectionString
        }

    let fromConnectionStringWithOptions connectionString options = {
        defaultConnectionOp() with
            Options = Some options
            Type = FromConnectionString
            ConnectionString = Some connectionString 
    }

    let fromConnectionUri uri = { 
        defaultConnectionOp() with
            Type = FromUri
            Uri = Some uri
    }

    let fromConnectionUriWithOptions uri options = { 
        defaultConnectionOp() with
            Options = Some options
            Type = FromUri
            ConnectionString = Some uri 
    }

    let container containerName options = {
        options with 
            ContainerName = Some containerName
    }

    let upload blobName item options = 
        Upload {
            Connection = options
            BlobName = blobName
            Item = item
            CreateContainer = CreateContainer false
            OverwriteBlob =  OverwriteBlob false
    }

    let download blobName options =
        Download {
            Connection = options
            BlobName = Some blobName
        }

    let delete blobName options =
        Delete {
            Connection = options
            BlobName = Some blobName
            InludeSnapshots = InludeSnapshots false
        }

    let deleteSnapshots blobName options =
        DeleteSnapshots {
            Connection = options
            BlobName = Some blobName
        }

    let includeSnapshots includeSnapshots (blobOperation: BlobOperation) =
        match blobOperation with
        | Delete op ->
            Delete {
                op with
                    InludeSnapshots =  InludeSnapshots includeSnapshots
            }
        | _ -> failwith "This operation is valid only for Upload" 

    let exists blobName options =
        Exists {
            Connection = options
            BlobName = Some blobName
        }

    let overwriteBlob overwriteBlob (blobOperation: BlobOperation) = 
        match blobOperation with
        | Upload op ->
            Upload {
                op with
                    OverwriteBlob =  OverwriteBlob overwriteBlob
            }
        | _ -> failwith "This operation is valid only for Upload"

    let createContainer createContainer blobOperation = 
        match blobOperation with
        | Upload op ->
            Upload {
                op with
                    CreateContainer =  CreateContainer createContainer
            }
        | _ -> failwith "This operation is valid only for upload"


    let private getClient connInfo =
        let clientOps =
            match connInfo.Options with
            | Some options -> options
            | None -> BlobClientOptions()

        let client =
            match connInfo.Type with
            | FromConnectionString ->
                maybe {
                    let! connStr = connInfo.ConnectionString
                    let! containerName = connInfo.ContainerName
                    return BlobContainerClient(connStr, containerName, clientOps) }
            | FromUri ->
                maybe {
                    let! uri = connInfo.Uri
                    return BlobContainerClient(uri, clientOps) }
            | _ -> failwith "No connection type provided"

        match client with
        | Some client -> client
        | None -> failwith "No connection information provided"


    let execAsync<'T> (op: BlobOperation): Async<Response<'T>> =
        match op with
        | Upload op -> 
            unbox BlobOperations.execUpload getClient op
        | Download op-> 
            unbox BlobOperations.execDownload getClient op
        | Delete op ->
            unbox BlobOperations.execDelete getClient op
        | DeleteSnapshots op -> 
            unbox BlobOperations.execDeleteSnapshots getClient op
        | Exists op ->
            unbox BlobOperations.execExists getClient op

