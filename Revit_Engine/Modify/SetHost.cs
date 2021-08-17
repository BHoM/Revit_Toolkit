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

        //[Description("Sets RevitSettings to default value if they are null.")]
        //[Input("settings", "RevitSettings to be set to default if null.")]
        //[Output("revitSettings")]
        public static IBHoMObject SetHost(this IBHoMObject obj, int hostId, int linkDocument = -1)
        {
            IBHoMObject clone = obj.DeepClone();
            RevitHostFragment hostFragment = new RevitHostFragment(hostId, linkDocument);
            obj.AddFragment(hostFragment, true);

            return clone;
        }

        /***************************************************/

        public static IBHoMObject SetHost(this IBHoMObject obj, IBHoMObject host)
        {
            RevitIdentifiers identifiers = host.FindFragment<RevitIdentifiers>();
            if (identifiers == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning($"Setting the new host failed because the input host object is not originating from Revit pull. BHoM_Guid: {obj.BHoM_Guid}");
                return obj;
            }

            return obj.SetHost(identifiers.ElementId);
        }

        /***************************************************/
    }
}




