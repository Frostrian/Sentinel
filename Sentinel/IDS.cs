using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentinel
{
    public static class IDS
    {
        public static List<string> Alerts = new();

        public static void Analyze(DeviceProfile profile, string topic, string payload)
        {
            var now = DateTime.Now;
            var lastTime = profile.LastMessageTime;
            profile.LastMessageTime = now;

            var delay = (now - lastTime).TotalSeconds;

            if (delay < 0.5)
                AddAlert(profile.DeviceId, "Veri çok sık geliyor", topic);

            if (topic.Contains("ping") && delay > 60)
                AddAlert(profile.DeviceId, "Ping kaybı - uzun süre cevap alınmadı", topic);

            if (topic.Contains("camera") && !profile.DeviceType.Contains("camera"))
                AddAlert(profile.DeviceId, "Kamera verisi alınıyor ama cihaz tipi farklı!", topic);
        }

        private static void AddAlert(string deviceId, string reason, string topic)
        {
            var message = $"⚠ IDS Uyarısı: {deviceId} - {reason} - [{topic}]";
            Alerts.Add(message);
            Console.WriteLine(message);
        }
    }
}
