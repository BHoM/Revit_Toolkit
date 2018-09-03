﻿using BH.oM.Revit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BH.Engine.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string OmniClass(this RevitFilePreview RevitFilePreview)
        {
            if (RevitFilePreview == null || RevitFilePreview.XDocument == null)
                return null;

            XDocument aXDocument = RevitFilePreview.XDocument;

            if (aXDocument != null && aXDocument.Root != null)
            {
                List<XElement> aXElementList = aXDocument.Root.Elements().ToList();
                if (aXElementList != null && aXElementList.Count > 0)
                {
                    XName aName = XName.Get("category", "http://www.w3.org/2005/Atom");
                    aXElementList = aXElementList.FindAll(x => x.Name == aName);
                    if (aXElementList != null)
                        foreach (XElement aXElement in aXElementList)
                        {
                            List<XElement> aChildXElementList = aXElement.Elements().ToList();
                            aName = XName.Get("scheme", "http://www.w3.org/2005/Atom");

                            if (aChildXElementList != null && aChildXElementList.Find(x => x.Name == aName && x.Value == "std:oc1") != null)
                            {
                                aName = XName.Get("term", "http://www.w3.org/2005/Atom");
                                XElement aChildXElement = aChildXElementList.Find(x => x.Name == aName);
                                if (aChildXElement != null)
                                    return aChildXElement.Value;
                            }
                        }
                }
            }
            return null;
        }

        /***************************************************/
    }
}

