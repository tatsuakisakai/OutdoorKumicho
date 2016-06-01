using Microsoft.Azure.Mobile.Server;

namespace OutdoorKumichoMobile.DataObjects
{
   
    public class KumichoActivity : EntityData
    {
        public string EventID { get; set; }
        public string Title { get; set; }
        public string PictureURL { get; set; }
        public string Description { get; set; }
        public string Schedule { get; set; }
        public string Area{ get; set; }
        public string ActivityType { get; set; }
        public string ActivityLevel { get; set; }
        public long MaxAttendees { get; set; }
        public long MinAttendees { get; set; }
        public long ActualAttendees { get; set; }
        public bool IsCanceled { get; set; }
        public bool IsComitted { get; set; }
    }

    public class ActivityAttendees : EntityData
    {
        public string EventID { get; set; }
        public string TwitterID { get; set; }
        public string FirstName { get; set; }
        public string FamilyName { get; set; }
        public bool IsCanceled { get; set; }
        public bool IsAttended { get; set; }
    }


}