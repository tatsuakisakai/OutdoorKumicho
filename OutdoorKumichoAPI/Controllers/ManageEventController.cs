using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using OutdoorKumichoAPI.Models;
using Newtonsoft.Json;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace OutdoorKumichoAPI.Controllers
{
    public class ManageEventController : ApiController
    {
        private NamespaceManager nsMan;
        private string TopicName;
        private string connectionString;
        private SubscriptionClient SClient;
        private string subscripitionname;
        private string triggersubName;
        // GET api/manageevent
        [SwaggerOperation("GetAll")]
        public string Get()
        {
            return  GetEventID();
        }

        // GET api/manageevent/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Get(string id)
        {
            string result = GetAttendeeList(id);
            return result;

        }

        private void InitTopic()
        {
            //Service Bus接続文字列の取得
            connectionString = CloudConfigurationManager
                .GetSetting("ServiceBusConnectionString");
            //Topic名の取得
            TopicName = CloudConfigurationManager
                .GetSetting("TopicName");
            //Subscription名の取得
            subscripitionname = CloudConfigurationManager
                .GetSetting("DefaultSubscriptionName");
            triggersubName = CloudConfigurationManager
                 .GetSetting("TriggerSubscriptionName");
            //NamespaceManagerインスタンスの作成
            nsMan = NamespaceManager
                .CreateFromConnectionString(connectionString);
            //Topicの存在をチェックし、存在しない場合は新規作成
            if (nsMan.TopicExists(TopicName) == false)
                nsMan.CreateTopic(TopicName);
           //2種類のSubscriptionの存在をチェックし、存在しない場合は新規作成
           if(nsMan.SubscriptionExists(TopicName, subscripitionname)== false)
                nsMan.CreateSubscription(TopicName, subscripitionname);
           if(nsMan.SubscriptionExists(TopicName, triggersubName) == false)
                nsMan.CreateSubscription(TopicName, triggersubName);
        }

        private string GetEventID()
        {
            string result = "";
            InitTopic(); //Topicの初期化
            //Subscription Clientインスタンスの作成
            SClient = SubscriptionClient.
                CreateFromConnectionString(connectionString, 
                TopicName, subscripitionname);
            //Subscriptionからメッセージを受信
            BrokeredMessage msg 
                = SClient.Receive(TimeSpan.FromSeconds(3));
            if (msg != null)
            {
                //メッセージを取得できた場合、本文を戻り値に設定
                result = msg.GetBody<string>();
                msg.Complete();
            }
            else
            {
                //取得できない場合、404を返す
                this.StatusCode(HttpStatusCode.NotFound);
                result = "0000";
            }

            return result;
        }
        private string GetAttendeeList(string eventid)
        {
            string resultstr = "";
            KumichoModel dbmodel = new KumichoModel();
            try
            {
                //EventIDに合致するイベントを読み込み
                var currentevent 
                    = dbmodel.KumichoActivities
                    .Where(c => c.EventID == eventid);
                //EventIDに合致する参加者一覧を取得
                var attend = dbmodel.ActivityAttendees
                    .Where(d => d.EventID == eventid && d.IsCanceled == false);
                if (currentevent.Count() == 0)
                {
                    return "";
                }
                else
                {
                    //Tableから読み込まれた情報を基にエンティティを作成
                    var eventinfo = currentevent.First();
                    List<Attendees> attendees = new List<Attendees>();
                    if (attend.Count() != 0)
                    {
                        //参加者リストを作成
                        foreach (var attendee in attend)
                        {
                            attendees.Add(new Attendees
                            {
                                TwitterID = attendee.TwitterID,
                                FamilyName = attendee.FamilyName,
                                FirstName = attendee.FirstName,
                                IsCanceled = attendee.IsCanceled,
                                IsAttended = attendee.IsAttended
                            });
                        }
                    }
                    //イベント情報を作成
                    Activity currentactivity = new Activity
                    {
                        Id = eventinfo.Id,
                        EventID = eventinfo.EventID,
                        Title = eventinfo.Title,
                        Schedule = eventinfo.Schedule,
                        Attendees = attendees,
                        IsCanceled = eventinfo.IsCanceled

                    };
                    //Jsonシリアライズし文字列化
                    resultstr 
                        = JsonConvert.SerializeObject(currentactivity);
                }
            }
            catch { }
            return resultstr;
        }
    }
}
