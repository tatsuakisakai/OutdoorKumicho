using System.Linq;
using System.Net;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using OutdoorKumichoAPI.Models;
using Microsoft.Azure;
using Microsoft.Azure.NotificationHubs;


namespace OutdoorKumichoAPI.Controllers
{
    public class TweetMessageController : ApiController
    {
        // GET api/tweetmessage?id=5;stat=confirm
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK,type: typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Get(string id, string stat)
        {
            return CreateMessage(id, stat);

        }

        private string CreateMessage(string id, string stat)
        {
            string messagetext;
            string notifytext = "イベントに関する情報があります。";
            //イベント情報をDBより取得
            KumichoModel dbmodel = new KumichoModel();
            var result = dbmodel.KumichoActivities.Where(c => c.EventID == id);
            if (result.Count() == 0)
            {
                messagetext = "イベントが確定またはキャンセルになりました。";
            }
            else
            {
                var eventinfo = result.First();
                if (stat == "cancel")
                {
                    notifytext 
                        = string.Format("イベント【{0}】は中止になりました。ご注意ください。",
                        eventinfo.Title); ;
                    messagetext = notifytext;
                    eventinfo.IsCanceled = true;
                    dbmodel.SaveChanges();
                }
                else
                {
                    notifytext 
                        = string.Format("イベント【{0}】は予定通り開催します。",
                        eventinfo.Title);
                    messagetext = notifytext + "参加者の方宛には通知をお送りしました。";
                    eventinfo.IsComitted = true;
                    dbmodel.SaveChanges();


                }
            }
            SendNotify(id, notifytext);

            return messagetext;
        }

        private void SendNotify(string id, string message)
        {
            //Notification Hub接続文字列からNoticifcationHubClientを作成
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(
                CloudConfigurationManager
                .GetSetting("NotificationConnectionString"),
                CloudConfigurationManager
                .GetSetting("NotificationHubName"));
            //Notification Hubで利用する通知メッセージの作成
            var toast 
                = string.Format(@"<toast><visual><binding template=""ToastText01""><text id=""1"">{0}</text></binding></visual></toast>",
                message);
            //メッセージの送信
            hub.SendWindowsNativeNotificationAsync(toast, id);
        }
    }
}
