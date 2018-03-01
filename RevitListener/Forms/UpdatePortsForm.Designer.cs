namespace BH.Adapter.Revit.Forms
{
    partial class UpdatePortsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.InputPort = new System.Windows.Forms.NumericUpDown();
            this.OutputPort = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.OkBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.InputPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OutputPort)).BeginInit();
            this.SuspendLayout();
            // 
            // InputPort
            // 
            this.InputPort.Location = new System.Drawing.Point(12, 37);
            this.InputPort.Maximum = new decimal(new int[] {
            65000,
            0,
            0,
            0});
            this.InputPort.Minimum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.InputPort.Name = "InputPort";
            this.InputPort.Size = new System.Drawing.Size(165, 22);
            this.InputPort.TabIndex = 0;
            this.InputPort.Value = new decimal(new int[] {
            14128,
            0,
            0,
            0});
            // 
            // OutputPort
            // 
            this.OutputPort.Location = new System.Drawing.Point(12, 92);
            this.OutputPort.Maximum = new decimal(new int[] {
            65000,
            0,
            0,
            0});
            this.OutputPort.Minimum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.OutputPort.Name = "OutputPort";
            this.OutputPort.Size = new System.Drawing.Size(165, 22);
            this.OutputPort.TabIndex = 1;
            this.OutputPort.Value = new decimal(new int[] {
            14129,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input port number";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output port number";
            // 
            // OkBtn
            // 
            this.OkBtn.Location = new System.Drawing.Point(107, 129);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(70, 45);
            this.OkBtn.TabIndex = 4;
            this.OkBtn.Text = "Ok";
            this.OkBtn.UseVisualStyleBackColor = true;
            this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(12, 129);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 45);
            this.CancelBtn.TabIndex = 5;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // UpdatePortsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(193, 198);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OutputPort);
            this.Controls.Add(this.InputPort);
            this.Name = "UpdatePortsForm";
            this.Text = "UpdatePortsForm";
            ((System.ComponentModel.ISupportInitialize)(this.InputPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OutputPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown InputPort;
        private System.Windows.Forms.NumericUpDown OutputPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button CancelBtn;
    }
}