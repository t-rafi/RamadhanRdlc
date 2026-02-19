using System;
using System.ComponentModel.DataAnnotations;

namespace RamadanReportApp.Models
{
    public class RamadanSchedule
    {
        public int Id { get; set; }
        public DateTime RamadanDate { get; set; }
        public TimeSpan SehriTime { get; set; }
        public TimeSpan IftarTime { get; set; }
        public string HijriDate { get; set; }
    }
}
