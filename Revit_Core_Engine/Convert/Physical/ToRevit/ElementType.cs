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
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections.Generic;
using BH.Engine.Reflection;
using BH.oM.Physical.FramingProperties;
using BH.oM.Spatial.ShapeProfiles;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static FamilySymbol ToRevitElementType(this IFramingElementProperty framingElementProperty, Document document, IEnumerable<BuiltInCategory> categories = null, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (framingElementProperty == null || document == null)
                return null;

            //if a FamilySymbol matching the FramingElementProperty is already in model, use it.
            FamilySymbol familySymbol = refObjects.GetValue<FamilySymbol>(document, framingElementProperty.BHoM_Guid);
            if (familySymbol != null)
                return familySymbol;


            settings = settings.DefaultIfNull();

            //Try to find a FamilySymbol matching the FramingElementProperty in the family library files specified in the revit config
            familySymbol = framingElementProperty.ElementType(document, categories, settings) as FamilySymbol;
            if (familySymbol != null)
            {
                // Copy parameters from BHoM object to Revit element
                familySymbol.CopyParameters(framingElementProperty, settings);

                refObjects.AddOrReplace(framingElementProperty, familySymbol);
                return familySymbol;
            }

            //Make a new family (not implemented)
            familySymbol = NewFamily(framingElementProperty as ConstantFramingProperty, document, categories, settings);
            if (familySymbol != null)
                return familySymbol;

            //Give up
            BH.Engine.Reflection.Compute.RecordWarning($"No FamilySymbol could be found in the model matching {framingElementProperty.Name}, and no FamilySymbol could be found in the specified Family Library File.");
            return null;

        }

        /***************************************************/

        public static HostObjAttributes ToRevitElementType(this oM.Physical.Constructions.IConstruction construction, Document document, IEnumerable<BuiltInCategory> categories = null, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (construction == null || document == null)
                return null;

            HostObjAttributes elementType = refObjects.GetValue<HostObjAttributes>(document, construction.BHoM_Guid);
            if (elementType != null)
                return elementType;

            settings = settings.DefaultIfNull();

            elementType = construction.ElementType(document, categories, settings) as HostObjAttributes;
            if (elementType == null)
                return null;

            // Copy parameters from BHoM object to Revit element
            elementType.CopyParameters(construction, settings);

            refObjects.AddOrReplace(construction, elementType);
            return elementType;
        }

        /***************************************************/

        public static FamilySymbol NewColumnFamily(ConstantFramingProperty framingElementProperty, Document document, IEnumerable<BuiltInCategory> categories = null, RevitSettings settings = null)
        {
            //Create a new family Document
            Autodesk.Revit.ApplicationServices.Application app = document.Application;
            
            Document familyDoc = app.NewFamilyDocument("myFavoriteColumnTemplate"); //what family template to grab?

            string name = framingElementProperty.Name;

            //Get the profile curve and convert to Revit craps
            FreeFormProfile profile = framingElementProperty.Profile as FreeFormProfile;
            CurveArray curves = new CurveArray() { };
            foreach(Curve curve in profile.Edges)
            {
                curves.Append(curve);
            }
            CurveArrArray profileCurves = new CurveArrArray() { };
            profileCurves.Append(curves);

            //Add the other bits
            SketchPlane lowerRefLevel = null;
            SketchPlane upperRefLevel = null;
            View sideView = null;
            double height = 1;

            //Create the Extrusion
            Extrusion unitColumn = familyDoc.FamilyCreate.NewExtrusion(true, profileCurves, lowerRefLevel, height);
            familyDoc.FamilyCreate.NewAlignment(sideView, unitColumn.top, upperRefLevel); //get top reference of extrusion?

            FamilyManager famMgr = familyDoc.FamilyManager;
            famMgr.NewType(name);

            Family fam = document.LoadFamily(familyDoc);
            FamilySymbol familySymbol = null; // get familySymbol from loaded family?
            familyDoc.Close();

            return familySymbol;
        }
    }
}

