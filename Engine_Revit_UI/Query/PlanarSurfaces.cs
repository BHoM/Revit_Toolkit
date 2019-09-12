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

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public IEnumerable<PlanarSurface> PlanarSurfaces(this HostObject hostObject, PullSettings pullSettings = null)
        {
            List<PolyCurve> aPolyCurveList = Query.Profiles(hostObject, pullSettings);

            List<PolyCurve> aPolyCurveList_Outer = null;
            try
            {
                aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);
            }
            catch (Exception aException)
            {
                aPolyCurveList_Outer = aPolyCurveList;
            }

            if (aPolyCurveList_Outer == null)
                return null;

            List<PlanarSurface> aResult = new List<PlanarSurface>();
            foreach (PolyCurve aPolyCurve in aPolyCurveList_Outer)
            {
                List<PolyCurve> aPolyCurveList_Inner = null;
                try
                {
                    aPolyCurveList_Inner = BH.Engine.Adapters.Revit.Query.InnerPolyCurves(aPolyCurve, aPolyCurveList);
                }
                catch (Exception aException)
                {

                }

                List<ICurve> aICurveList = new List<ICurve>();
                if (aPolyCurveList_Inner != null && aPolyCurveList_Inner.Count > 0)
                    aICurveList = aPolyCurveList_Inner.ConvertAll(x => (ICurve)x);

                //TODO: Create method in Geometry Engine shall be used however IsClosed method returns false for some of the PolyCurves pulled from Revit
                //PlanarSurface aPlanarSurface = BH.Engine.Geometry.Create.PlanarSurface(aPolyCurve, aPolyCurveList_Inner.ConvertAll(x => (ICurve)x)
                PlanarSurface aPlanarSurface = new PlanarSurface()
                {
                    ExternalBoundary = aPolyCurve,
                    InternalBoundaries = aICurveList
                };

                aResult.Add(aPlanarSurface);
            }

            return aResult;
        }

        /***************************************************/
    }
}