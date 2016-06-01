using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OutdoorKumichoAPI.Models
{
    public class Activity
    {
        public string Id { get; set; }
        [JsonProperty(PropertyName = "EventID")]
        public string EventID { get; set; }
        [JsonProperty(PropertyName = "Title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "Schedule")]
        public string Schedule { get; set; }
        [JsonProperty(PropertyName = "Attendees")]
        public List<Attendees> Attendees { get; set; }
        [JsonProperty(PropertyName = "IsCanceled")]
        public bool IsCanceled { get; set; }

    }
    public class Attendees
    {
        public string Id { get; set; }
        public string TwitterID { get; set; }
        [JsonProperty(PropertyName = "FirstName")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "FamilyName")]
        public string FamilyName { get; set; }
        [JsonProperty(PropertyName = "IsCanceled")]
        public bool IsCanceled { get; set; }
        [JsonProperty(PropertyName = "IsAttended")]
        public bool IsAttended { get; set; }
    }
}
