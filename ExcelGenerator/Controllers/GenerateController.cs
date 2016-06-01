using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using OutdoorKumichoAPI.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace EXCELGenerator.Controllers
{
    public class GenerateController : ApiController
    {
        [SwaggerOperation("Post")]
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Post([FromBody]string value)
        {
           return GenerateWorkBook(value);
        }


        private static string GenerateWorkBook(string id)
        {
            string result = "";
            try
            {
                CloudBlobContainer excelontainer;
                excelontainer = InitContainer();
                //参加者リストをデシリアイズ
                Activity eventinfo 
                    = JsonConvert.DeserializeObject<Activity>(id);
                //Blobの参照を取得
                CloudBlockBlob blob = excelontainer
                    .GetBlockBlobReference(
                    string.Format("{0}.xlsx", eventinfo.Title));
                //EXCELドキュメント生成用のMemoryStream作成
                MemoryStream ms = new MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                //参加者一覧を抽出
                var attend = eventinfo.Attendees;
                ExcelPackage package = new ExcelPackage(ms);
                //新規WorkSheetの作成
                ExcelWorksheet AttendeeList 
                    = package.Workbook.Worksheets.Add(eventinfo.Title);
                //文書見出しの作成
                AttendeeList.Cells["B1"].Value 
                    = string.Format("【{0}】参加者", eventinfo.Title);
                AttendeeList.Cells["B1"].Style.Font.Bold = true;
                AttendeeList.Cells["B1"].Style.Font.Size = 16;
                AttendeeList.Cells["A2"].Value 
                    = string.Format("開催日時：{0}", eventinfo.Schedule);
                //参加者一覧見出し行の作成
                AppendRow(AttendeeList, 3,
                    new List<string> { "No.", "氏", "名", "出欠" });
                //参加者一覧の出力
                int listIndex = 4;
                foreach (var attendee in attend)
                {
                    //参加者情報をEXCELシートに書出し
                    AppendRow(AttendeeList, listIndex,
                          new List<string>
                          {
                            (listIndex - 3).ToString(),
                            attendee.FamilyName,
                            attendee.FirstName,
                            "出 / 欠"
                          });
                    listIndex++;
                }
                package.Save();
                //MemoryStreamを先頭にSeek
                ms.Seek(0, SeekOrigin.Begin);
                //MemoryStreamの内容をBlobに書き出し
                blob.UploadFromStream(ms);
                //BlobのURLを返り値に設定
                result = blob.Uri.ToString();
            }
            catch
            {
            }
            return result;
        }

        private static void AppendRow(ExcelWorksheet AttendeeList,
            int listIndex,List<string> colitems)
        {
            for(int colindex = 1;colindex < 5;colindex++)
            {
                //セルに値を設定
                AttendeeList.Cells[listIndex, colindex].
                    Value = colitems[colindex - 1];
                //セルに罫線を引く
                AttendeeList.Cells[listIndex, colindex].
                    Style.Border.BorderAround(OfficeOpenXml.
                    Style.ExcelBorderStyle.Thin);
                //カラム幅の指定(1カラム目だけ幅を狭く)
                AttendeeList.Column(colindex).
                    Width = colindex == 1 ? 4 : 18;
            }
            //文字列の水平位置を設定
            AttendeeList.Cells[listIndex, 4].
                Style.HorizontalAlignment 
                = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        }
        private static CloudBlobContainer InitContainer()
        {
            //Blobコンテナ
            CloudBlobContainer excelontainer;
            //接続文字列からストレージアカウントを作成
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //BlobClientを作成
            CloudBlobClient blobClient 
                = storageAccount.CreateCloudBlobClient();
            //Blobコンテナの参照を取得し、存在しない場合は新規作成
            excelontainer = blobClient.GetContainerReference("excels");
            excelontainer.CreateIfNotExists();
            return excelontainer;
        }
    }
}
