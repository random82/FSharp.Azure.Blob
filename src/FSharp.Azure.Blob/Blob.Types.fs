namespace FSharp.Azure.Blob

open Azure.Storage.Blobs
open System
open System.IO
open Azure.Storage.Blobs.Models

type CreateContainer = CreateContainer of bool
type OverwriteBlob = OverwriteBlob of bool

type ConnectionType = 
    | FromConnectionString
    | FromUri
    | NotDefined

type ConnectionOperation =
    {
        Options: BlobClientOptions option
        Type: ConnectionType
        Endpoint: string option
        AccessKey: string option
        ConnectionString: string option
        Uri: Uri option
        ContainerName: string option
    }

type ReadOp =
    {
        Connection: ConnectionOperation
        BlobName: string option
    }

type CreateOrUpdateOp =
    {
        Connection: ConnectionOperation
        BlobName: string
        Item: Stream
        CreateContainer: CreateContainer
        OverwriteBlob: OverwriteBlob
    }

type UpdateOp =
    { 
        Connection: ConnectionOperation
        Id: string 
    }

type DeleteOp =
    { 
        Connection: ConnectionOperation
        Id: string
    }
type BlobOperation =
    | CreateOrUpdate of CreateOrUpdateOp
    | Read of ReadOp