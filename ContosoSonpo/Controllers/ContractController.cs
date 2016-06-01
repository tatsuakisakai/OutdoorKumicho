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
    public class ContractController : ApiController
    {
        // POST api/contract/5
        [SwaggerOperation("Post")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Post([FromBody]string value)
        {
            SaveDataToBlob(value);
        }

        // DELETE api/contract/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(string id)
        {
            CloudBlobContainer sonpocontainer;
            //コンテナの取得
            sonpocontainer = GetContainer();
            //Blobの取得
            CloudBlockBlob blob = sonpocontainer.GetBlockBlobReference(string.Format("{0}.json", id));
            //Blob存在時に削除
            blob.DeleteIfExists();
        }
        private static CloudBlobContainer GetContainer()
        {
            CloudBlobContainer sonpocontainer;
            //Storage Accountの作成
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //BlobClientの作成
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //コンテナの取得（存在しない場合は新規作成)
            sonpocontainer = blobClient.GetContainerReference("outdoorkumicho");
            sonpocontainer.CreateIfNotExists();
            return sonpocontainer;
        }

        private static string SaveDataToBlob(string id)
        {
            CloudBlobContainer sonpocontainer;
            //コンテナの取得
            sonpocontainer = GetContainer();
            //Jsonのデシリアライズ
            Activity eventinfo = JsonConvert.DeserializeObject<Activity>(id);
            //Blobの取得
            CloudBlockBlob blob = sonpocontainer.GetBlockBlobReference(string.Format("{0}.json", eventinfo.EventID));
            //BlobにデータをUpload
            blob.UploadText(JsonConvert.SerializeObject(eventinfo));
            return blob.Uri.ToString();
        }
    }
}
