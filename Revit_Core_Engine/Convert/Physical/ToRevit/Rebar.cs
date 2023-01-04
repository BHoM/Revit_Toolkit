/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****            Interface methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Physical.Reinforcement.IReinforcingBar to a Revit Rebar.")]
        [Input("reinforcement", "BH.oM.Physical.Reinforcement.IReinforcingBar to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("rebar", "Revit Rebar resulting from converting the input BH.oM.Physical.Reinforcement.IReinforcingBar.")]
        public static Rebar IToRevitRebar(this IReinforcingBar reinforcement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return ToRevitRebar(reinforcement as dynamic, document, settings, refObjects);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.Physical.Reinforcement.PrimaryReinforcingBar to a Revit Rebar.")]
        [Input("bar", "BH.oM.Physical.Reinforcement.PrimaryReinforcingBar to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("rebar", "Revit Rebar resulting from converting the input BH.oM.Physical.Reinforcement.PrimaryReinforcingBar.")]
        public static Rebar ToRevitRebar(this PrimaryReinforcingBar bar, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return ToRevitRebar((IReinforcingBar)bar, document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Reinforcement.Stirrup to a Revit Rebar.")]
        [Input("stirrup", "BH.oM.Physical.Reinforcement.Stirrup to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("rebar", "Revit Rebar resulting from converting the input BH.oM.Physical.Reinforcement.Stirrup.")]
        public static Rebar ToRevitRebar(this Stirrup stirrup, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return ToRevitRebar((IReinforcingBar)stirrup, document, settings, refObjects);
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Rebar ToRevitRebar(this IReinforcingBar bar, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (bar == null || document == null)
                return null;

            Rebar rebar = refObjects.GetValue<Rebar>(document, bar.BHoM_Guid);
            if (rebar != null)
                return rebar;

            settings = settings.DefaultIfNull();

            //getting host
            Element host = null;
            if (bar.CustomData.ContainsKey("HostId"))
                host = document.GetElement(new ElementId((int)bar.CustomData["HostId"]));
            else
            {
                BH.Engine.Base.Compute.RecordError("One or more rebars does not contain information about the host.");
                return null;
            }

            if (host == null)
            {
                BH.Engine.Base.Compute.RecordError($"Rebar host with ID: {bar.CustomData["host"]} does not exist in the Revit project.");
                return null;
            }

            //getting bar type
            RebarBarType barType = bar.ElementType(document, settings);
            if (barType == null)
            {
                BH.Engine.Base.Compute.RecordError($"Revit project does not contain rebar family containing type with matching diameter for one or more rebars.\nMissing Family: \"Rebar Bar : {(int)(bar.Diameter * 1000)}\"");
                return null;
            }

            //setting bend radiuses
            barType.SetParameter(BuiltInParameter.REBAR_STANDARD_BEND_DIAMETER, bar.BendRadius * 2);
            barType.SetParameter(BuiltInParameter.REBAR_STANDARD_HOOK_BEND_DIAMETER, bar.BendRadius * 2);
            barType.SetParameter(BuiltInParameter.REBAR_BAR_STIRRUP_BEND_DIAMETER, bar.BendRadius * 2);

            //creating rebar
            RebarFreeFormValidationResult rffvr = new RebarFreeFormValidationResult();

            List<List<Curve>> curves = new List<List<Curve>> { bar.CentreCurve.IToRevitCurves() };  //
            IList<IList<Curve>> iListCurves = new List<IList<Curve>>();                             // Needed to convert
            foreach (List<Curve> list in curves)                                                    // List<List<Curve>> -> IList<IList<Curve>>
                iListCurves.Add(list);                                                              //

            rebar = Rebar.CreateFreeForm(document, barType, host, iListCurves, out rffvr);

            //setting hooks
            if (bar.GetType() == typeof(Stirrup))
            {
                RebarHookType rht = new FilteredElementCollector(document).OfClass(typeof(RebarHookType)).Where(x => x.Name == "Standard - 90 deg.").FirstOrDefault() as RebarHookType;
                rebar.SetHookTypeId(0, rht.Id);
                rebar.SetHookTypeId(1, rht.Id);
                rebar.SetHookOrientation(0, RebarHookOrientation.Left);
                rebar.SetHookOrientation(1, RebarHookOrientation.Left);
            }

            rebar.CopyParameters(bar, settings);

            refObjects.AddOrReplace(bar, rebar);
            return rebar;
        }

        /***************************************************/

    }
}


