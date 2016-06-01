using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using OutdoorKumichoAPI.Models;
using Newtonsoft.Json;

namespace Contososonpo.Controllers
{
    public class CancelController : ApiController
    {
        // GET api/cancel/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Get(string id)
        {
            CloudBlobContainer sonpocontainer;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            sonpocontainer = blobClient.GetContainerReference("outdoorkumicho");
            sonpocontainer.CreateIfNotExists();
            Activity eventinfo = JsonConvert.DeserializeObject<Activity>(id);
            CloudBlockBlob blob = sonpocontainer.GetBlockBlobReference(string.Format("{0}.json", eventinfo.Title));
            blob.DeleteIfExists(); 
            return sonpocontainer.Uri.ToString();
        }
    }
}
