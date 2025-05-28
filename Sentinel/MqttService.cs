using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Sentinel;

namespace Sentinel
{
    public class MqttService
    {
        private IMqttClient client;
        private MqttFactory factory;
        private Action<string> logAction;
        private Action<DeviceProfile> onDeviceAuth;
        private Action<string, string> onDataReceived;
        private Action RefreshIDSAlerts;
        private Action<Action> uiDispatcher;

        private Dictionary<string, DeviceProfile> profileLookup = new(); // deviceId -> profile

        public MqttService(
            Action<string> logCallback,
            Action<DeviceProfile> deviceAuthCallback,
            Action<string, string> dataCallback,
            Action<Action> uiSync,
            Action refreshIDSCallback)
        {
            logAction = logCallback;
            onDeviceAuth = deviceAuthCallback;
            onDataReceived = dataCallback;
            uiDispatcher = uiSync;
            RefreshIDSAlerts = refreshIDSCallback;
            factory = new MqttFactory();

            // IDS sistemine kaynak tanımla
            IDS.SetProfileSource(() => profileLookup.Values.ToList());
        }

        public async Task ConnectAsync()
        {
            client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("SentinelController")
                .WithTcpServer("localhost", 1883)
                .Build();

            client.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                if (topic == "auth/request")
                    HandleAuthRequest(payload);
                else
                    HandleData(topic, payload);

                await Task.CompletedTask;
            };

            await client.ConnectAsync(options);
            await client.SubscribeAsync("#");
            logAction("MQTT bağlantısı kuruldu. Tüm topic'ler dinleniyor.");
        }

        private void HandleAuthRequest(string json)
        {
            try
            {

                var authReq = JsonSerializer.Deserialize<AuthRequest>(json);
                logAction($"🔐 Auth isteği: {authReq.deviceId} / {authReq.ip}");

                if (profileLookup.TryGetValue(authReq.deviceId, out var existingProfile))
                {
                    if (existingProfile.Ip != authReq.ip)
                    {
                        IDS.AddExternalAlert($"🛑 SPOOF TESPİTİ: {authReq.deviceId} farklı IP ({authReq.ip}) üzerinden doğrulama gönderdi! Önceki IP: {existingProfile.Ip}");
                    }
                    return; // zaten kayıtlıysa ekleme
                }

                var profile = new DeviceProfile
                {
                    DeviceId = authReq.deviceId,
                    Ip = authReq.ip,
                    DeviceType = GuessDeviceType(authReq.deviceId),
                    FirstSeen = DateTime.Now,
                    LastPing = DateTime.Now
                };

                profileLookup[authReq.deviceId] = profile;

                var responseTopic = $"auth/{authReq.deviceId}/response";
                var responseJson = "{\"status\":\"ok\",\"reason\":\"verified\"}";

                client.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic(responseTopic)
                    .WithPayload(responseJson)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());

                onDeviceAuth(profile); // UI tarafına gönder

            
                
                //var existing = ExtractDeviceId(profile.DeviceId);
                //if (existing != null && existing.Ip != authReq.ip)
                //{
                //    IDS.AddExternalAlert($"🛑 SPOOF AUTH: {authReq.deviceId} daha önce {existing.Ip} ile kaydolmuştu, şimdi {authReq.ip}");
                //}
            }
            catch (Exception ex)
            {
                logAction("❌ Auth parse hatası: " + ex.Message);
            }
        }

        private void HandleData(string topic, string payload)
        {
            var deviceId = ExtractDeviceId(topic);
            if (!profileLookup.TryGetValue(deviceId, out var profile))
            {
                string reason = $"⚠ Tanımsız cihazdan veri geldi: {deviceId} - [{topic}]";
                IDS.RegisterUnknownDevice(deviceId, topic);
                return;
            }

            string incomingIp = ExtractedIpFromPayloadOrTopic(payload);
            if (!string.IsNullOrEmpty(incomingIp) && incomingIp != profile.Ip)
            {
                IDS.AddExternalAlert($"🛑 SPOOF TESPİTİ: {deviceId} farklı IP ({incomingIp}) üzerinden veri gönderiyor! Kayıtlı IP: {profile.Ip}");
            }

            // IDS Analizi
            IDS.Analyze(profile, topic, payload);
            logAction($"📡 {deviceId} verisi alındı: {topic}");

            // UI Update
            uiDispatcher(() =>
            {
                onDataReceived(topic, payload);
                RefreshIDSAlerts();
            });

            profile.LastSeen = DateTime.Now;

            // Konuya göre zaman damgası güncelle
            if (topic.Contains("heat")) profile.LastHeat = DateTime.Now;
            else if (topic.Contains("ping")) profile.LastPing = DateTime.Now;
            else if (topic.Contains("battery")) profile.LastBattery = DateTime.Now;
            else if (topic.Contains("frame") || topic.Contains("status")) profile.LastCameraData = DateTime.Now;
            else if (topic.Contains("alarm")) profile.LastAlarmData = DateTime.Now;
            else if (topic.Contains("access")) profile.LastFingerprintData = DateTime.Now;
            else if (topic.Contains("motion")) profile.LastMotionData = DateTime.Now;
        }

        private string ExtractDeviceId(string topic)
        {
            var parts = topic.Split('/');
            return parts.Length > 1 ? parts[1] : "UNKNOWN";
        }

        private string ExtractedIpFromPayloadOrTopic(string payload)
        {
            try
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("ip", out var ipProp))
                    return ipProp.GetString();
            }
            catch { }
            return null;
        }

        private string GuessDeviceType(string deviceId)
        {
            if (deviceId.StartsWith("FP")) return "fingerprint";
            if (deviceId.StartsWith("CAM")) return "camera";
            if (deviceId.StartsWith("ALARM")) return "alarm";
            return "sensor";
        }

        private class AuthRequest
        {
            public string deviceId { get; set; }
            public string ip { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }
    }
}