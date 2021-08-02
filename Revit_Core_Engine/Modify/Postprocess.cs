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
using BH.Engine.Adapters.Revit;
using BH.Engine.Base;
using BH.Engine.Revit;
using BH.Engine.Spatial;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        [Description("Postprocess the BHoM object orginating from the convert of a Revit element.")]
        [Input("obj", "BHoM object to postprocess.")]
        [Input("transform", "Optional, transformation to be applied to the BHoM object.")]
        [Input("settings", "Revit adapter settings to be used while performing the postprocessing.")]
        [Output("object", "Postprocessed BHoM object.")]
        public static IBHoMObject IPostprocess(this IBHoMObject obj, Transform transform, RevitSettings settings)
        {
            return Postprocess(obj as dynamic, transform, settings);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Postprocess the BHoM IElement orginating from the convert of a Revit element.")]
        [Input("element", "BHoM IElement to postprocess.")]
        [Input("transform", "Optional, transformation to be applied to the BHoM IElement.")]
        [Input("settings", "Revit adapter settings to be used while performing the postprocessing.")]
        [Output("element", "Postprocessed BHoM IElement.")]
        public static IElement Postprocess(this IElement element, Transform transform, RevitSettings settings)
        {
            if (element == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Unable to postprocess a null BHoM object.");
                return null;
            }

            settings = settings.DefaultIfNull();

            if (transform?.IsIdentity == false)
                element = element.ITransform(transform.FromRevit(), settings.DistanceTolerance);

            return element;
        }

        /***************************************************/

        [Description("Postprocess the BHoM ModelInstance orginating from the convert of a Revit element.")]
        [Input("instance", "BHoM ModelInstance to postprocess.")]
        [Input("transform", "Optional, transformation to be applied to the BHoM ModelInstance.")]
        [Input("settings", "Revit adapter settings to be used while performing the postprocessing.")]
        [Output("instance", "Postprocessed BHoM ModelInstance.")]
        public static IInstance Postprocess(this ModelInstance instance, Transform transform, RevitSettings settings)
        {
            if (instance == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Unable to postprocess a null BHoM object.");
                return null;
            }

            settings = settings.DefaultIfNull();

            if (transform?.IsIdentity == false)
                instance = instance.Transform(transform.FromRevit(), settings.DistanceTolerance) as ModelInstance;

            return instance;
        }

        /***************************************************/

        [Description("Postprocess the BHoM Level orginating from the convert of a Revit element.")]
        [Input("level", "BHoM Level to postprocess.")]
        [Input("transform", "Optional, transformation to be applied to the BHoM Level.")]
        [Input("settings", "Revit adapter settings to be used while performing the postprocessing.")]
        [Output("level", "Postprocessed BHoM Level.")]
        public static BH.oM.Geometry.SettingOut.Level Postprocess(this BH.oM.Geometry.SettingOut.Level level, Transform transform, RevitSettings settings)
        {
            if (level == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Unable to postprocess a null BHoM object.");
                return null;
            }
            
            if (transform?.IsIdentity == false)
            {
                level = level.ShallowClone();
                level.Elevation += transform.Origin.Z.ToSI(UnitType.UT_Length);
            }

            return level;
        }


        /***************************************************/
        /****             Fallback methods              ****/
        /***************************************************/
        
        private static IBHoMObject Postprocess(this IBHoMObject obj, TransformMatrix transform, RevitSettings settings)
        {
            return obj;
        }

        /***************************************************/
    }
}

