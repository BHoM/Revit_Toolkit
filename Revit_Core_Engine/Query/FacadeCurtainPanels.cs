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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Facade.Elements;
using BH.Engine.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using System.Runtime;
using BH.oM.Revit.Enums;
using BH.Engine.Geometry;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the panels from a Revit curtain grid and returns them in a form of BHoM facade Openings.")]
        [Input("curtainGrid", "Revit curtain grid to extract the panels from.")]
        [Input("document", "Revit document, to which the curtain grid belongs.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panels", "Panels extracted from the input Revit curtain grid and converted to BHoM facade Openings.")]
        public static List<oM.Facade.Elements.Opening> FacadeCurtainPanels(this CurtainGrid curtainGrid, Document document, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (curtainGrid == null)
                return null;

            List<Element> panels = curtainGrid.GetPanelIds().Select(x => document.GetElement(x)).ToList();
            List<CurtainCell> cells = curtainGrid.GetCurtainCells().ToList();

            if (panels.Count != cells.Count)
                return null;

            List<FrameEdge> mullions = curtainGrid.CurtainWallMullions(document, settings, refObjects);
            //List<Tuple<BH.oM.Geometry.Point, FrameEdge>> mullionsByLocation = mullions.Where(x => x.Curve != null).Select(x => new Tuple<BH.oM.Geometry.Point, FrameEdge>(x.Curve.IPointAtParameter(0.5), x)).ToList();

            List<oM.Facade.Elements.Opening> result = new List<oM.Facade.Elements.Opening>();

            for (int i = 0; i < panels.Count; i++)
            {
                FamilyInstance panel = panels[i] as FamilyInstance;
                if (panel == null)
                    continue;

                List<PolyCurve> pcs = new List<PolyCurve>();
                try
                {
                    // This catches when PlanarizedCurveLoops throws an exception due to the cell having no loops, meaning in Revit it exists in the database but is no longer a valid CurtainWall cell
                    CurveArrArray x = cells[i].PlanarizedCurveLoops;
                    pcs = cells[i].CurveLoops.FromRevit();
                }
                catch 
                { 
                    continue; 
                }

                foreach (PolyCurve pc in pcs)
                {
                    BH.oM.Facade.Elements.Opening bHoMOpening = new oM.Facade.Elements.Opening();
                    bHoMOpening.OpeningConstruction = panel.Constr(settings, refObjects);
                    bHoMOpening.Type = panel.Opn();

                    bHoMOpening.Edges = new List<FrameEdge>();
                    foreach (ICurve curve in pc.Curves)
                    {
                        BH.oM.Geometry.Point mid = curve.IPointAtParameter(0.5);
                        FrameEdge mullion = mullions.FirstOrDefault(x => x.Curve != null && mid.IDistance(x.Curve) <= settings.DistanceTolerance);
                        if (mullion == null)
                        {
                            BH.Engine.Base.Compute.RecordWarning("Mullion information is missing for some profiles in the curtain wall.");
                            mullion = new FrameEdge();
                        }

                        mullion.Curve = curve;
                        bHoMOpening.Edges.Add(mullion);
                    }

                    bHoMOpening.Name = panel.Name;

                    //Set identifiers, parameters & custom data
                    bHoMOpening.SetIdentifiers(panel);
                    bHoMOpening.CopyParameters(panel, settings.MappingSettings);
                    bHoMOpening.SetProperties(panel, settings.MappingSettings);

                    result.Add(bHoMOpening);
                }
            }
            
            return result;
        }

        //TODO: apply in all applicable places
        private static OpeningType Opn(this FamilyInstance panel)
        {
            BuiltInCategory category = (BuiltInCategory)panel.Category.Id.IntegerValue;
            if (category == Autodesk.Revit.DB.BuiltInCategory.OST_Windows || category == Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels)
                return BH.oM.Facade.Elements.OpeningType.Window;
            else if (category == Autodesk.Revit.DB.BuiltInCategory.OST_Doors)
                return BH.oM.Facade.Elements.OpeningType.Door;
            else
                return BH.oM.Facade.Elements.OpeningType.Undefined;
        }

        //TODO: apply in all applicable places
        private static BH.oM.Physical.Constructions.Construction Constr(this FamilyInstance panel, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects)
        {
            if ((panel as Autodesk.Revit.DB.Panel)?.FindHostPanel() is ElementId hostId && panel.Document.GetElement(hostId) is Wall wall)
            {
                HostObjAttributes hostObjAttributes = wall.Document.GetElement(wall.GetTypeId()) as HostObjAttributes;
                string materialGrade = wall.MaterialGrade(settings);
                return hostObjAttributes.ConstructionFromRevit(materialGrade, settings, refObjects);
            }
            else
            {
                int category = panel.Category.Id.IntegerValue;
                if (category == (int)Autodesk.Revit.DB.BuiltInCategory.OST_Walls)
                {
                    HostObjAttributes hostObjAttributes = panel.Document.GetElement(panel.GetTypeId()) as HostObjAttributes;
                    string materialGrade = panel.MaterialGrade(settings);
                    return hostObjAttributes.ConstructionFromRevit(materialGrade, settings, refObjects);
                }
                else
                    return panel.GlazingConstruction();
            }
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        //private static oM.Facade.Elements.Opening FacadePanelAsOpening(this oM.Facade.Elements.Panel panel, string refId = "", Dictionary<string, List<IBHoMObject>> refObjects = null)
        //{
        //    if (panel == null)
        //        return null;
            
        //    oM.Facade.Elements.Opening opening = refObjects.GetValue<oM.Facade.Elements.Opening>(refId);
        //    if (opening != null)
        //        return opening;

        //    opening = new oM.Facade.Elements.Opening { Name = panel.Name, Edges = panel.ExternalEdges, Fragments = panel.Fragments, OpeningConstruction = panel.Construction, Tags = panel.Tags, CustomData = panel.CustomData, Type = oM.Facade.Elements.OpeningType.Undefined };

        //    return opening;
        //}

        /***************************************************/
    }
}


