using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentinel;

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
        public DateTime LastMotionData { get; set; }

        public bool IsOffline { get; set; } = false;

        public string Status => IsOffline ? "OFFLINE" : "ONLINE";

        // 🔽 Veri davranış analizi için zaman listeleri
        public List<DateTime> PingTimestamps { get; } = new();
        public List<DateTime> HeatTimestamps { get; } = new();
        public List<DateTime> BatteryTimestamps { get; } = new();
        public List<DateTime> CameraTimestamps { get; } = new();
        public List<DateTime> AlarmTimestamps { get; } = new();
        public List<DateTime> FingerprintTimestamps { get; } = new();
        public List<DateTime> MotionTimestamps { get; } = new(); // opsiyonel olarak eklendi

        // 🔽 Eksik olan bu: Veri davranış profili tanımı
        public DeviceBehaviorProfile ExpectedBehavior { get; set; } = new DeviceBehaviorProfile
        {
            ExpectedPingInterval = 25,
            ExpectedHeatInterval = 50,
            ExpectedBatteryInterval = 150,
            ExpectedCameraInterval = 15,
            ExpectedAlarmInterval = 50,
            ExpectedFingerprintInterval = 60,
            ExpectedMotionInterval = 60
        };

        public class DeviceBehaviorProfile
        {
            public double ExpectedPingInterval { get; set; }
            public double ExpectedHeatInterval { get; set; }
            public double ExpectedBatteryInterval { get; set; }
            public double ExpectedCameraInterval { get; set; }
            public double ExpectedAlarmInterval { get; set; }
            public double ExpectedFingerprintInterval { get; set; }
            public double ExpectedMotionInterval { get; set; }
        }
    }

}
