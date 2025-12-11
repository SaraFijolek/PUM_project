using System;
using System.Collections.Generic;

namespace App1.Models
{
    public class Activity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? DistanceKm { get; set; }
        public TimeSpan? Duration => EndTime.HasValue ? EndTime - StartTime : (TimeSpan?)null;
        public string Note { get; set; }
        public string Type { get; set; } 
        public string PaceText { get; set; }
        public string SpeedText { get; set; }
        public string PhotoBase64 { get; set; }
        public List<GpsPoint> Track { get; set; } = new List<GpsPoint>();
        public int TrackCount => Track?.Count ?? 0;
    }

    public class GpsPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

