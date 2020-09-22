module BlobClient

open FSharp.Azure.Blob
open System.IO
open System
open Azure.Storage.Blobs.Models

let readFile containerName blob =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString 
                    |> Blob.container containerName
                    |> Blob.download blob
                    |> Blob.execAsync<BlobDownloadInfo>
    result

let uploadFile containerName file =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString 
                    |> Blob.container containerName
                    |> Blob.upload "test.json" file
                    |> Blob.overwriteBlob true
                    |> Blob.createContainer true
                    |> Blob.execAsync<BlobContentInfo>
    result

let deleteFile containerName blob  =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString 
                    |> Blob.container containerName
                    |> Blob.delete blob
                    |> Blob.includeSnapshots true
                    |> Blob.execAsync<bool>
    result

let deleteSnapshots containerName blob  =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString 
                    |> Blob.container containerName
                    |> Blob.deleteSnapshots blob
                    |> Blob.execAsync<bool>
    result

let existsFile containerName blob =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.exists blob
                    |> Blob.execAsync<bool>
    result

let getProperties containerName blob =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.getProperties blob
                    |> Blob.execAsync<BlobProperties>
    result

let setProperties containerName blob properties =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.setProperties blob properties
                    |> Blob.execAsync<BlobInfo>
    result

let setMetadata containerName blob metadata =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.setMetadata blob metadata
                    |> Blob.execAsync<BlobInfo>
    result

[<EntryPoint>]
let main _ =

    use file = File.OpenRead "test.json"
    
    // Upload
    async {
        let! blobContentInfo = uploadFile "container" file
        blobContentInfo.Value.ContentHash
        |> BitConverter.ToString
        |> Console.WriteLine
    } |> Async.RunSynchronously

    // Check If Exists
    async {
        let! result = existsFile "container" "test.json"
        match result.Value with
        | true ->  Console.WriteLine "Blob exists"
        | false ->  failwith "Blob not found"
    } |> Async.RunSynchronously    

    // Download
    async {
        let! blobInfo = readFile "container" "test.json"
        use sw = new StreamReader(blobInfo.Value.Content)
        sw.ReadToEnd() |> Console.WriteLine
    } |> Async.RunSynchronously

    // Set / Update Properties
    async {
        let! blobProperites = getProperties "container" "test.json"  
        let properties = new Azure.Storage.Blobs.Models.BlobHttpHeaders()
        properties.CacheControl <- blobProperites.Value.CacheControl
        properties.ContentDisposition <- blobProperites.Value.ContentDisposition
        properties.ContentEncoding <- blobProperites.Value.ContentEncoding
        properties.ContentHash <- blobProperites.Value.ContentHash
        properties.ContentLanguage <- blobProperites.Value.ContentLanguage
        properties.ContentType <- blobProperites.Value.ContentType
        let! blobInfo = setProperties "container" "test.json" properties
        Console.WriteLine "Properties set"
    } |> Async.RunSynchronously

    // Set / Update Metadata
    async {
        let! blobProperites = getProperties "container" "test.json"  
        let metadata = blobProperites.Value.Metadata
        metadata.Add("new-metadata-key","new-metadata-value")
        let! blobInfo = setMetadata "container" "test.json" metadata
        Console.WriteLine "Metadata set"
    } |> Async.RunSynchronously

    // Get Properties / Metadata
    async {
        let! blobProperites = getProperties "container" "test.json"  
        let properties = blobProperites.Value
        Console.WriteLine "Properties:"
        Console.WriteLine ("  Content Type: " + properties.ContentType)
        Console.WriteLine ("  Content Length: " + properties.ContentLength.ToString())
        let metadata = blobProperites.Value.Metadata
        Console.WriteLine "Metadata:"
        metadata.Keys |> Seq.iter (fun k -> printfn "  \"%s\" : \"%s\"" k (metadata.Item k))            
    } |> Async.RunSynchronously

    // Delete
    async {
        let! result = deleteFile "container" "test.json"
        match result.Value with
        | true ->  Console.WriteLine "Success"
        | false ->  failwith "File should be deleted"
    } |> Async.RunSynchronously

    0