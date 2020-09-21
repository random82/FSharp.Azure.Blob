namespace FSharp.Azure.Blob

open Azure
open Azure.Storage.Blobs
open System
open System.IO

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

    let fromConnectionString (connectionString: string) = 
        { defaultConnectionOp() with
            Type = FromConnectionString
            ConnectionString = Some connectionString
        }

    let fromConnectionStringWithOptions (connectionString: string) (options: BlobClientOptions) = {
        defaultConnectionOp() with
            Options = Some options
            Type = FromConnectionString
            ConnectionString = Some connectionString 
    }

    let fromConnectionUri (uri: System.Uri) = { 
        defaultConnectionOp() with
            Type = FromUri
            Uri = Some uri
    }

    let fromConnectionUriWithOptions (uri: string) (options: BlobClientOptions) = { 
        defaultConnectionOp() with
            Options = Some options
            Type = FromUri
            ConnectionString = Some uri 
    }

    let container (containerName: string) (options: ConnectionOperation) = {
        options with 
            ContainerName = Some containerName
    }

    let upload (blobName: string) (item: Stream) (options: ConnectionOperation) = 
        Upload {
            Connection = options
            BlobName = blobName
            Item = item
            CreateContainer = CreateContainer false
            OverwriteBlob =  OverwriteBlob false
    }

    let download (blobName: string) (options: ConnectionOperation) =
        Download {
            Connection = options
            BlobName = Some blobName
        }

    let delete (blobName: string) (options: ConnectionOperation) =
        Delete {
            Connection = options
            BlobName = Some blobName
            InludeSnapshots = InludeSnapshots false
        }

    let deleteSnapshots (blobName: string) (options: ConnectionOperation) =
        DeleteSnapshots {
            Connection = options
            BlobName = Some blobName
        }

    let includeSnapshots (includeSnapshots: bool) (blobOperation: BlobOperation) =
        match blobOperation with
        | Delete op ->
            Delete {
                op with
                    InludeSnapshots =  InludeSnapshots includeSnapshots
            }
        | _ -> failwith "This operation is valid only for Delete" 

    let exists (blobName: string) (options: ConnectionOperation) =
        Exists {
            Connection = options
            BlobName = Some blobName
        }

    let getProperties (blobName: string) (options: ConnectionOperation) =
        GetProperties {
            Connection = options
            BlobName = Some blobName
        }

    let setProperties (blobName: string) (properties: Azure.Storage.Blobs.Models.BlobHttpHeaders) (options: ConnectionOperation) =
        SetProperties {
            Connection = options
            BlobName = Some blobName
            Properties = Some properties
        }

    let setMetadata (blobName: string) (metadata: Collections.Generic.IDictionary<string,string>) (options: ConnectionOperation) =
        SetMetadata {
            Connection = options
            BlobName = Some blobName
            Metadata = Some metadata
        }

    let overwriteBlob (overwriteBlob: bool) (blobOperation: BlobOperation) = 
        match blobOperation with
        | Upload op ->
            Upload {
                op with
                    OverwriteBlob =  OverwriteBlob overwriteBlob
            }
        | _ -> failwith "This operation is valid only for Upload"

    let createContainer (createContainer: bool) (blobOperation: BlobOperation) = 
        match blobOperation with
        | Upload op ->
            Upload {
                op with
                    CreateContainer =  CreateContainer createContainer
            }
        | _ -> failwith "This operation is valid only for upload"

    let private getClient (connInfo: ConnectionOperation) =
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
        | GetProperties op ->
            unbox BlobOperations.getProperties getClient op
        | SetMetadata op ->
            unbox BlobOperations.setMetadata getClient op

