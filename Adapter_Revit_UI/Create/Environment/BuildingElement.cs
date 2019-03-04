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

using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.UI.Revit.Engine;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Element Create(BuildingElement buildingElement, Document document, PushSettings pushSettings = null)
        {
            throw new System.NotImplementedException("The method to create a Revit Element from a BHoM Building Element has not been fixed yet. Check Issue #247 for more info");
            /*if (buildingElement == null)
            {
                NullObjectCreateError(typeof(BuildingElement));
                return null;
            }

            if (buildingElement.BuildingElementProperties == null)
            {
                NullObjectCreateError(typeof(BuildingElementProperties));
                return null;
            }

            pushSettings = pushSettings.DefaultIfNull();

            //if (pushSettings.Replace)
            //    Delete(buildingElement.BuildingElementProperties, document);

            buildingElement.BuildingElementProperties.ToRevit(document, pushSettings);


            //Set Level
            /*if (buildingElement.Level != null)
                Create(buildingElement.Level, pushSettings);*/

            /*if (pushSettings.Replace)
                Delete(buildingElement, document);

            return buildingElement.ToRevit(document, pushSettings);*/
        }

        /***************************************************/
    }
}