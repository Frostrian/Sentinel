using Sentinel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class IDS
{
    public static List<string> Alerts = new();

    private static Func<List<DeviceProfile>> profileSource;

    public static void SetProfileSource(Func<List<DeviceProfile>> getter)
    {
        profileSource = getter;
    }

    private static Dictionary<string, DateTime> lastPing = new();
    private static Dictionary<string, DateTime> lastHeat = new();
    private static Dictionary<string, DateTime> lastBattery = new();
    private static Dictionary<string, DateTime> lastTopicGeneral = new();

    public static void Analyze(DeviceProfile profile, string topic, string payload)
    {
        var now = DateTime.Now;
        string deviceId = profile.DeviceId;

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
            var conflict = others.Any(p => p.Ip == profile.Ip && p.DeviceId != deviceId);
            if (conflict)
                AddAlert(deviceId, $"Aynı IP ({profile.Ip}) başka cihaz tarafından da kullanılıyor!", topic);
        }

        string key = $"{deviceId}:{topic}";
        if (lastTopicGeneral.TryGetValue(key, out var prevTime))
            if ((now - prevTime).TotalMilliseconds < 300)
                AddAlert(deviceId, "Aynı veri çok kısa sürede tekrarlandı", topic);
        lastTopicGeneral[key] = now;
    }

    public static void AddExternalAlert(string msg)
    {
        Alerts.Add(msg);
        Console.WriteLine(msg);
    }

    private static void AddAlert(string deviceId, string reason, string topic)
    {
        var message = $"⚠ IDS Uyarısı: {deviceId} - {reason} - [{topic}]";
        Alerts.Add(message);
        Console.WriteLine(message);
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
}