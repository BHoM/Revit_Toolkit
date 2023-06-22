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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Set the endpoint of a tag's leader consistently across Revit versions.")]
        [Input("tag", "A tag whose leader endpoint must be at a specific location.")]
        [Input("point", "Endpoint for the input tag's leader.")]
        public static void SetLeaderEnd(this IndependentTag tag, XYZ point)
        {
            if (tag.HasLeader && tag.LeaderEndCondition == LeaderEndCondition.Free)
            {
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021 || REVIT2022
                tag.LeaderEnd = point;
#else
                foreach (Reference reference in tag.GetTaggedReferences())
                {
                    if (tag.IsLeaderVisible(reference))
                    {
                        tag.SetLeaderEnd(reference, point);
                    }
                }
#endif
            }
        }

        /***************************************************/
    }
}