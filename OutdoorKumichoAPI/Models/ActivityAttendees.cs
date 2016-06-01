namespace OutdoorKumichoAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ActivityAttendees
    {
        public string Id { get; set; }

        public string EventID { get; set; }

        public string TwitterID { get; set; }

        public string FirstName { get; set; }

        public string FamilyName { get; set; }

        public bool IsCanceled { get; set; }

        public bool IsAttended { get; set; }

        [Column(TypeName = "timestamp")]
        [MaxLength(8)]
        [Timestamp]
        public byte[] Version { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool Deleted { get; set; }
    }
}
