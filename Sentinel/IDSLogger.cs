using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class IDSLogger
{
    private static readonly string logFilePath = "IDS_Logs.json";
    private static readonly object fileLock = new();

    public static void Log(string deviceId, string reason, string topic)
    {
        var entry = new IDSLogEntry
        {
            Timestamp = DateTime.Now,
            DeviceId = deviceId,
            Reason = reason,
            Topic = topic
        };

        lock (fileLock)
        {
            List<IDSLogEntry> existingLogs = new();

            if (File.Exists(logFilePath))
            {
                var json = File.ReadAllText(logFilePath);
                existingLogs = JsonSerializer.Deserialize<List<IDSLogEntry>>(json) ?? new();
            }

            existingLogs.Add(entry);

            var updatedJson = JsonSerializer.Serialize(existingLogs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(logFilePath, updatedJson);
        }
    }

    private class IDSLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string DeviceId { get; set; }
        public string Reason { get; set; }
        public string Topic { get; set; }
    }
}