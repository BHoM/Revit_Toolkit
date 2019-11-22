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

using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Autodesk.Revit.DB.Mechanical.Space ToRevitSpace(this Space space, Document document, PushSettings pushSettings = null)
        {
            if (space == null)
                return null;

            Autodesk.Revit.DB.Mechanical.Space aSpace = pushSettings.FindRefObject<Autodesk.Revit.DB.Mechanical.Space>(document, space.BHoM_Guid);
            if (aSpace != null)
                return aSpace;

            Level aLevel = Query.BottomLevel(space.Location.Z, document);
            if (aLevel == null)
                return null;

            UV aUV = new UV(space.Location.X.FromSI(UnitType.UT_Length), space.Location.Y.FromSI(UnitType.UT_Length));
            aSpace = document.Create.NewSpace(aLevel, aUV);

            aSpace.CheckIfNullPush(space);
            if (aSpace == null)
                return null;

            aSpace.Name = space.Name;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aSpace, space, null);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(space, aSpace);

            return aSpace;
        }

        /***************************************************/
    }
}