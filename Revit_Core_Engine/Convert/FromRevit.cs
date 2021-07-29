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
using Autodesk.Revit.DB.Analysis;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        [Description("Interface method that tries to find a suitable FromRevit convert for any Revit Element.")]
        [Input("element", "Revit Element to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Element.")]
        public static object IFromRevit(this Element element, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return FromRevit(element as dynamic, discipline, transform, settings, refObjects);
        }

        /***************************************************/

        [Description("Interface method that tries to find a suitable FromRevit convert for a Revit Location.")]
        [Input("location", "Revit Location to be converted.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Location.")]
        public static IGeometry IFromRevit(this Location location)
        {
            if (location == null)
                return null;

            return FromRevit(location as dynamic);
        }

        
        /***************************************************/
        /****      Convert Revit elements to BHoM       ****/
        /***************************************************/

        [Description("Converts a Revit EnergyAnalysisDetailModel to a BHoM object based on the requested engineering discipline.")]
        [Input("energyAnalysisModel", "Revit EnergyAnalysisDetailModel to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit EnergyAnalysisDetailModel.")]
        public static IEnumerable<IBHoMObject> FromRevit(this EnergyAnalysisDetailModel energyAnalysisModel, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (energyAnalysisModel == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("BHoM object could not be read because Revit energy analysis model is null.");
                return null;
            }

            switch (discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisModel.EnergyAnalysisModelFromRevit(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        //[Description("Fallback method when no suitable Element FromRevit is found.")]
        //[Input("element", "Revit Element to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Null resulted from no suitable Element FromRevit method.")]
        public static IEnumerable<IBHoMObject> FromRevit(this Element element, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (element == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("BHoM object could not be read because Revit element is null.");
                return null;
            }

            object converted;
            
            Type targetBHoMType = element.IBHoMType(discipline, settings);
            if (targetBHoMType == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning($"Given Revit element of type {element.GetType().Name} does not have a correspondent BHoM type for discipline {discipline}. It has been converted to a generic ModelInstance or DraftingInstance. ElementId: {element.Id}");
                converted = element.ObjectFromRevit(settings, refObjects);
            }
            else
            {
                MethodInfo convertMethod = element.ConvertMethod(targetBHoMType);
                converted = convertMethod.Invoke(null, new object[] { element, settings, refObjects });

                if (converted == null || (typeof(IEnumerable<object>).IsAssignableFrom(converted.GetType()) && ((IEnumerable<object>)converted).Count(x => x != null) == 0))
                {
                    converted = element.ObjectFromRevit(settings, refObjects);
                    element.NotConvertedWarning(discipline);
                }
            }

            List<IBHoMObject> result = null;
            if (converted is IBHoMObject)
                result = new List<IBHoMObject> { ((IBHoMObject)converted).IPostprocess(transform, settings) };
            else if (converted is IEnumerable<IBHoMObject>)
                result = new List<IBHoMObject>(((IEnumerable<IBHoMObject>)converted).Select(x => x.IPostprocess(transform, settings)));
            
            return result;
        }


        /***************************************************/
        /****             Fallback Methods              ****/
        /***************************************************/

        [Description("Fallback method when no suitable FromRevit for Location is found, e.g. when it's not LocationPoint nor LocationCurve.")]
        [Input("location", "Revit Location to be converted.")]
        [Output("fromRevit", "Null resulted from no suitable Location FromRevit method.")]
        private static IGeometry FromRevit(this Location location)
        {
            return null;
        }

        /***************************************************/
    }
}

