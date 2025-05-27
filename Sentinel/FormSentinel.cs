using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sentinel
{
    public partial class FormSentinel : Form
    {
        private List<DeviceProfile> devices = new();
        private List<DeviceTracker> deviceTracker = new();
        private Dictionary<string, DeviceProfile> profileLookup = new();
        private MqttService mqtt;
        public FormSentinel()
        {
            InitializeComponent();
            mqtt = new MqttService(Log, AddDevice, HandleDataDisplay, action => this.Invoke(action), RefreshIDSAlerts);
            Load += async (s, e) => await mqtt.ConnectAsync();
        }

        //SentinelProfileManager profileManager = new SentinelProfileManager(deviceTracker, Log);

        private void FormSentinel_Load(object sender, EventArgs e)
        {

        }

 

        public void Log(string msg)
        {
            txtLog.Invoke((MethodInvoker)(() =>
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {msg} \n");
            }));
        }

        private void AddDevice(DeviceProfile device)
        {
            devices.Add(device);
            profileLookup[device.DeviceId] = device;
            listBoxDevices.Invoke((MethodInvoker)(() =>
            {
                listBoxDevices.Items.Add($"{device.DeviceId} | {device.Ip} | {device.DeviceType}");
            }));
        }
        private string ExtractDeviceId(string topic)
        {
            var parts = topic.Split('/');
            return parts.Length > 1 ? parts[1] : "UNKNOWN";
        }

        private void HandleDataDisplay(string topic, string payload)
        {
            string entry = $"{DateTime.Now:HH:mm:ss} → {topic} | {payload}";
            string deviceId = ExtractDeviceId(topic);

            // 🔁 Motion önce kontrol edilmeli!
            if (topic.Contains("motion"))
            {
                listBoxAlarmData.Items.Add(entry); 
                TrimList(listBoxAlarmData);

                if (profileLookup.TryGetValue(deviceId, out var profile))
                    labelAlarmInfo.Text = $"Hareket: {GetIntervalInfo(profile.MotionTimestamps)}";
            }
            else if (topic.StartsWith("sensor/"))
            {
                listBoxSensorData.Items.Add(entry);
                TrimList(listBoxSensorData);

                if (profileLookup.TryGetValue(deviceId, out var profile))
                    labelSensorInfo.Text = $"Isı: {GetIntervalInfo(profile.HeatTimestamps)}";
            }
            else if (topic.StartsWith("camera/"))
            {
                listBoxCameraData.Items.Add(entry);
                TrimList(listBoxCameraData);

                if (profileLookup.TryGetValue(deviceId, out var profile))
                    labelCameraInfo.Text = $"Kamera: {GetIntervalInfo(profile.CameraTimestamps)}";
            }
            else if (topic.StartsWith("alarm/"))
            {
                listBoxAlarmData.Items.Add(entry);
                TrimList(listBoxAlarmData);

                if (profileLookup.TryGetValue(deviceId, out var profile))
                    labelAlarmInfo.Text = $"Alarm: {GetIntervalInfo(profile.AlarmTimestamps)}";
            }
            else if (topic.StartsWith("fingerprint/"))
            {
                listBoxFingerprintData.Items.Add(entry);
                TrimList(listBoxFingerprintData);

                if (profileLookup.TryGetValue(deviceId, out var profile))
                    labelFPInfo.Text = $"FP: {GetIntervalInfo(profile.FingerprintTimestamps)}";
            }
        }

        private void RefreshIDSAlerts()
        {
            listBoxAlerts.Items.Clear();
            foreach (var alert in IDS.Alerts.TakeLast(50))
            {
                listBoxAlerts.Items.Add(alert);
            }
        }

        private void TrimList(ListBox box)
        {
            while (box.Items.Count > 10)  // önceki 1000'di
                box.Items.RemoveAt(0);
        }

        private string GetIntervalInfo(List<DateTime> timestamps)
        {
            if (timestamps.Count < 2) return "veri yetersiz";

            var intervals = new List<double>();
            for (int i = 1; i < timestamps.Count; i++)
                intervals.Add((timestamps[i] - timestamps[i - 1]).TotalSeconds);

            var avg = intervals.Average();
            return $"{avg:F1}s ort";
        }

        private void listBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDevices.SelectedItem == null) return;

            var line = listBoxDevices.SelectedItem.ToString();
            var deviceId = line.Split('|')[0].Trim();

            if (profileLookup.TryGetValue(deviceId, out var profile))
            {
                var info = $"📟 Cihaz ID: {profile.DeviceId}\n" +
                           $"🌐 IP: {profile.Ip}\n" +
                           $"🧬 Tür: {profile.DeviceType}\n" +
                           $"⏱ İlk Görülme: {profile.FirstSeen:yyyy-MM-dd HH:mm:ss}\n" +
                           $"🔁 Son Görülme: {profile.LastSeen:yyyy-MM-dd HH:mm:ss}\n" +
                           $"🌡 Son Isı: {FormatTime(profile.LastHeat)}\n" +
                           $"📶 Son Ping: {FormatTime(profile.LastPing)}\n" +
                           $"🔋 Son Batarya: {FormatTime(profile.LastBattery)}\n" +
                           $"📷 Son Kamera: {FormatTime(profile.LastCameraData)}\n" +
                           $"🧤 Son Parmak İzi: {FormatTime(profile.LastFingerprintData)}\n" +
                           $"🚨 Son Alarm: {FormatTime(profile.LastAlarmData)}\n" +
                           $"⚠ Durum: {(profile.IsOffline ? "❌ OFFLINE" : "✅ ONLINE")}\n" +
                           $"\n📈 Beklenen Veri Aralıkları:\n" +
                           $"   - Ping: {profile.ExpectedBehavior.ExpectedPingInterval} sn\n" +
                           $"   - Isı: {profile.ExpectedBehavior.ExpectedHeatInterval} sn\n" +
                           $"   - Batarya: {profile.ExpectedBehavior.ExpectedBatteryInterval} sn\n" +
                           $"   - Kamera: {profile.ExpectedBehavior.ExpectedCameraInterval} sn\n" +
                           $"   - Parmak İzi: {profile.ExpectedBehavior.ExpectedFingerprintInterval} sn\n" +
                           $"   - Alarm: {profile.ExpectedBehavior.ExpectedAlarmInterval} sn\n" +
                           $"   - Hareket: {profile.ExpectedBehavior.ExpectedMotionInterval} sn";

                rtbCihazInfo.Text = info;
            }
        }

        private string FormatTime(DateTime dt)
        {
            return dt == DateTime.MinValue ? "Yok" : dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
