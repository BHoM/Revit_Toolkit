/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Architecture.Elements;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Physical.Elements;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts Revit element type representation from a given BHoM object pulled from Revit.")]
        [Input("bHoMObject", "BHoMObject to be queried for Revit element type representation.")]
        [Output("elementType", "Revit element type representation extracted from the input BHoM object pulled from Revit.")]
        public static IBHoMObject IGetRevitElementType(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not extract Revit element type representation from a null BHoM object.");
                return null;
            }

            return (GetRevitElementType(bHoMObject as dynamic));
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static IBHoMObject GetRevitElementType(this Ceiling bHoMObject)
        {
            return bHoMObject.Construction;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.Environment.Elements.Panel bHoMObject)
        {
            return bHoMObject.Construction;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.Environment.Elements.Opening bHoMObject)
        {
            return bHoMObject.OpeningConstruction;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.Facade.Elements.FrameEdge bHoMObject)
        {
            return bHoMObject.FrameEdgeProperty;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.Facade.Elements.Opening bHoMObject)
        {
            return bHoMObject.OpeningConstruction;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.Facade.Elements.Panel bHoMObject)
        {
            return bHoMObject.Construction;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.MEP.System.CableTray bHoMObject)
        {
            return bHoMObject.SectionProperty;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.MEP.System.Duct bHoMObject)
        {
            return bHoMObject.SectionProperty;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.MEP.System.Pipe bHoMObject)
        {
            return bHoMObject.SectionProperty;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this IFramingElement bHoMObject)
        {
            return bHoMObject.Property;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this Door bHoMObject)
        {
            return bHoMObject.Construction;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this ISurface bHoMObject)
        {
            return bHoMObject.Construction;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this Window bHoMObject)
        {
            return bHoMObject.Construction;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this IInstance bHoMObject)
        {
            return bHoMObject.Properties;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this Sheet bHoMObject)
        {
            return bHoMObject.InstanceProperties;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this Viewport bHoMObject)
        {
            return bHoMObject.InstanceProperties;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this ViewPlan bHoMObject)
        {
            return bHoMObject.InstanceProperties;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.Structure.Elements.Bar bHoMObject)
        {
            return bHoMObject.SectionProperty;
        }

        /***************************************************/

        private static IBHoMObject GetRevitElementType(this BH.oM.Structure.Elements.Panel bHoMObject)
        {
            return bHoMObject.Property;
        }


        /***************************************************/
        /****              Fallback methods             ****/
        /***************************************************/

        private static IBHoMObject GetRevitElementType(this IBHoMObject bHoMObject)
        {
            RevitTypeFragment fragment = bHoMObject.FindFragment<RevitTypeFragment>();
            if (fragment == null)
                BH.Engine.Base.Compute.RecordError($"BHoM object of type {bHoMObject.GetType().FullName} does not store the information about correspondent Revit element type.");

            return fragment;
        }

        /***************************************************/
    }
}



