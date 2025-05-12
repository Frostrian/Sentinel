namespace Sentinel
{
    public partial class FormSentinel : Form
    {
        private List<DeviceProfile> devices = new();
        private List<DeviceTracker> deviceTracker = new();
        private MqttService mqtt;
        public FormSentinel()
        {
            InitializeComponent();
            mqtt = new MqttService(Log, AddDevice, HandleDataDisplay, action => this.Invoke(action), RefreshIDSAlerts);
            Load += async (s, e) => await mqtt.ConnectAsync();
        }

        SentinelProfileManager profileManager = new SentinelProfileManager(deviceTracker, Log);

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
    }
}
