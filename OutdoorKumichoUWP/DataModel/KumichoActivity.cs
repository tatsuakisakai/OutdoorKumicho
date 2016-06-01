using Newtonsoft.Json;

namespace outdoorkumicho
{
    public class KumichoActivity
    {
        public string Id { get; set; }
        [JsonProperty(PropertyName = "EventID")]
        public string EventID { get; set; }

        [JsonProperty(PropertyName = "Title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "PictureURL")]
        public string PictureURL { get; set; }
        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "Schedule")]
        public string Schedule { get; set; }
        [JsonProperty(PropertyName = "Area")]
        public string Area { get; set; }
        [JsonProperty(PropertyName = "ActivityType")]
        public string ActivityType { get; set; }
        [JsonProperty(PropertyName = "ActivityLevel")]
        public string ActivityLevel { get; set; }
        [JsonProperty(PropertyName = "MaxAttendees")]
        public long MaxAttendees { get; set; }
        [JsonProperty(PropertyName = "MinAttendees")]
        public long MinAttendees { get; set; }
        [JsonProperty(PropertyName = "ActualAttendees")]
        public long ActualAttendees { get; set; }

        [JsonProperty(PropertyName = "IsCanceled")]
        public bool IsCanceled { get; set; }
        [JsonProperty(PropertyName = "IsComitted")]
        public bool IsComitted { get; set; }

    }
    public class ActivityAttendees
    {
        public string Id { get; set; }
        [JsonProperty(PropertyName = "EventID")]
        public string EventID { get; set; }

        [JsonProperty(PropertyName = "TwitterID")]
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
