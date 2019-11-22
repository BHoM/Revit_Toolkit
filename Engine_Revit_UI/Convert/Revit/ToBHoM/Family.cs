/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Adapters.Revit.Elements.Family ToBHoMFamily(this Family family, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Adapters.Revit.Elements.Family aFamily = pullSettings.FindRefObject<oM.Adapters.Revit.Elements.Family>(family.Id.IntegerValue);
            if (aFamily != null)
                return aFamily;

            aFamily = new oM.Adapters.Revit.Elements.Family();
            aFamily.Name = family.Name;

            IEnumerable<ElementId> aElementIds = family.GetFamilySymbolIds();
            if(aElementIds != null && aElementIds.Count() > 0 )
            {
                if (aFamily.PropertiesList == null)
                    aFamily.PropertiesList = new List<InstanceProperties>();

                foreach(ElementId aElementId in aElementIds)
                {
                    if (aElementId == null || aElementId == ElementId.InvalidElementId)
                        continue;

                    ElementType aElementType = family.Document.GetElement(aElementId) as ElementType;
                    if (aElementType == null)
                        continue;

                    InstanceProperties aInstanceProperties = ToBHoMInstanceProperties(aElementType, pullSettings);
                    if (aInstanceProperties == null)
                        continue;

                    aFamily.PropertiesList.Add(aInstanceProperties);
                }
            }

            aFamily = Modify.SetIdentifiers(aFamily, family) as oM.Adapters.Revit.Elements.Family;
            if (pullSettings.CopyCustomData)
                aFamily = Modify.SetCustomData(aFamily, family) as oM.Adapters.Revit.Elements.Family;

            aFamily = aFamily.UpdateValues(pullSettings, family) as oM.Adapters.Revit.Elements.Family;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aFamily);

            return aFamily;
        }

        /***************************************************/
    }
}
