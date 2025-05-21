namespace Sentinel
{
    partial class FormSentinel
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listBoxSensorData = new ListBox();
            listBoxCameraData = new ListBox();
            listBoxFingerprintData = new ListBox();
            listBoxAlarmData = new ListBox();
            listBoxDevices = new ListBox();
            txtLog = new RichTextBox();
            listBoxAlerts = new ListBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            rtbCihazInfo = new RichTextBox();
            label7 = new Label();
            label8 = new Label();
            labelSensorInfo = new Label();
            labelCameraInfo = new Label();
            labelFPInfo = new Label();
            labelAlarmInfo = new Label();
            SuspendLayout();
            // 
            // listBoxSensorData
            // 
            listBoxSensorData.FormattingEnabled = true;
            listBoxSensorData.Location = new Point(12, 28);
            listBoxSensorData.Name = "listBoxSensorData";
            listBoxSensorData.Size = new Size(846, 109);
            listBoxSensorData.TabIndex = 0;
            // 
            // listBoxCameraData
            // 
            listBoxCameraData.FormattingEnabled = true;
            listBoxCameraData.Location = new Point(12, 158);
            listBoxCameraData.Name = "listBoxCameraData";
            listBoxCameraData.Size = new Size(846, 109);
            listBoxCameraData.TabIndex = 0;
            // 
            // listBoxFingerprintData
            // 
            listBoxFingerprintData.FormattingEnabled = true;
            listBoxFingerprintData.Location = new Point(12, 288);
            listBoxFingerprintData.Name = "listBoxFingerprintData";
            listBoxFingerprintData.Size = new Size(846, 109);
            listBoxFingerprintData.TabIndex = 0;
            // 
            // listBoxAlarmData
            // 
            listBoxAlarmData.FormattingEnabled = true;
            listBoxAlarmData.Location = new Point(12, 418);
            listBoxAlarmData.Name = "listBoxAlarmData";
            listBoxAlarmData.Size = new Size(846, 124);
            listBoxAlarmData.TabIndex = 0;
            // 
            // listBoxDevices
            // 
            listBoxDevices.FormattingEnabled = true;
            listBoxDevices.Location = new Point(864, 28);
            listBoxDevices.Name = "listBoxDevices";
            listBoxDevices.Size = new Size(437, 799);
            listBoxDevices.TabIndex = 0;
            listBoxDevices.SelectedIndexChanged += listBoxDevices_SelectedIndexChanged;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(12, 695);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(846, 138);
            txtLog.TabIndex = 1;
            txtLog.Text = "";
            // 
            // listBoxAlerts
            // 
            listBoxAlerts.FormattingEnabled = true;
            listBoxAlerts.Location = new Point(12, 563);
            listBoxAlerts.Name = "listBoxAlerts";
            listBoxAlerts.Size = new Size(846, 109);
            listBoxAlerts.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(874, 11);
            label1.Name = "label1";
            label1.Size = new Size(68, 15);
            label1.TabIndex = 2;
            label1.Text = "Cihaz listesi";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(16, 545);
            label2.Name = "label2";
            label2.Size = new Size(91, 15);
            label2.TabIndex = 2;
            label2.Text = "IDS Alarm listesi";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 9);
            label3.Name = "label3";
            label3.Size = new Size(90, 15);
            label3.TabIndex = 2;
            label3.Text = "Isı Sensörü Data";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 140);
            label4.Name = "label4";
            label4.Size = new Size(74, 15);
            label4.TabIndex = 2;
            label4.Text = "Kamera Data";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 270);
            label5.Name = "label5";
            label5.Size = new Size(88, 15);
            label5.TabIndex = 2;
            label5.Text = "Parmak izi Data";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 400);
            label6.Name = "label6";
            label6.Size = new Size(75, 15);
            label6.TabIndex = 2;
            label6.Text = "Hareket Data";
            // 
            // rtbCihazInfo
            // 
            rtbCihazInfo.Location = new Point(1307, 29);
            rtbCihazInfo.Name = "rtbCihazInfo";
            rtbCihazInfo.Size = new Size(290, 475);
            rtbCihazInfo.TabIndex = 1;
            rtbCihazInfo.Text = "";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(1318, 11);
            label7.Name = "label7";
            label7.Size = new Size(70, 15);
            label7.TabIndex = 2;
            label7.Text = "Cihaz Bilgisi";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(14, 675);
            label8.Name = "label8";
            label8.Size = new Size(62, 15);
            label8.TabIndex = 2;
            label8.Text = "Log Listesi";
            // 
            // labelSensorInfo
            // 
            labelSensorInfo.AutoSize = true;
            labelSensorInfo.Location = new Point(108, 9);
            labelSensorInfo.Name = "labelSensorInfo";
            labelSensorInfo.Size = new Size(90, 15);
            labelSensorInfo.TabIndex = 2;
            labelSensorInfo.Text = "Veri bekleniyor..";
            // 
            // labelCameraInfo
            // 
            labelCameraInfo.AutoSize = true;
            labelCameraInfo.Location = new Point(108, 140);
            labelCameraInfo.Name = "labelCameraInfo";
            labelCameraInfo.Size = new Size(90, 15);
            labelCameraInfo.TabIndex = 2;
            labelCameraInfo.Text = "Veri bekleniyor..";
            // 
            // labelFPInfo
            // 
            labelFPInfo.AutoSize = true;
            labelFPInfo.Location = new Point(108, 270);
            labelFPInfo.Name = "labelFPInfo";
            labelFPInfo.Size = new Size(90, 15);
            labelFPInfo.TabIndex = 2;
            labelFPInfo.Text = "Veri bekleniyor..";
            // 
            // labelAlarmInfo
            // 
            labelAlarmInfo.AutoSize = true;
            labelAlarmInfo.Location = new Point(108, 400);
            labelAlarmInfo.Name = "labelAlarmInfo";
            labelAlarmInfo.Size = new Size(90, 15);
            labelAlarmInfo.TabIndex = 2;
            labelAlarmInfo.Text = "Veri bekleniyor..";
            // 
            // FormSentinel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1609, 844);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(labelAlarmInfo);
            Controls.Add(labelFPInfo);
            Controls.Add(labelCameraInfo);
            Controls.Add(labelSensorInfo);
            Controls.Add(label3);
            Controls.Add(label8);
            Controls.Add(label2);
            Controls.Add(label7);
            Controls.Add(label1);
            Controls.Add(rtbCihazInfo);
            Controls.Add(txtLog);
            Controls.Add(listBoxAlerts);
            Controls.Add(listBoxAlarmData);
            Controls.Add(listBoxFingerprintData);
            Controls.Add(listBoxCameraData);
            Controls.Add(listBoxDevices);
            Controls.Add(listBoxSensorData);
            Name = "FormSentinel";
            Text = "Form IOT HUB";
            Load += FormSentinel_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox listBoxSensorData;
        private ListBox listBoxCameraData;
        private ListBox listBoxFingerprintData;
        private ListBox listBoxAlarmData;
        private ListBox listBoxDevices;
        private RichTextBox txtLog;
        private ListBox listBoxAlerts;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private RichTextBox rtbCihazInfo;
        private Label label7;
        private Label label8;
        private Label labelSensorInfo;
        private Label labelCameraInfo;
        private Label labelFPInfo;
        private Label labelAlarmInfo;
    }
}
