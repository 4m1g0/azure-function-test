using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace UploadFileFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("CONNECTION STRING");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("quickstartblobscb838932-2910-475b-a353-982d28c64a94");


            if (req.Method == HttpMethod.Get)
            {
                string response = "<ul>";

                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;

                        response += string.Format("<li>Block blob of length {0}: <a href=\"{1}\">{2}</a></li>\n", blob.Properties.Length, blob.Uri, blob.Name);

                    }
                    else if (item.GetType() == typeof(CloudPageBlob))
                    {
                        CloudPageBlob pageBlob = (CloudPageBlob)item;

                        response += string.Format("<li>Page blob of length {0}: <a href=\"{1}\">{2}</a></li>\n", pageBlob.Properties.Length, pageBlob.Uri, pageBlob.Name);

                    }
                    else if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        CloudBlobDirectory directory = (CloudBlobDirectory)item;

                        response += string.Format("<li>Directory: {0}</li>\n", directory.Uri);
                    }
                }

                response += @"</ul>
                  <form method=""post"" enctype=""multipart/form-data"">
                    <input type=""file"" name=""fileToUpload"" id=""fileToUpload"">
                    <input type = ""submit"" value = ""Upload Image"" name = ""submit"" >
                  </form>";

                var r = req.CreateResponse(HttpStatusCode.OK, response, "text/plain");

                r.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");
                return r;
            }

            if (req.Method == HttpMethod.Post)
            {
                if (req.Content.IsMimeMultipartContent())
                {
                    var streamProvider = new AzureStorageMultipartFormDataStreamProvider(container);
                    var task = req.Content.ReadAsMultipartAsync(streamProvider);

                    var filename = streamProvider.FileData.FirstOrDefault()?.LocalFileName;
                    if (string.IsNullOrEmpty(filename))
                    {
                        return req.CreateResponse(HttpStatusCode.BadRequest, "An error has occured while uploading your file. Please try again.");
                    }

                    return req.CreateResponse(HttpStatusCode.OK, $"File: {filename} has successfully uploaded");
                }

                return req.CreateResponse(HttpStatusCode.OK, "ok");
            }

            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
