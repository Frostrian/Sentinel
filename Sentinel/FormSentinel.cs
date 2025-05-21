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

        private void HandleDataDisplay(string topic, string payload)
        {
            string entry = $"{DateTime.Now:HH:mm:ss} → {topic} | {payload}";

            if (topic.StartsWith("sensor/"))
            {
                listBoxSensorData.Items.Add(entry);
                TrimList(listBoxSensorData);
            }
            else if (topic.StartsWith("camera/"))
            {
                listBoxCameraData.Items.Add(entry);
                TrimList(listBoxCameraData);
            }
            else if (topic.StartsWith("alarm/"))
            {
                listBoxAlarmData.Items.Add(entry);
                TrimList(listBoxAlarmData);
            }
            else if (topic.StartsWith("fingerprint/"))
            {
                listBoxFingerprintData.Items.Add(entry);
                TrimList(listBoxFingerprintData);
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
            while (box.Items.Count > 1000)
            {
                box.Items.RemoveAt(0);
            }
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
