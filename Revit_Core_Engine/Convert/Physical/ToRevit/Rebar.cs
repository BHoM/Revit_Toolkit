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

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Reinforcement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****            Interface methods              ****/
        /***************************************************/

        public static FamilyInstance IToRevitRebar(this IReinforcingBar reinforcement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return ToRevitRebar(reinforcement as dynamic, document, settings, refObjects);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Rebar ToRevitRebar(this PrimaryReinforcingBar bar, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (bar == null || document == null)
                return null;

            Rebar rebar = refObjects.GetValue<Rebar>(document, bar.BHoM_Guid);
            if (rebar != null)
                return rebar;

            settings = settings.DefaultIfNull();

            //getting bar type
            string barTypeName = ((int)(bar.Diameter * 1000)).ToString();
            RebarBarType barType = new FilteredElementCollector(document).OfClass(typeof(RebarBarType)).Where(x => x.Name == barTypeName).FirstOrDefault() as RebarBarType;
            if (barType == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit project does not contain adequate rebar family for one or more rebars.");
                return null;
            }

            //getting host
            Element host = null;
            try //remove try-catch
            {
                host = document.GetElement(new ElementId((int)bar.CustomData["host"]));
            }
            catch (KeyNotFoundException e)
            {
                BH.Engine.Reflection.Compute.RecordError("One or more rebars does not contain information about the host.");
                return null;
            }
            if (host == null)
            {
                BH.Engine.Reflection.Compute.RecordError("One or more selected hosts does not exist in the Revit project.");
                return null;
            }

            //creating rebar
            RebarFreeFormValidationResult rffvr = new RebarFreeFormValidationResult();
                        
            List<List<Curve>> curves = new List<List<Curve>> { bar.CentreCurve.IToRevitCurves() };  //
            IList<IList<Curve>> iListCurves = new List<IList<Curve>>();                             // Needed to convert
            foreach (List<Curve> list in curves)                                                    // List<List<Curve>> -> IList<IList<Curve>>
                iListCurves.Add(list);                                                              //

            rebar = Rebar.CreateFreeForm(document, barType, host, iListCurves, out rffvr);


            rebar.CopyParameters(bar, settings);
            //rebar.SetLocation(bar, settings);

            refObjects.AddOrReplace(bar, rebar);
            return rebar;
        }

        /***************************************************/

        public static Rebar ToRevitRebar(this Stirrup stirrup, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (stirrup == null || document == null)
                return null;

            Rebar rebar = refObjects.GetValue<Rebar>(document, stirrup.BHoM_Guid);
            if (rebar != null)
                return rebar;

            settings = settings.DefaultIfNull();

            /*** copyPaste from .Rebar  ***/
            //works, but maybe can be improved

            //getting bar type
            string barTypeName = ((int)(stirrup.Diameter * 1000)).ToString();
            RebarBarType barType = new FilteredElementCollector(document).OfClass(typeof(RebarBarType)).Where(x => x.Name == barTypeName).FirstOrDefault() as RebarBarType;
            if (barType == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit project does not contain adequate rebar family for one or more rebars.");
                return null;
            }

            //getting host
            Element host = null;
            try
            {
                host = document.GetElement(new ElementId((int)stirrup.CustomData["host"]));
            }
            catch (KeyNotFoundException e)
            {
                BH.Engine.Reflection.Compute.RecordError("One or more rebars does not contain information about the host.");
                return null;
            }
            if (host == null)
            {
                BH.Engine.Reflection.Compute.RecordError("One or more selected hosts does not exist in the Revit project.");
                return null;
            }

            //creating rebar
            RebarFreeFormValidationResult rffvr = new RebarFreeFormValidationResult();

            List<List<Curve>> curves = new List<List<Curve>> { stirrup.CentreCurve.IToRevitCurves() };  //
            IList<IList<Curve>> iListCurves = new List<IList<Curve>>();                                 // Needed to convert
            foreach (List<Curve> list in curves)                                                        // List<List<Curve>> -> IList<IList<Curve>>
                iListCurves.Add(list);                                                                  //

            rebar = Rebar.CreateFreeForm(document, barType, host, iListCurves, out rffvr);

            rebar.CopyParameters(stirrup, settings);
            //rebar.SetLocation(stirrup, settings);

            /*** end of copypaste       ***/

            refObjects.AddOrReplace(stirrup, rebar);
            return rebar;
        }

        /***************************************************/
    }
}
