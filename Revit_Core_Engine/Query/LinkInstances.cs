///*
// * This file is part of the Buildings and Habitats object Model (BHoM)
// * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
// *
// * Each contributor holds copyright over their respective contributions.
// * The project versioning (Git) records all such contribution source information.
// *                                           
// *                                                                              
// * The BHoM is free software: you can redistribute it and/or modify         
// * it under the terms of the GNU Lesser General Public License as published by  
// * the Free Software Foundation, either version 3.0 of the License, or          
// * (at your option) any later version.                                          
// *                                                                              
// * The BHoM is distributed in the hope that it will be useful,              
// * but WITHOUT ANY WARRANTY; without even the implied warranty of               
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
// * GNU Lesser General Public License for more details.                          
// *                                                                            
// * You should have received a copy of the GNU Lesser General Public License     
// * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
// */

//using Autodesk.Revit.DB;
//using System.Collections.Generic;
//using System.Linq;

//namespace BH.Revit.Engine.Core
//{
//    public static partial class Query
//    {
//        /***************************************************/
//        /****              Public methods               ****/
//        /***************************************************/

//        public static List<RevitLinkInstance> LinkInstances(this Document hostDocument, string identifier)
//        {
//            List<RevitLinkInstance> allInstances = new FilteredElementCollector(hostDocument).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();
//            string linkName = identifier.ToLower();

//            // Try get the link doc by its link instance Id
//            int id;
//            if (int.TryParse(linkName, out id))
//            {
//                RevitLinkInstance instance = hostDocument.GetElement(new ElementId(id)) as RevitLinkInstance;
//                if (instance != null)
//                    return new List<RevitLinkInstance> { instance };
//            }

//            // Get the links by link name parameter.
//            List<RevitLinkInstance> result = allInstances.Where(x => x.LookupParameterString(BuiltInParameter.RVT_LINK_INSTANCE_NAME).ToLower() == linkName).ToList();

//            // Get the links by file name or path.
//            bool suffix = false;
//            if (!linkName.EndsWith(".rvt"))
//            {
//                linkName += ".rvt";
//                suffix = true;
//            }

//            List<RevitLinkInstance> fromPath;
//            if (linkName.Contains("\\"))
//                fromPath = allInstances.Where(x => x.GetLinkDocument().PathName.ToLower() == linkName).ToList();
//            else
//                fromPath = allInstances.Where(x => (hostDocument.GetElement(x.GetTypeId()) as RevitLinkType)?.Name?.ToLower() == linkName).ToList();

//            fromPath = fromPath.Where(x => result.All(y => y.Id.IntegerValue != x.Id.IntegerValue)).ToList();
//            result.AddRange(fromPath);

//            if (suffix && fromPath.Count != 0)
//                BH.Engine.Reflection.Compute.RecordWarning($"Link name {linkName} inside a link request does not end with .rvt - the suffix has been added to find the correspondent documents.");

//            return result;
//        }
            
//        /***************************************************/
//    }
//}

