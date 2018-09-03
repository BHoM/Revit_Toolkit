using BH.oM.Revit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace BH.Engine.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static RevitFilePreview RevitFilePreview(string Path)
        {
            XDocument aXDocument = null;

            if (!File.Exists(Path))
                return null;

            StorageInfo aStorageInfo = (StorageInfo)InvokeStorageRootMethod(null, "Open", Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (aStorageInfo != null)
            {
                StreamInfo[] aStreamInfoArray = aStorageInfo.GetStreams();
                List<string> aNames = aStreamInfoArray.ToList().ConvertAll(x => x.Name);
                foreach (StreamInfo aStreamInfo in aStreamInfoArray)
                {
                    if (aStreamInfo.Name.Equals("PartAtom"))
                    {
                        byte[] aFileInfoData = ParseStreamInfo(aStreamInfo);
                        aXDocument = XDocument.Parse(Encoding.Default.GetString(aFileInfoData));
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

        private static object InvokeStorageRootMethod(StorageInfo StorageInfoRoot, string MethodName, params object[] MethodArgs)
        {
            BindingFlags aBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            Type aStorageRootType = typeof(StorageInfo).Assembly.GetType("System.IO.Packaging.StorageRoot", true, false);
            object aResult = aStorageRootType.InvokeMember(MethodName, aBindingFlags, null, StorageInfoRoot, MethodArgs);
            return aResult;
        }

        private static byte[] ParseStreamInfo(StreamInfo StreamInfo)
        {
            byte[] aResult = null;
            try
            {
                using (Stream aStream = StreamInfo.GetStream(FileMode.Open, FileAccess.Read))
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

        private static void CloseStorageInfo(StorageInfo StorageInfo)
        {
            InvokeStorageRootMethod(StorageInfo, "Close");
        }
    }
}

