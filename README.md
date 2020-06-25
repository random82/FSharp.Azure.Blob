# FSharp.Azure.Blob

## Usage

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
```

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
```

### Delete blob

```fsharp
let deleteFile containerName blob  =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString
                    |> Blob.container containerName
                    |> Blob.delete blob
                    |> Blob.includeSnapshots true
                    |> Blob.execAsync<bool>
    result
```

### Delete snapshots

```fsharp
let deleteSnapshots containerName blob  =
    let connString = "UseDevelopmentStorage=true"
    let result = connString
                    |> Blob.fromConnectionString 
                    |> Blob.container containerName
                    |> Blob.deleteSnapshots blob
                    |> Blob.execAsync<bool>
    result
```

## Development

## Sample run in VS Code

1. Run Azure Storage Emulator
2. Use VSCode to run a sample client
3. Check the debug console
