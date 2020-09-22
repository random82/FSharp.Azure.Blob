namespace FSharp.Azure.Blob

open Azure.Storage.Blobs
open System
open System.IO

type CreateContainer = CreateContainer of bool
type OverwriteBlob = OverwriteBlob of bool
type InludeSnapshots = InludeSnapshots of bool

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

type DownloadOp =
    {
        Connection: ConnectionOperation
        BlobName: string option
    }

type DeleteOp =
    {
        Connection: ConnectionOperation
        BlobName: string option
        InludeSnapshots: InludeSnapshots
    }

type DeleteSnapshotsOp =
    {
        Connection: ConnectionOperation
        BlobName: string option
    }

type UploadOp =
    {
        Connection: ConnectionOperation
        BlobName: string
        Item: Stream
        CreateContainer: CreateContainer
        OverwriteBlob: OverwriteBlob
    }

type ExistsOp =
    {
        Connection: ConnectionOperation
        BlobName: string option
    }

type GetPropertiesOp =
    {
        Connection: ConnectionOperation
        BlobName: string option
    }

type SetPropertiesOp =
    {
        Connection: ConnectionOperation
        BlobName: string option
        Properties: Azure.Storage.Blobs.Models.BlobHttpHeaders option
    }

type SetMetadataOp =
    {
        Connection: ConnectionOperation
        BlobName: string option
        Metadata: Collections.Generic.IDictionary<string,string> option
    }

type BlobOperation =
    | Upload of UploadOp
    | Download of DownloadOp
    | Delete of DeleteOp
    | DeleteSnapshots of DeleteSnapshotsOp
    | Exists of ExistsOp
    | GetProperties of GetPropertiesOp
    | SetProperties of SetPropertiesOp
    | SetMetadata of SetMetadataOp