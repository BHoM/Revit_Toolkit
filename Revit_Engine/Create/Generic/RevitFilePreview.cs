using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Revit File Preview Class which stores basic information about Revit File such as Family Category, Familiy Type Names etc.")]
        [Input("path", "Path to the Revit file")]
        [Output("RevitFilePreview")]
        public static RevitFilePreview RevitFilePreview(string path)
        {
            XDocument aXDocument = null;

            if (!File.Exists(path))
                return null;

            StorageInfo aStorageInfo = (StorageInfo)InvokeStorageRootMethod(null, "Open", path, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (aStorageInfo != null)
            {
                StreamInfo[] aStreamInfoArray = aStorageInfo.GetStreams();
                List<string> aNames = aStreamInfoArray.ToList().ConvertAll(x => x.Name);
                foreach (StreamInfo aStreamInfo in aStreamInfoArray)
                {
                    if (aStreamInfo.Name.Equals("PartAtom"))
                    {
                        byte[] aBytes = ParseStreamInfo(aStreamInfo);
                        try
                        {
                            aXDocument = XDocument.Parse(Encoding.UTF8.GetString(aBytes));
                        }
                        catch
                        {
                            aXDocument = null;
                        }

                        if(aXDocument == null)
                        {
                            try
                            {
                                aXDocument = XDocument.Parse(Encoding.Default.GetString(aBytes));
                            }
                            catch
                            {
                                aXDocument = null;
                            }
                        }
                        
                        break;
                    }
                }

                CloseStorageInfo(aStorageInfo);
            }

            RevitFilePreview aRevitFilePreview = new RevitFilePreview()
            {
                XDocument = aXDocument
            };

            return aRevitFilePreview;
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

