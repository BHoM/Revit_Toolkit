/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static TagInformation TagInformationFromRevit(this IndependentTag tag, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            TagInformation tagInformation = refObjects.GetValue<TagInformation>(tag.Id);
            if (tagInformation != null)
                return tagInformation;

            if (tag == null || tag.IsOrphaned)
                return null;

            using (Transaction t = new Transaction(tag.Document, "temp"))
            {
                t.Start();
                View view = tag.Document.GetElement(tag.OwnerViewId) as View;
                tagInformation = new TagInformation();
                tagInformation.Name = tag.Name;
                tagInformation.ViewId = tag.OwnerViewId.IntegerValue;
                tagInformation.HasLeader = tag.HasLeader;

                if (tag.Pinned)
                    tag.Pinned = false;

                if (!tag.HasLeader)
                    tag.HasLeader = true;

                tag.LeaderEndCondition = LeaderEndCondition.Free;
                tag.LeaderEnd = tag.TagHeadPosition;
                tag.LeaderElbow = tag.TagHeadPosition;

                tag.Document.Regenerate();

                BoundingBoxXYZ tagBox = tag.get_BoundingBox(view);
                tagInformation.Width = (tagBox.Max.X - tagBox.Min.X).ToSI(UnitType.UT_Length);
                tagInformation.Height = (tagBox.Max.Y - tagBox.Min.Y).ToSI(UnitType.UT_Length);
                tagInformation.Location = (tagBox.Max.PointFromRevit() + tagBox.Min.PointFromRevit()) / 2;

                Element element = tag.Document.GetElement(tag.TaggedLocalElementId);
                tagInformation.TaggedElementId = tag.TaggedLocalElementId.IntegerValue;
                if ((element.Location as LocationCurve) != null)
                    tagInformation.IsTaggedElementLinear = true;

                Options options = new Options();
                options.ComputeReferences = false;
                options.IncludeNonVisibleObjects = false;
                options.View = view;

                Transform transform = null;
                if (element is FamilyInstance)
                    transform = (element as FamilyInstance).GetTotalTransform();

                tagInformation.TaggedElementCurves = element.get_Geometry(options).Curves(transform, settings);
                t.RollBack();
            }

            //Set identifiers & custom data
            tagInformation.SetIdentifiers(tag);
            tagInformation.SetCustomData(tag);

            refObjects.AddOrReplace(tag.Id, tagInformation);
            return tagInformation;
        }

        /***************************************************/
    }
}

