/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using Autodesk.Revit.DB;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****               Public methods              ****/
        /***************************************************/

        [Description("Manipulates the given reference to a linked element in order to make it applicable for dimensioning in the host document.")]
        [Input("reference", "Linked element reference to be prepared for dimensioning in the host document.")]
        [Input("hostDocument", "Revit document to act as a host document for dimensioning.")]
        [Output("reference", "Linked element reference prepared for dimensioning in the host document.")]
        public static Reference PrepareForLinkDimensioning(this Reference reference, Document hostDocument)
        {
            if (reference.LinkedElementId.IntegerValue == -1)
                return null;

            string[] ss = reference.ConvertToStableRepresentation(hostDocument).Split(':');
            string res = string.Empty;
            bool first = true;
            foreach (string s in ss)
            {
                string t = s;
                if (s.Contains("RVTLINK"))
                {
                    if (res.EndsWith(":0"))
                        t = "RVTLINK";
                    else
                        t = "0:RVTLINK";
                }

                if (!first)
                    res = string.Concat(res, ":", t);
                else
                {
                    res = t;
                    first = false;
                }
            }

            return Reference.ParseFromStableRepresentation(hostDocument, res);
        }

        /***************************************************/
    }
}

