using System;
using System.Linq;

public class SentinelProfileManager
{
    private readonly DeviceTracker tracker;
    private readonly Action<string> logAction;

    public SentinelProfileManager(DeviceTracker tracker, Action<string> logCallback)
    {
        this.tracker = tracker;
        this.logAction = logCallback;
    }

    public void UpdateProfile(string deviceId, string ip, string deviceType, string topic)
    {
        // IP conflict control
        var ipConflict = tracker.Profiles.FirstOrDefault(p => p.Ip == ip && p.DeviceId != deviceId);
        if (ipConflict != null)
        {
            logAction($"🛑 IDS: IP çakışması! {deviceId} ve {ipConflict.DeviceId} aynı IP ({ip}) ile veri gönderiyor.");
        }

        tracker.UpdateOrCreate(deviceId, ip, deviceType, topic);
    }

    public void CheckTimeoutsAndSilence(int offlineTimeout = 120)
    {
        tracker.CheckTimeouts(profile =>
        {
            logAction($"⚠ {profile.DeviceId} bağlantısı kesildi (Timeout {offlineTimeout} sn)");
        }, offlineTimeout);

        foreach (var profile in tracker.Profiles)
        {
            if (!profile.IsOffline)
            {
                if (profile.DeviceType.Contains("Heat"))
                {
                    var sincePing = (DateTime.Now - profile.LastPing).TotalSeconds;
                    var sinceHeat = (DateTime.Now - profile.LastHeat).TotalSeconds;

                    if (sincePing > 60)
                        logAction($"⚠ {profile.DeviceId} ping bilgisi gelmedi (60+ sn)");

                    if (sinceHeat > 150)
                        logAction($"⚠ {profile.DeviceId} ısı bilgisi alınamadı (150+ sn)");
                }

                if (profile.DeviceType.Contains("Alarm"))
                {
                    var sinceAlarm = (DateTime.Now - profile.LastAlarmData).TotalSeconds;
                    if (sinceAlarm > 120)
                        logAction($"⚠ {profile.DeviceId} alarm durumu verisi alınamadı (120+ sn)");
                }

                if (profile.DeviceType.Contains("Fingerprint"))
                {
                    var sinceTouch = (DateTime.Now - profile.LastFingerprintData).TotalSeconds;
                    if (sinceTouch > 180)
                        logAction($"⚠ {profile.DeviceId} parmak izi erişimi gelmedi (180+ sn)");
                }

                if (profile.DeviceType.Contains("Camera"))
                {
                    var sinceFrame = (DateTime.Now - profile.LastCameraData).TotalSeconds;
                    if (sinceFrame > 60)
                        logAction($"⚠ {profile.DeviceId} kamera görüntüsü alınamadı (60+ sn)");
                }
            }
        }
    }
}