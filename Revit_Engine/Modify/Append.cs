﻿using System.Collections.Generic;
using BH.oM.Adapters.Revit.Generic;
using System.IO;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Adds new directory to FamilyLibrary.")]
        [Input("directory", "Directory from where famlies will be loaded if not exists in model")]
        [Input("topDirectoryOnly", "Search through top dilectory folder and skip subfolders")]
        [Output("FamilyLibrary")]
        public static FamilyLibrary Append(this FamilyLibrary familyLibrary, string directory, bool topDirectoryOnly = false)
        {
            if (familyLibrary == null)
                return null;

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return familyLibrary;

            FamilyLibrary aFamilyLibrary = familyLibrary.GetShallowClone() as FamilyLibrary;

            DirectoryInfo aDirectoryInfo = new DirectoryInfo(directory);

            SearchOption aSearchOption = SearchOption.AllDirectories;
            if (topDirectoryOnly)
                aSearchOption = SearchOption.TopDirectoryOnly;

            FileInfo[] aFileInfos = aDirectoryInfo.GetFiles("*.rfa", aSearchOption);
            foreach (FileInfo aFileInfo in aFileInfos)
                aFamilyLibrary = aFamilyLibrary.Append(aFileInfo.FullName);

            return aFamilyLibrary;
        }

        /***************************************************/

        [Description("Adds new path to FamilyLibrary.")]
        [Input("path", "path of Revit file")]
        [Output("FamilyLibrary")]
        public static FamilyLibrary Append(this FamilyLibrary familyLibrary, string path)
        {
            if (familyLibrary == null)
                return null;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return familyLibrary;

            FamilyLibrary aFamilyLibrary = familyLibrary.GetShallowClone() as FamilyLibrary;

            if (aFamilyLibrary.Dictionary == null)
                aFamilyLibrary.Dictionary = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            string aFamilyName = Path.GetFileNameWithoutExtension(path);
            RevitFilePreview aRevitFilePreview = Create.RevitFilePreview(path);

            string aCategoryName = aRevitFilePreview.CategoryName();

            Dictionary<string, Dictionary<string, string>> aDictionary_Category = null;

            if (!aFamilyLibrary.Dictionary.TryGetValue(aCategoryName, out aDictionary_Category))
            {
                aDictionary_Category = new Dictionary<string, Dictionary<string, string>>();
                aFamilyLibrary.Dictionary.Add(aCategoryName, aDictionary_Category);
            }


            foreach (string aTypeName in aRevitFilePreview.FamilyTypeNames())
            {
                Dictionary<string, string> aDictionary_Type = null;
                if (!aDictionary_Category.TryGetValue(aTypeName, out aDictionary_Type))
                {
                    aDictionary_Type = new Dictionary<string, string>();
                    aDictionary_Category.Add(aTypeName, aDictionary_Type);
                }

                if (!aDictionary_Type.ContainsKey(aFamilyName))
                    aDictionary_Type.Add(aFamilyName, path);
            }

            return aFamilyLibrary;
        }

        /***************************************************/
    }
}
