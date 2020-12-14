/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

namespace BH.Revit.Adapter.Core.Forms
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
            this.OkBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.InputPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OutputPort)).BeginInit();
            this.SuspendLayout();
            // 
            // InputPort
            // 
            this.InputPort.Location = new System.Drawing.Point(16, 55);
            this.InputPort.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.InputPort.Size = new System.Drawing.Size(193, 22);
            this.InputPort.TabIndex = 0;
            this.InputPort.Value = new decimal(new int[] {
            14128,
            0,
            0,
            0});
            // 
            // OutputPort
            // 
            this.OutputPort.Location = new System.Drawing.Point(16, 131);
            this.OutputPort.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.OutputPort.Size = new System.Drawing.Size(193, 22);
            this.OutputPort.TabIndex = 1;
            this.OutputPort.Value = new decimal(new int[] {
            14129,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(196, 41);
            this.label1.TabIndex = 2;
            this.label1.Text = "Push port (must be equal to ConnectorSettings.PushPort):";
            // 
            // OkBtn
            // 
            this.OkBtn.Location = new System.Drawing.Point(129, 166);
            this.OkBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(80, 27);
            this.OkBtn.TabIndex = 4;
            this.OkBtn.Text = "OK";
            this.OkBtn.UseVisualStyleBackColor = true;
            this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(43, 166);
            this.CancelBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(80, 27);
            this.CancelBtn.TabIndex = 5;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(196, 41);
            this.label2.TabIndex = 6;
            this.label2.Text = "Pull port (must be equal to ConnectorSettings.PullPort):";
            // 
            // UpdatePortsForm
            // 
            this.AcceptButton = this.OkBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(216, 206);
            this.ControlBox = false;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OutputPort);
            this.Controls.Add(this.InputPort);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "UpdatePortsForm";
            this.Text = "Set Ports";
            ((System.ComponentModel.ISupportInitialize)(this.InputPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OutputPort)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown InputPort;
        private System.Windows.Forms.NumericUpDown OutputPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Label label2;
    }
}
