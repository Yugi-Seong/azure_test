using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.Linq;

namespace Function
{
    public static class HttpTrigger1
    {
        [FunctionName("HttpTrigger1")]
        public static string Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequest req, ILogger log, ExecutionContext context)

        {
            string connStrA = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            string datetime = DateTime.UtcNow.AddHours(9).ToString("yyyyMMdd hh:mm");
            
            JObject objB = (JObject)JsonConvert.DeserializeObject(requestBody);

            BlobServiceClient clientA = new BlobServiceClient(connStrA);
            BlobContainerClient containerA = clientA.GetBlobContainerClient("yugicon");
            BlobClient blobA = containerA.GetBlobClient(datetime + ".json");

            string responseA = "No Data";
            JObject ObjA = new JObject();

            if (blobA.Exists())
            {
                using (MemoryStream msA = new MemoryStream())
                {
                    blobA.DownloadTo(msA);
                    responseA = System.Text.Encoding.UTF8.GetString(msA.ToArray());
                    ObjA = JObject.Parse(responseA);
                }
            }

            
                IList<string> keys = objB.Properties().Select(p => p.Name).ToList();    
                foreach (var item in keys)
                {
                    ObjA.Add(item, objB[item]);
                }
            
            

            // string tagData = Newtonsoft.Json.JsonConvert.SerializeObject(responseA);
            string tagData = Newtonsoft.Json.JsonConvert.SerializeObject(ObjA);
            
            using (Stream streamA = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tagData)))
            {
                blobA.Upload(streamA, true);
            }
            
            return requestBody;
        }
    }
}