/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.Text;
using System.IO.Packaging;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using System.ComponentModel;
using System.Xml.Linq;

using BH.oM.Reflection.Attributes;
using BH.oM.Adapters.Revit.Generic;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Retrieves. XDocument from RevitFilePreview")]
        [Input("revitFilePreview", "RevitFilePreview")]
        [Output("XDocument")]
        public static XDocument XDocument(this RevitFilePreview revitFilePreview)
        {
            if (revitFilePreview == null)
                return null;

            string aPath = revitFilePreview.Path;

            if (string.IsNullOrWhiteSpace(revitFilePreview.Path) || !File.Exists(revitFilePreview.Path))
                return null;

            if (File.Exists(revitFilePreview.Path))
            {
                StorageInfo aStorageInfo = (StorageInfo)InvokeStorageRootMethod(null, "Open", aPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                if (aStorageInfo != null)
                {
                    XDocument aXDocument = null;
                    StreamInfo[] aStreamInfoArray = aStorageInfo.GetStreams();
                    List<string> aNames = aStreamInfoArray.ToList().ConvertAll(x => x.Name);
                    foreach (StreamInfo aStreamInfo in aStreamInfoArray)
                    {
                        if (aStreamInfo.Name.Equals("PartAtom"))
                        {
                            byte[] aBytes = ParseStreamInfo(aStreamInfo);
                            
                            try
                            {
                                aXDocument = System.Xml.Linq.XDocument.Parse(Encoding.UTF8.GetString(aBytes));
                            }
                            catch
                            {
                                return null;
                            }

                            if (aXDocument == null)
                            {
                                try
                                {
                                    aXDocument = System.Xml.Linq.XDocument.Parse(Encoding.Default.GetString(aBytes));
                                }
                                catch
                                {
                                    return null;
                                }
                            }

                            break;
                        }
                    }

                    CloseStorageInfo(aStorageInfo);
                    return aXDocument;
                }
            }

            return null;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static object InvokeStorageRootMethod(StorageInfo storageInfoRoot, string methodName, params object[] methodArgs)
        {
            BindingFlags aBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            Type aStorageRootType = typeof(StorageInfo).Assembly.GetType("System.IO.Packaging.StorageRoot", true, false);
            object aResult = aStorageRootType.InvokeMember(methodName, aBindingFlags, null, storageInfoRoot, methodArgs);
            return aResult;
        }

        /***************************************************/

        private static byte[] ParseStreamInfo(StreamInfo streamInfo)
        {
            byte[] aResult = null;
            try
            {
                using (Stream aStream = streamInfo.GetStream(FileMode.Open, FileAccess.Read))
                {
                    aResult = new byte[aStream.Length];
                    aStream.Read(aResult, 0, aResult.Length);
                    return aResult;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                aResult = null;
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