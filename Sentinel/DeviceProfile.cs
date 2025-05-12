using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentinel
{
    public class DeviceProfile
    {
        public string DeviceId { get; set; }
        public string Ip { get; set; }
        public string DeviceType { get; set; }

        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }

        public DateTime LastHeat { get; set; }
        public DateTime LastPing { get; set; }
        public DateTime LastBattery { get; set; }

        public DateTime LastCameraData { get; set; }
        public DateTime LastAlarmData { get; set; }
        public DateTime LastFingerprintData { get; set; }

        public bool IsOffline { get; set; } = false;

        public string Status => IsOffline ? "OFFLINE" : "ONLINE";
    }
}
