namespace OutdoorKumichoAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class KumichoActivities
    {
        public string Id { get; set; }

        public string EventID { get; set; }

        public string Title { get; set; }

        public string PictureURL { get; set; }

        public string Description { get; set; }

        public string Schedule { get; set; }

        public string Area { get; set; }

        public string ActivityType { get; set; }

        public string ActivityLevel { get; set; }

        public long MaxAttendees { get; set; }

        public long MinAttendees { get; set; }

        public long ActualAttendees { get; set; }

        public bool IsCanceled { get; set; }

        public bool IsComitted { get; set; }

        [Column(TypeName = "timestamp")]
        [MaxLength(8)]
        [Timestamp]
        public byte[] Version { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool Deleted { get; set; }
    }
}
