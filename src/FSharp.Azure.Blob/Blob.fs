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

    let createOrUpdate blobName item options = 
        CreateOrUpdate {
            Connection = options
            BlobName = blobName
            Item = item
            CreateContainer = CreateContainer false
            OverwriteBlob =  OverwriteBlob false
    }

    let readBlob blobName options =
        Read {
            Connection = options
            BlobName = Some blobName
        }

    let overwriteBlob overwriteBlob (blobOperation: BlobOperation) = 
        match blobOperation with
        | CreateOrUpdate op ->
            CreateOrUpdate {
                op with
                    OverwriteBlob =  OverwriteBlob overwriteBlob
            }
        | _ -> failwith "This operation is valid only for CreateOrUpdate"



    let createContainer createContainer blobOperation = 
        match blobOperation with
        | CreateOrUpdate op ->
            CreateOrUpdate {
                op with
                    CreateContainer =  CreateContainer createContainer
            }
        | _ -> failwith "This operation is valid only for CreateOrUpdate"


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
        | CreateOrUpdate op -> 
            unbox BlobOperations.execCreateOrUpdate getClient op
        | Read op-> 
            unbox BlobOperations.execRead getClient op

