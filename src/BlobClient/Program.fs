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
                    |> Blob.readBlob blob
                    |> Blob.execAsync<BlobDownloadInfo>
    result
    


let uploadFile containerName file =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString 
                    |> Blob.container containerName
                    |> Blob.createOrUpdate "test.json" file
                    |> Blob.overwriteBlob true
                    |> Blob.createContainer true
                    |> Blob.execAsync<BlobContentInfo>

    result


[<EntryPoint>]
let main _ =

    use file = File.OpenRead "test.json"

    async {
        let! blobInfo = uploadFile "invoice" file
        blobInfo.Value.ContentHash
        |> BitConverter.ToString
        |> Console.WriteLine
    } |> Async.RunSynchronously

    async {
        let! blobInfo = readFile "invoice" "test.json"
        use sw = new StreamReader(blobInfo.Value.Content)
        sw.ReadToEnd() |> Console.WriteLine
        
    } |> Async.RunSynchronously

    0