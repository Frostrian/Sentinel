using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

        private List<DeviceProfile> RegisteredDevices = new();

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
            IDS.SetProfileSource(() => RegisteredDevices);
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
                var authReq = System.Text.Json.JsonSerializer.Deserialize<AuthRequest>(json);
                logAction($"🔐 Auth isteği: {authReq.deviceId} / {authReq.ip}");

                var ipConflict = RegisteredDevices
                    .FirstOrDefault(d => d.Ip == authReq.ip && d.DeviceId != authReq.deviceId);

                if (ipConflict != null)
                {
                    logAction($"🛑 IP ÇAKIŞMASI: {authReq.deviceId} ile {ipConflict.DeviceId} aynı IP ({authReq.ip}) kullanıyor!");

                    // Tercihe bağlı: IDS'e de loglatılabilir
                    IDS.AddExternalAlert($"🛑 IP ÇAKIŞMASI: {authReq.deviceId} ile {ipConflict.DeviceId} aynı IP ({authReq.ip})");
                }

                var profile = new DeviceProfile
                {
                    DeviceId = authReq.deviceId,
                    Ip = authReq.ip,
                    DeviceType = GuessDeviceType(authReq.deviceId),
                    FirstSeen = DateTime.Now,
                    LastPing = DateTime.Now
                };

                RegisteredDevices.Add(profile);

                var responseTopic = $"auth/{authReq.deviceId}/response";
                var responseJson = "{\"status\":\"ok\",\"reason\":\"verified\"}";

                client.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic(responseTopic)
                    .WithPayload(responseJson)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());

                onDeviceAuth(profile);
            }
            catch (Exception ex)
            {
                logAction("❌ Auth parse hatası: " + ex.Message);
            }
        }


        private void HandleData(string topic, string payload)
        {
            var deviceId = ExtractDeviceId(topic);
            var profile = RegisteredDevices.FirstOrDefault(x => x.DeviceId == deviceId);
            if (profile != null)
            {
                IDS.Analyze(profile, topic, payload);
                logAction($"📡 {deviceId} verisi alındı: {topic}");

                uiDispatcher(() =>
                {
                    onDataReceived(topic, payload);
                    RefreshIDSAlerts();
                });

                profile.LastSeen = DateTime.Now;

                // Konu bazlı zaman güncellemeleri
                if (topic.Contains("heat")) profile.LastHeat = DateTime.Now;
                else if (topic.Contains("ping")) profile.LastPing = DateTime.Now;
                else if (topic.Contains("battery")) profile.LastBattery = DateTime.Now;
                else if (topic.Contains("frame") || topic.Contains("status")) profile.LastCameraData = DateTime.Now;
                else if (topic.Contains("alarm")) profile.LastAlarmData = DateTime.Now;
                else if (topic.Contains("access")) profile.LastFingerprintData = DateTime.Now;
                else if (topic.Contains("motion")) profile.LastMotionData = DateTime.Now;
            }
        }

        private string ExtractDeviceId(string topic)
        {
            var parts = topic.Split('/');
            return parts.Length > 1 ? parts[1] : "UNKNOWN";
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