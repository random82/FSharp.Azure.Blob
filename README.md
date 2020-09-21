# FSharp.Azure.Blob [![NuGet version (FSharp.Azure.Blob)](https://img.shields.io/nuget/v/FSharp.Azure.Blob.svg?style=flat-square)](https://www.nuget.org/packages/FSharp.Azure.Blob/)

Making Azure Blob API more FSharp friendly

## Usage

### Upload blob

```fsharp
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

let uploadTestJson =
    use file = File.OpenRead "test.json"

    async {
        let! blobInfo = uploadFile "container" file
        blobInfo.Value.ContentHash
        |> BitConverter.ToString
        |> Console.WriteLine
    } |> Async.RunSynchronously
```

### Download blob

```fsharp
let readFile containerName blob =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.download blob
                    |> Blob.execAsync<BlobDownloadInfo>
    result

let downloadTestJson =
    async {
        let! blobInfo = readFile "container" "test.json"
        use sw = new StreamReader(blobInfo.Value.Content)
        sw.ReadToEnd() |> Console.WriteLine
    } |> Async.RunSynchronously
```



### Delete blob

```fsharp
let deleteFile containerName blob =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.delete blob
                    |> Blob.includeSnapshots true
                    |> Blob.execAsync<bool>
    result
	
let deleteTestJson =
	async {
        let! result = deleteFile "container" "test.json"
        match result.Value with
        | true ->  Console.WriteLine "Success"
        | false ->  failwith "File should be deleted"
    } |> Async.RunSynchronously
```

### Delete snapshots

```fsharp
let deleteSnapshots containerName blob =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString 
                    |> Blob.container containerName
                    |> Blob.deleteSnapshots blob
                    |> Blob.execAsync<bool>
    result
	
let deleteTestJsonSnapshots =
	async {
        let! result = deleteSnapshots "container" "test.json"
        match result.Value with
        | true ->  Console.WriteLine "Success"
        | false ->  failwith "Snapshots should be deleted"
    } |> Async.RunSynchronously	
```

### Check if blob exists

```fsharp
let existsFile containerName blob =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.exists blob
                    |> Blob.execAsync<bool>
    result
	
let checkTestJsonExists =
	async {
        let! result = existsFile "container" "test.json"
        match result.Value with
        | true ->  Console.WriteLine "Blob exists"
        | false ->  failwith "Blob not found"
    } |> Async.RunSynchronously  
```

### Retreive blob properties / metadata

To learn more about properties and metadata please consult the documentation: 
[Manage blob properties and metadata with .NET](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-properties-metadata?tabs=dotnet)

```fsharp
let getProperties containerName blob =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.getProperties blob
                    |> Blob.execAsync<BlobProperties>
    result
	
let getTestJsonPropertiesAndMetadata =
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
```

### Set / Update Properties

Note that when you wish to change a property you need to set all other values in
the property object by first retrieving them by calling get properties.

```fsharp
let setProperties containerName blob properties =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.setProperties blob properties
                    |> Blob.execAsync<BlobInfo>
    result
	
let setTestJsonProperties =
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
```

### Set / Update Metadata

Note that when you wish to add or update metadata you need to first retrieve the
existing metadata by calling get properties.

```fsharp
let setMetadata containerName blob metadata =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.setMetadata blob metadata
                    |> Blob.execAsync<BlobInfo>
    result
	
let setTestJsonMetadata =
	async {
        let! blobProperites = getProperties "container" "test.json"  
        let metadata = blobProperites.Value.Metadata
        metadata.Add("new-metadata-key","new-metadata-value")
        let! blobInfo = setMetadata "container" "test.json" metadata
        Console.WriteLine "Metadata set"
    } |> Async.RunSynchronously
```

## Development

## Sample run in VS Code

1. Run Azure Storage Emulator
2. Use VSCode to run a sample client
3. Check the debug console
