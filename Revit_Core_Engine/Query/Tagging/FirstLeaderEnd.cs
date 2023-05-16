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
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets the location of a tag's leader endpoint or of its first leader endpoint if it has multiple leaders.")]
        [Input("tag", "A tag to query for the first leader endpoint location.")]
        [Output("xyz", "The location of a tag's leader endpoint or of its first leader endpoint if it has multiple leaders.")]
        public static XYZ FirstLeaderEnd(this IndependentTag tag)
        {
            if (tag.HasLeader && tag.LeaderEndCondition == LeaderEndCondition.Free)
            {
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021 || REVIT2022
                return tag.LeaderEnd;
#else
                foreach (Reference reference in tag.GetTaggedReferences())
                {
                    if (tag.IsLeaderVisible(reference))
                    {
                        return tag.GetLeaderEnd(reference);

                    }
                }
#endif
            }

            return null;
        }

        /***************************************************/
    }
}

