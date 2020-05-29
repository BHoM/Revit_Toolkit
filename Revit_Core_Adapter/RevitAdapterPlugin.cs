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

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BH.Adapter;
using BH.Adapter.Revit;
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections.Generic;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitAdapterPlugin : BHoMAdapter,  IInternalRevitAdapter
    {
        /***************************************************/
        /****               Private Fields              ****/
        /***************************************************/

        private UIControlledApplication m_UIControlledApplication;

        private Document m_Document;


        /***************************************************/
        /****            Public Constructors            ****/
        /***************************************************/
        
        public RevitAdapterPlugin(UIControlledApplication uIControlledApplication, Document document)
            : base()
        {
            AdapterIdName = BH.Engine.Adapters.Revit.Convert.AdapterIdName;
            m_AdapterSettings.UseAdapterId = false;

            m_Document = document;
            m_UIControlledApplication = uIControlledApplication;
            
            AdapterComparers = new Dictionary<Type, object>
            {
                //{typeof(ISectionProperty), new BHoMObjectNameOrToStringComparer() },
                //{typeof(IProfile), new BHoMObjectNameOrToStringComparer() },
                //{typeof(ISurfaceProperty), new BHoMObjectNameOrToStringComparer() },
                //{typeof(Material), new BHoMObjectNameComparer() },
                //{typeof(Level), new BHoMObjectNameComparer() },
            };

            DependencyTypes = new Dictionary<Type, List<Type>>
            {
                {typeof(oM.Adapters.Revit.Elements.Viewport), new List<Type> { typeof(oM.Adapters.Revit.Elements.Sheet), typeof(oM.Adapters.Revit.Elements.ViewPlan) } },
                {typeof(oM.Adapters.Revit.Elements.Sheet), new List<Type> { typeof(oM.Adapters.Revit.Elements.ViewPlan)} }
                //{typeof(ISectionProperty), new List<Type> { typeof(Material), typeof(IProfile) } },
                //{typeof(PanelPlanar), new List<Type> { typeof(ISurfaceProperty), typeof(Level) } },
                //{typeof(ISurfaceProperty), new List<Type> { typeof(Material) } }
            };
        }


        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        public Document Document
        {
            get
            {
                return m_Document;
            }
        }

        /***************************************************/

        public UIDocument UIDocument
        {
            get
            {
                if (m_Document == null)
                    return null;

                return new UIDocument(m_Document);
            }
        }

        /***************************************************/

        public UIControlledApplication UIControlledApplication
        {
            get
            {
                return m_UIControlledApplication;
            }
        }

        /***************************************************/

        public RevitSettings RevitSettings { get; set; }

        /***************************************************/
    }
}