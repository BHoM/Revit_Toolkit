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
using Autodesk.Revit.DB.Structure;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Elements;
using BH.oM.Environment.Fragments;

using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static Window ToBHoMWindow(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Window aWindow = pullSettings.FindRefObject<Window>(familyInstance.Id.IntegerValue);
            if (aWindow != null)
                return aWindow;

            PlanarSurface aPlanarSurface = BH.Engine.Geometry.Create.PlanarSurface(Query.PolyCurve(familyInstance, pullSettings));
            aWindow = new Window()
            {
                Name = Query.FamilyTypeFullName(familyInstance),
                Location = aPlanarSurface

            };


            ElementType aElementType = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as ElementType;
            //Set ExtendedProperties
            OriginContextFragment aOriginContextFragment = new OriginContextFragment();
            aOriginContextFragment.ElementID = familyInstance.Id.IntegerValue.ToString();
            aOriginContextFragment.TypeName = Query.FamilyTypeFullName(familyInstance);
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, familyInstance) as OriginContextFragment;
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElementType) as OriginContextFragment;
            aWindow.Fragments.Add(aOriginContextFragment);

            aWindow = Modify.SetIdentifiers(aWindow, familyInstance) as Window;
            if (pullSettings.CopyCustomData)
                aWindow = Modify.SetCustomData(aWindow, familyInstance, pullSettings.ConvertUnits) as Window;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aWindow);

            return aWindow;
        }

        internal static Window ToBHoMWindow(this Panel panel, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Window aWindow = pullSettings.FindRefObject<Window>(panel.Id.IntegerValue);
            if (aWindow != null)
                return aWindow;

            ElementId aElementId = panel.FindHostPanel(); ;
            if (aElementId == null || aElementId == ElementId.InvalidElementId)
                return null;

            List<PolyCurve> aPolyCurveList = Query.Profiles_Wall(panel.Document.GetElement(aElementId) as dynamic, pullSettings);
            if (aPolyCurveList == null || aPolyCurveList.Count == 0)
                return null;

            PlanarSurface aPlanarSurface = BH.Engine.Geometry.Create.PlanarSurface(aPolyCurveList.First());
            aWindow = new Window()
            {
                Name = Query.FamilyTypeFullName(panel),
                Location = aPlanarSurface

            };


            ElementType aElementType = panel.Document.GetElement(panel.GetTypeId()) as ElementType;
            //Set ExtendedProperties
            OriginContextFragment aOriginContextFragment = new OriginContextFragment();
            aOriginContextFragment.ElementID = panel.Id.IntegerValue.ToString();
            aOriginContextFragment.TypeName = Query.FamilyTypeFullName(panel);
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, panel) as OriginContextFragment;
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElementType) as OriginContextFragment;
            aWindow.Fragments.Add(aOriginContextFragment);

            aWindow = Modify.SetIdentifiers(aWindow, panel) as Window;
            if (pullSettings.CopyCustomData)
                aWindow = Modify.SetCustomData(aWindow, panel, pullSettings.ConvertUnits) as Window;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aWindow);

            return aWindow;
        }

        /***************************************************/
    }
}