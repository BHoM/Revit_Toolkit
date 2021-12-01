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

using BH.Engine.Base;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using BH.oM.Revit;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Sets the host information to a given BHoM object.")]
        [Input("obj", "BHoM object to set the host information to.")]
        [Input("hostId", "ElementId of the host Revit element to be assigned to the BHoM object.")]
        [Input("linkDocument", "If the host Revit element originates from a link document, name, path or ElementId of the link document need to be specifiec here.")]
        [Output("withHost", "BHoM object with assigned host information.")]
        public static IBHoMObject SetHost(this IBHoMObject obj, int hostId, string linkDocument = "")
        {
            if (obj == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot set host to a null target object.");
                return null;
            }

            RevitHostFragment hostFragment = new RevitHostFragment(hostId, linkDocument);
            return obj.AddFragment(hostFragment, true);
        }

        /***************************************************/

        [Description("Sets the host information to a given BHoM object based on a BHoM object pulled from Revit that carries reference to the host Revit element.")]
        [Input("obj", "BHoM object to set the host information to.")]
        [Input("host", "BHoM object pulled from Revit carrying reference to the Revit element to be assigned as host.")]
        [Output("withHost", "BHoM object with assigned host information.")]
        public static IBHoMObject SetHost(this IBHoMObject obj, IBHoMObject host)
        {
            if (obj == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot set host to a null target object.");
                return null;
            }

            if (host == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot set host based on a null host object.");
                return null;
            }

            RevitIdentifiers identifiers = host.FindFragment<RevitIdentifiers>();
            if (identifiers == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning($"Setting the new host failed because the input host object is not originating from Revit pull. BHoM_Guid: {obj.BHoM_Guid}");
                return obj;
            }

            return obj.SetHost(identifiers.ElementId, identifiers.LinkDocument);
        }

        /***************************************************/
    }
}
