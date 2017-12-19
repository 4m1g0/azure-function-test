using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

public class AzureStorageMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
{
    private readonly CloudBlobContainer _blobContainer;

    public AzureStorageMultipartFormDataStreamProvider(CloudBlobContainer blobContainer) : base("azure")
    {
        _blobContainer = blobContainer;
    }

    public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (headers == null) throw new ArgumentNullException(nameof(headers));

        // Generate a new filename for every new blob
        var fileName = Guid.NewGuid().ToString();

        CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(fileName);

        if (headers.ContentType != null)
        {
            // Set appropriate content type for your uploaded file
            blob.Properties.ContentType = headers.ContentType.MediaType;
        }

        this.FileData.Add(new MultipartFileData(headers, blob.Name));

        return blob.OpenWrite();
    }
}