/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BH.Revit.Adapter.Core.Forms
{
    public partial class UpdatePortsForm : Form
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public UpdatePortsForm()
        {
            InitializeComponent();
            int inPort = RevitListener.Listener.InPort();
            if (inPort != -1)
                this.InputPort.Value = inPort;

            int outPort = RevitListener.Listener.OutPort();
            if (outPort != -1)
                this.OutputPort.Value = outPort;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private void OkBtn_Click(object sender, EventArgs e)
        {
            int inPort = (int)Math.Round(InputPort.Value);
            int outPort = (int)Math.Round(OutputPort.Value);

            if (inPort == outPort || inPort < 3000 || inPort > 65000 || outPort < 3000 || outPort > 65000)
            {
                MessageBox.Show("Input port and output port must have values between 3000 and 65000 and can not be the same", "Port number error");
                return;
            }

            RevitListener.Listener.SetPorts(inPort, outPort);
            this.Close();
        }

        /***************************************************/

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /***************************************************/
    }
}



