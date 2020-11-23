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
using BH.Engine.Base;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Adds new directory to existing FamilyLibrary.")]
        [Input("familyLibrary", "FamilyLibrary to be extended.")]
        [Input("directory", "Directory to be added.")]
        [Input("topDirectoryOnly", "If true, add top directory folder and skip subfolders.")]
        [Output("familyLibrary")]
        public static FamilyLibrary Append(this FamilyLibrary familyLibrary, string directory, bool topDirectoryOnly = false)
        {
            if (familyLibrary == null)
                return null;

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return familyLibrary;

            FamilyLibrary famLibrary = familyLibrary.ShallowClone();

            DirectoryInfo directoryInfo = new DirectoryInfo(directory);

            SearchOption searchOption = SearchOption.AllDirectories;
            if (topDirectoryOnly)
                searchOption = SearchOption.TopDirectoryOnly;

            FileInfo[] fileInfos = directoryInfo.GetFiles("*.rfa", searchOption);
            foreach (FileInfo fileInfo in fileInfos)
                famLibrary = famLibrary.Append(fileInfo.FullName);

            return famLibrary;
        }

        /***************************************************/

        [Description("Adds new path to existing FamilyLibrary.")]
        [Input("familyLibrary", "FamilyLibrary to be extended.")]
        [Input("path", "Path of Revit file to be added.")]
        [Output("familyLibrary")]
        public static FamilyLibrary Append(this FamilyLibrary familyLibrary, string path)
        {
            if (familyLibrary == null)
                return null;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return familyLibrary;

            FamilyLibrary famLibrary = familyLibrary.ShallowClone();

            if (famLibrary.Dictionary == null)
                famLibrary.Dictionary = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            string familyName = Path.GetFileNameWithoutExtension(path);
            RevitFilePreview revitFilePreview = Create.RevitFilePreview(path);

            string categoryName = revitFilePreview.CategoryName();

            Dictionary<string, Dictionary<string, string>> categoryDictionary = null;

            if (!famLibrary.Dictionary.TryGetValue(categoryName, out categoryDictionary))
            {
                categoryDictionary = new Dictionary<string, Dictionary<string, string>>();
                famLibrary.Dictionary.Add(categoryName, categoryDictionary);
            }

            foreach (string typeName in revitFilePreview.FamilyTypeNames())
            {
                Dictionary<string, string> typeDictionary = null;
                if (!categoryDictionary.TryGetValue(typeName, out typeDictionary))
                {
                    typeDictionary = new Dictionary<string, string>();
                    categoryDictionary.Add(typeName, typeDictionary);
                }

                if (!typeDictionary.ContainsKey(familyName))
                    typeDictionary.Add(familyName, path);
            }

            return famLibrary;
        }

        /***************************************************/
    }
}

