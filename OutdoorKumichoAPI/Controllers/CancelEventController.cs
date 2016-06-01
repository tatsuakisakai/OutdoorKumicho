using System;
using System.Net;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace OutdoorKumichoAPI.Controllers
{
    public class CancelEventController : ApiController
    {
        // GET api/values
        [SwaggerOperation("GetAll")]
        public string Get()
        {
            return GetCancelEventID();
        }

        private string GetCancelEventID()
        {
            string result = "";
            string connectionString
                = CloudConfigurationManager
                .GetSetting("ServiceBusConnectionString");
            string TopicName = "cancelevent";
            string subscripitionname
                = CloudConfigurationManager
                .GetSetting("DefaultSubscriptionName");
            string triggersubName
                = CloudConfigurationManager
                .GetSetting("TriggerSubscriptionName");
            NamespaceManager nsMan
                = NamespaceManager
                .CreateFromConnectionString(connectionString);
            if (nsMan.TopicExists(TopicName) == false)
                nsMan.CreateTopic(TopicName);
            if (nsMan.SubscriptionExists(TopicName,
                subscripitionname) == false)
                nsMan.CreateSubscription(TopicName, subscripitionname);
            if (nsMan.SubscriptionExists(TopicName,
                triggersubName) == false)
                nsMan.CreateSubscription(TopicName, triggersubName);
            SubscriptionClient SClient
                = SubscriptionClient.CreateFromConnectionString(
                    connectionString, TopicName, subscripitionname);
            BrokeredMessage msg
                = SClient.Receive(TimeSpan.FromSeconds(3));

            if (msg != null)
            {
                result = msg.GetBody<string>();
                msg.Complete();
            }
            else
            {
                this.StatusCode(HttpStatusCode.NotFound);
            }
            return result;
        }

    }
}
