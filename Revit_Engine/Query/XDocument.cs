/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Retrieves XDocument from header of a Revit family file (.rfa) wrapped by RevitFilePreview.")]
        [Input("revitFilePreview", "RevitFilePreview to be queried.")]
        [Output("xDocument")]
        public static XDocument XDocument(this RevitFilePreview revitFilePreview)
        {
            return revitFilePreview?.Path.XDocument();
        }

        /***************************************************/

        [Description("Retrieves XDocument from header of a Revit family file (.rfa) under a given path.")]
        [Input("path", "Path to the .rfa file to be queried.")]
        [Output("xDocument")]
        public static XDocument XDocument(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (!File.Exists(path))
            {
                BH.Engine.Base.Compute.RecordError($"Revit file does not exist under path {path}.");
                return null;
            }

            StorageInfo storageInfo = (StorageInfo)InvokeStorageRootMethod(null, "Open", path, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (storageInfo != null)
            {
                XDocument document = null;
                StreamInfo[] streamInfo = storageInfo.GetStreams();
                List<string> names = streamInfo.ToList().ConvertAll(x => x.Name);
                foreach (StreamInfo sInfo in streamInfo)
                {
                    if (sInfo.Name.Equals("PartAtom"))
                    {
                        byte[] bytes = ParseStreamInfo(sInfo);

                        try
                        {
                            document = System.Xml.Linq.XDocument.Parse(Encoding.UTF8.GetString(bytes));
                        }
                        catch
                        {
                            BH.Engine.Base.Compute.RecordError($"Internal error occurred when attempting to convert a file under path {path} into RevitFilePreview.");
                            return null;
                        }

                        if (document == null)
                        {
                            try
                            {
                                document = System.Xml.Linq.XDocument.Parse(Encoding.Default.GetString(bytes));
                            }
                            catch
                            {
                                BH.Engine.Base.Compute.RecordError($"Internal error occurred when attempting to convert a file under path {path} into RevitFilePreview.");
                                return null;
                            }
                        }

                        break;
                    }
                }

                CloseStorageInfo(storageInfo);
                return document;
            }

            return null;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static object InvokeStorageRootMethod(StorageInfo storageInfoRoot, string methodName, params object[] methodArgs)
        {
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            Type storageType = typeof(StorageInfo).Assembly.GetType("System.IO.Packaging.StorageRoot", true, false);
            object result = storageType.InvokeMember(methodName, bindingFlags, null, storageInfoRoot, methodArgs);
            return result;
        }

        /***************************************************/

        private static byte[] ParseStreamInfo(StreamInfo streamInfo)
        {
            byte[] result = null;
            try
            {
                using (Stream stream = streamInfo.GetStream(FileMode.Open, FileAccess.Read))
                {
                    result = new byte[stream.Length];
                    stream.Read(result, 0, result.Length);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                result = null;
            }
        }

        /***************************************************/

        private static void CloseStorageInfo(StorageInfo storageInfo)
        {
            InvokeStorageRootMethod(storageInfo, "Close");
        }

        /***************************************************/
    }
}





