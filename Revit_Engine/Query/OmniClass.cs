/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns OmniClass assigned to Revit family represented by RevitFilePreview.")]
        [Input("revitFilePreview", "RevitFilePreview to be queried.")]
        [Output("omniClass")]
        public static string OmniClass(this RevitFilePreview revitFilePreview)
        {
            if (revitFilePreview == null)
                return null;

            return OmniClass(revitFilePreview.XDocument());
        }

        /***************************************************/

        [Description("Returns OmniClass assigned to Revit family stored in XDocument.")]
        [Input("xDocument", "XDocument from header of Revit family file (*.rfa).")]
        [Output("omniClass")]
        public static string OmniClass(this XDocument xDocument)
        {
            if (xDocument == null || xDocument.Root == null)
                return null;

            List<XElement> elementList = xDocument.Root.Elements().ToList();
            if (elementList != null && elementList.Count > 0)
            {
                XName name = XName.Get("category", "http://www.w3.org/2005/Atom");
                elementList = elementList.FindAll(x => x.Name == name);
                if (elementList != null)
                    foreach (XElement element in elementList)
                    {
                        List<XElement> childList = element.Elements().ToList();
                        name = XName.Get("scheme", "http://www.w3.org/2005/Atom");

                        if (childList != null && childList.Find(x => x.Name == name && x.Value == "std:oc1") != null)
                        {
                            name = XName.Get("term", "http://www.w3.org/2005/Atom");
                            XElement childElement = childList.Find(x => x.Name == name);
                            if (childElement != null)
                                return childElement.Value;
                        }
                    }
            }

            return null;
        }

        /***************************************************/
    }
}




