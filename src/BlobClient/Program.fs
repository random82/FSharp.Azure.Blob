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


[<EntryPoint>]
let main _ =

    use file = File.OpenRead "test.json"

    async {
        let! blobInfo = uploadFile "container" file
        blobInfo.Value.ContentHash
        |> BitConverter.ToString
        |> Console.WriteLine
    } |> Async.RunSynchronously

    async {
        let! blobInfo = readFile "container" "test.json"
        use sw = new StreamReader(blobInfo.Value.Content)
        sw.ReadToEnd() |> Console.WriteLine
        
    } |> Async.RunSynchronously

    async {
        let! result = deleteFile "container" "test.json"
        match result.Value with
        | true ->  Console.WriteLine "Success"
        | false ->  failwith "File should be deleted"
        
    } |> Async.RunSynchronously

    0