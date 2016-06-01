using System.Web.Http;
using Microsoft.Azure;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
namespace OutdoorKumichoMobile.Controllers
{
    [MobileAppController,AllowAnonymous]
    public class ConfirmEventController : ApiController
    {
        // GET: api/ConfirmEvent/5
        public string Get(string id)
        {
            return SendTopicMessage(id);
        }

        private static string SendTopicMessage(string id)
        {
            //Service Bus接続文字列
            string connectionString = CloudConfigurationManager.GetSetting("ServiceBusConnectionString");
            //Topic名
            string TopicName = CloudConfigurationManager.GetSetting("CommitTopic");
            //Subscription名
            string APISubscriptionName = CloudConfigurationManager.GetSetting("APISubscription");
            string TriggerSubscriptionName = CloudConfigurationManager.GetSetting("TriggerSubscription");
            //NamespaceManagerの生成
            NamespaceManager nsMan = NamespaceManager.CreateFromConnectionString(connectionString);
            //TopicおよびSubscriptionの存在をチェックし、存在しない場合は新規作成
            if (nsMan.TopicExists(TopicName) == false)
                nsMan.CreateTopic(TopicName);
            if (nsMan.SubscriptionExists(TopicName, APISubscriptionName) == false)
                nsMan.CreateSubscription(TopicName, APISubscriptionName);
            if (nsMan.SubscriptionExists(TopicName, TriggerSubscriptionName) == false)
                nsMan.CreateSubscription(TopicName, TriggerSubscriptionName);
            //Topicクライアントの作成
            TopicClient TClient = TopicClient.CreateFromConnectionString(connectionString, TopicName);
            //Topicにメッセージを送信
            TClient.Send(new BrokeredMessage(id));
            //TopicClientをClose
            TClient.Close();
            return id;
        }
    }
}
