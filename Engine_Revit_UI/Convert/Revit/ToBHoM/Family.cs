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

        public static oM.Adapters.Revit.Elements.Family ToBHoMFamily(this Family revitFamily, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Adapters.Revit.Elements.Family family = pullSettings.FindRefObject<oM.Adapters.Revit.Elements.Family>(revitFamily.Id.IntegerValue);
            if (family != null)
                return family;

            family = new oM.Adapters.Revit.Elements.Family();
            family.Name = revitFamily.Name;

            IEnumerable<ElementId> elementIDs = revitFamily.GetFamilySymbolIds();
            if(elementIDs != null && elementIDs.Count() > 0 )
            {
                if (family.PropertiesList == null)
                    family.PropertiesList = new List<InstanceProperties>();

                foreach(ElementId elementID in elementIDs)
                {
                    if (elementID == null || elementID == ElementId.InvalidElementId)
                        continue;

                    ElementType elementType = revitFamily.Document.GetElement(elementID) as ElementType;
                    if (elementType == null)
                        continue;

                    InstanceProperties instanceProperties = ToBHoMInstanceProperties(elementType, pullSettings);
                    if (instanceProperties == null)
                        continue;

                    family.PropertiesList.Add(instanceProperties);
                }
            }

            family = Modify.SetIdentifiers(family, revitFamily) as oM.Adapters.Revit.Elements.Family;
            if (pullSettings.CopyCustomData)
                family = Modify.SetCustomData(family, revitFamily) as oM.Adapters.Revit.Elements.Family;

            family = family.UpdateValues(pullSettings, revitFamily) as oM.Adapters.Revit.Elements.Family;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(family);

            return family;
        }

        /***************************************************/
    }
}

