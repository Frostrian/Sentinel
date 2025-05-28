using Sentinel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class IDS
{
    public static List<string> Alerts = new();

    private static Func<List<DeviceProfile>> profileSource;
    private static ListBox blacklistUI;
    
    private static Action<Action> uiDispatcher;

    public static void SetProfileSource(Func<List<DeviceProfile>> getter)
    {
        profileSource = getter;
    }

    public static void SetBlacklistUI(ListBox uiListBox, Action<Action> dispatcher)
    {
        blacklistUI = uiListBox;
        uiDispatcher = dispatcher;
    }

    private static Dictionary<string, DateTime> lastPing = new();
    private static Dictionary<string, DateTime> lastHeat = new();
    private static Dictionary<string, DateTime> lastBattery = new();
    private static Dictionary<string, DateTime> lastTopicGeneral = new();

    private static HashSet<string> IgnoreDevices = new();

    public static void IgnoreDevice(string deviceId)
    {
        if (!IgnoreDevices.Contains(deviceId))
        {
            IgnoreDevices.Add(deviceId);
            AddAlert(deviceId, $"🚫 Cihaz IDS tarafından kara listeye alındı (Flood tespiti)", "SYSTEM");

            if (blacklistUI != null && uiDispatcher != null)
            {
                uiDispatcher?.Invoke(() =>
                {
                    if (!blacklistUI.Items.Contains(deviceId))
                        blacklistUI.Items.Add(deviceId);
                });
            }
        }
    }

    public static void ResetIgnore(string deviceId)
    {
        if (IgnoreDevices.Contains(deviceId))
            IgnoreDevices.Remove(deviceId);

        if (blacklistUI != null && uiDispatcher != null)
        {
            uiDispatcher(() => {
                if (blacklistUI.Items.Contains(deviceId))
                    blacklistUI.Items.Remove(deviceId);
            });
        }
    }
    private static ListBox alertListUI;

    public static void Initialize(ListBox alertListBox, Action<Action> dispatcher)
    {
        alertListUI = alertListBox;
        uiDispatcher = dispatcher;
    }
 

    private static void AddAlert(string deviceId, string reason, string topic)
    {
        var message = $"⚠ IDS Uyarısı: {deviceId} - {reason} - [{topic}]";
        Alerts.Add(message);

        IDSLogger.Log(deviceId, reason, topic);
    }

    public static void Analyze(DeviceProfile profile, string topic, string payload)
    {
        var now = DateTime.Now;
        string deviceId = profile.DeviceId;

        if (IgnoreDevices.Contains(deviceId))
            return;

        // Payload içinde sıcaklık varsa kontrol et
        if (TryExtract(payload, "temperature", out double t))
        {
            if (t < -50 || t > 100)
                AddAlert(deviceId, $"‼ Şüpheli sıcaklık verisi tespit edildi: {t}°C", topic);
        }

        // Payload içinde batarya varsa kontrol et
        if (TryExtract(payload, "battery", out double b))
        {
            if (b < 0 || b > 100)
                AddAlert(deviceId, $"‼ Geçersiz batarya verisi: {b}%", topic);
        }

        // Geri kalan kontroller
        AnalyzeBehavior(profile, topic);

        if ((now - profile.TrafficWindowStart).TotalSeconds > 60)
        {
            profile.TrafficWindowStart = now;
            profile.MessageCountLastMinute = 0;
        }
        profile.MessageCountLastMinute++;

        if (profile.MessageCountLastMinute > 100)
        {
            AddAlert(deviceId, $"Aşırı trafik: Son 60 sn içinde {profile.MessageCountLastMinute} mesaj", topic);
        }

        if (profile.MessageCountLastMinute > 20)
        {
            IgnoreDevice(deviceId);
            return;
        }

        if (topic.Contains("ping"))
        {
            if (lastPing.TryGetValue(deviceId, out var last))
                if ((now - last).TotalSeconds < 10)
                    AddAlert(deviceId, "Ping çok sık geliyor", topic);
            lastPing[deviceId] = now;
        }

        if (topic.Contains("heat"))
        {
            if (lastHeat.TryGetValue(deviceId, out var last))
                if ((now - last).TotalSeconds < 20)
                    AddAlert(deviceId, "Isı verisi çok sık geliyor", topic);
            lastHeat[deviceId] = now;

            if (TryExtract(payload, "temperature", out double temp) && temp > 60)
                AddAlert(deviceId, $"Sıcaklık çok yüksek: {temp}°C", topic);
        }

        if (topic.Contains("battery"))
        {
            if (lastBattery.TryGetValue(deviceId, out var last))
                if ((now - last).TotalSeconds < 30)
                    AddAlert(deviceId, "Batarya verisi çok sık geliyor", topic);
            lastBattery[deviceId] = now;

            if (TryExtract(payload, "battery", out double battery) && battery < 15)
                AddAlert(deviceId, $"Düşük pil seviyesi: {battery}%", topic);
        }

        if (topic.Contains("camera") && !profile.DeviceType.Contains("camera"))
            AddAlert(deviceId, "Kamera verisi alınıyor ama cihaz tipi farklı!", topic);

        if (profileSource != null)
        {
            var others = profileSource.Invoke();

            var sameIdProfiles = others.Where(p => p.DeviceId == deviceId).ToList();
            if (sameIdProfiles.Count > 1)
            {
                var distinctIps = sameIdProfiles.Select(p => p.Ip).Distinct().ToList();
                if (distinctIps.Count > 1)
                {
                    AddAlert(deviceId, $"🛑 SPOOF TESPİTİ: {deviceId} farklı IP'lerden ({string.Join(", ", distinctIps)}) geliyor!", topic);
                }
            }

            var conflict = others.Any(p => p.Ip == profile.Ip && p.DeviceId != deviceId);
            if (conflict)
            {
                AddAlert(deviceId, $"⚠ IP ÇAKIŞMASI: IP ({profile.Ip}) başka cihaz tarafından da kullanılıyor!", topic);
            }
        }

        string key = $"{deviceId}:{topic}";
        if (lastTopicGeneral.TryGetValue(key, out var prevTime))
            if ((now - prevTime).TotalMilliseconds < 300)
                AddAlert(deviceId, "Aynı veri çok kısa sürede tekrarlandı", topic);
        lastTopicGeneral[key] = now;

        if (TryExtract(payload, "ip", out string extractedIp))
        {
            if (!string.IsNullOrEmpty(extractedIp) && extractedIp != profile.Ip)
            {
                AddAlert(deviceId, $"🛑 SPOOF TESPİTİ (Payload IP uyuşmazlığı): {deviceId} farklı IP ({extractedIp}) üzerinden veri gönderiyor! Kayıtlı IP: {profile.Ip}", topic);
            }
        }
    }

    private static bool TryExtract(string json, string key, out double value)
    {
        value = 0;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(key, out var prop))
                return prop.TryGetDouble(out value);
        }
        catch { }
        return false;
    }

    private static bool TryExtract(string json, string key, out string value)
    {
        value = null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(key, out var prop))
            {
                value = prop.GetString();
                return true;
            }
        }
        catch { }
        return false;
    }

    public static void AddExternalAlert(string msg)
    {
        Alerts.Add(msg);

        if (uiDispatcher != null && alertListUI != null)
        {
            uiDispatcher?.Invoke(() => alertListUI.Items.Add(msg));
        }
    }
    public static void RegisterUnknownDevice(string deviceId, string topic)
    {
        string reason = $"⚠ Tanımsız cihazdan veri geldi: {deviceId}";
        AddExternalAlert(reason);
        IDSLogger.Log(deviceId, reason, topic);
    }
    private static void AnalyzeBehavior(DeviceProfile profile, string topic)
    {
        void Check(List<DateTime> times, double expectedSeconds, string label)
        {
            if (times.Count < 3) return;

            var intervals = new List<double>();
            for (int i = 1; i < times.Count; i++)
                intervals.Add((times[i] - times[i - 1]).TotalSeconds);

            var avg = intervals.Average();
            if (Math.Abs(avg - expectedSeconds) > expectedSeconds * 0.4)
            {
                AddAlert(profile.DeviceId, $"{label} veri sıklığı beklenen dışı: ort {avg:F1}s (beklenen {expectedSeconds}s)", topic);
            }

            while (times.Count > 10)
                times.RemoveAt(0);
        }

        if (topic.Contains("heat"))
        {
            profile.HeatTimestamps.Add(DateTime.Now);
            Check(profile.HeatTimestamps, profile.ExpectedBehavior.ExpectedHeatInterval, "Isı");
        }
        if (topic.Contains("ping"))
        {
            profile.PingTimestamps.Add(DateTime.Now);
            Check(profile.PingTimestamps, profile.ExpectedBehavior.ExpectedPingInterval, "Ping");
        }
        if (topic.Contains("battery"))
        {
            profile.BatteryTimestamps.Add(DateTime.Now);
            Check(profile.BatteryTimestamps, profile.ExpectedBehavior.ExpectedBatteryInterval, "Batarya");
        }
        if (topic.Contains("frame") || topic.Contains("status"))
        {
            profile.CameraTimestamps.Add(DateTime.Now);
            Check(profile.CameraTimestamps, profile.ExpectedBehavior.ExpectedCameraInterval, "Kamera");
        }
        if (topic.Contains("access"))
        {
            profile.FingerprintTimestamps.Add(DateTime.Now);
            Check(profile.FingerprintTimestamps, profile.ExpectedBehavior.ExpectedFingerprintInterval, "Parmak izi");
        }
        if (topic.Contains("alarm"))
        {
            profile.AlarmTimestamps.Add(DateTime.Now);
            Check(profile.AlarmTimestamps, profile.ExpectedBehavior.ExpectedAlarmInterval, "Alarm");
        }
    }
}
