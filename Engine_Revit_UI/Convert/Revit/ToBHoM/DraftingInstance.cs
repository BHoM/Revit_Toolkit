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
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Adapters.Revit.Elements.DraftingInstance ToBHoMDraftingInstance(this CurveElement curveElement, PullSettings pullSettings = null)
        {
            oM.Adapters.Revit.Elements.DraftingInstance aDraftingInstance = pullSettings.FindRefObject<oM.Adapters.Revit.Elements.DraftingInstance>(curveElement.Id.IntegerValue);
            if (aDraftingInstance != null)
                return aDraftingInstance;

            View aView = curveElement.Document.GetElement(curveElement.OwnerViewId) as View;
            if (aView == null)
                return null;

            oM.Adapters.Revit.Properties.InstanceProperties aInstanceProperties = ToBHoMInstanceProperties(curveElement.LineStyle as GraphicsStyle, pullSettings) as oM.Adapters.Revit.Properties.InstanceProperties;

            aDraftingInstance = BH.Engine.Adapters.Revit.Create.DraftingInstance(aInstanceProperties, aView.Name, curveElement.GeometryCurve.ToBHoM(pullSettings));

            aDraftingInstance.Name = curveElement.Name;
            aDraftingInstance = Modify.SetIdentifiers(aDraftingInstance, curveElement) as oM.Adapters.Revit.Elements.DraftingInstance;
            if (pullSettings.CopyCustomData)
                aDraftingInstance = Modify.SetCustomData(aDraftingInstance, curveElement, true) as oM.Adapters.Revit.Elements.DraftingInstance;

            aDraftingInstance = aDraftingInstance.UpdateValues(pullSettings, curveElement) as oM.Adapters.Revit.Elements.DraftingInstance;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aDraftingInstance);

            return aDraftingInstance;
        }

        /***************************************************/
    }
}
