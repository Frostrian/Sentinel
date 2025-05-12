using Sentinel;
using System;
using System.Collections.Generic;
using System.Linq;

public class DeviceTracker
{
    private Dictionary<string, DeviceProfile> _profiles = new Dictionary<string, DeviceProfile>();

    public IReadOnlyCollection<DeviceProfile> Profiles => _profiles.Values;

    public void UpdateOrCreate(string deviceId, string ip, string deviceType, string topic)
    {
        if (!_profiles.TryGetValue(deviceId, out var profile))
        {
            profile = new DeviceProfile
            {
                DeviceId = deviceId,
                Ip = ip,
                DeviceType = deviceType,
                FirstSeen = DateTime.Now
            };
            _profiles[deviceId] = profile;
        }

        profile.LastSeen = DateTime.Now;
        profile.Ip = ip;
        profile.DeviceType = deviceType;
        profile.IsOffline = false;

        // Topic bazlı zaman etiketleri
        if (topic.Contains("heat"))
            profile.LastHeat = DateTime.Now;
        else if (topic.Contains("ping"))
            profile.LastPing = DateTime.Now;
        else if (topic.Contains("battery"))
            profile.LastBattery = DateTime.Now;
        else if (topic.Contains("frame") || topic.Contains("status"))
            profile.LastCameraData = DateTime.Now;
        else if (topic.Contains("access"))
            profile.LastFingerprintData = DateTime.Now;
        else if (topic.Contains("alarm"))
            profile.LastAlarmData = DateTime.Now;

        var ipConflict = _profiles.Values.FirstOrDefault(p => p.Ip == ip && p.DeviceId != deviceId);
        if (ipConflict != null)
        {
            // IDS uyarısı: IP adresi farklı cihazlar tarafından kullanılıyor
            Log($"🛑 IDS: IP çakışması! {deviceId} ve {ipConflict.DeviceId} aynı IP ({ip}) ile veri gönderiyor.");
        }
    }

    public void CheckTimeouts(Action<DeviceProfile> onTimeout, int timeoutSeconds = 120)
    {
        foreach (var profile in _profiles.Values)
        {
            var elapsed = (DateTime.Now - profile.LastSeen).TotalSeconds;
            if (elapsed > timeoutSeconds && !profile.IsOffline)
            {
                profile.IsOffline = true;
                onTimeout?.Invoke(profile);
            }
        }
    }
}