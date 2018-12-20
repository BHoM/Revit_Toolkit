/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Environment;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Environment.Elements.Panel> ToBHoMPanels(this Element element, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aResult = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(element.Id.IntegerValue);
            if (aResult != null && aResult.Count > 0)
                return aResult;

            aResult = Query.Panels(element.get_Geometry(new Options()), pullSettings);
            if (aResult == null || aResult.Count == 0)
                return aResult;

            for(int i=0; i < aResult.Count; i++)
            {
                aResult[i] = Modify.SetIdentifiers(aResult[i], element) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    aResult[i] = Modify.SetCustomData(aResult[i], element, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aResult[i]);
            }

            return aResult;
        }

        /***************************************************/

        internal static List<oM.Environment.Elements.Panel> ToBHoMPanels(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aResult = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(roofBase.Id.IntegerValue);
            if (aResult != null && aResult.Count > 0)
                return aResult;

            aResult = Query.Panels(roofBase.get_Geometry(new Options()), pullSettings);
            if (aResult == null || aResult.Count == 0)
                return aResult;

            for (int i = 0; i < aResult.Count; i++)
            {
                aResult[i] = Modify.SetIdentifiers(aResult[i], roofBase) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    aResult[i] = Modify.SetCustomData(aResult[i], roofBase, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aResult[i]);
            }

            return aResult;
        }

        /***************************************************/

        internal static List<oM.Environment.Elements.Panel> ToBHoMPanels(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aResult = new List<oM.Environment.Elements.Panel>();

            oM.Environment.Elements.Panel aPanel = pullSettings.FindRefObject<oM.Environment.Elements.Panel>(familyInstance.Id.IntegerValue);
            if (aPanel != null)
            {
                aResult.Add(aPanel);
                return aResult;
            }

            aPanel = Create.Panel(new oM.Geometry.Polyline[] { familyInstance.VerticalBounds(pullSettings)});
            if (aPanel != null)
                aResult.Add(aPanel);

            aPanel = Modify.SetIdentifiers(aPanel, familyInstance) as oM.Environment.Elements.Panel;
            if (pullSettings.CopyCustomData)
                aPanel = Modify.SetCustomData(aPanel, familyInstance, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPanel);

            return aResult;
        }

        /***************************************************/


    }
}