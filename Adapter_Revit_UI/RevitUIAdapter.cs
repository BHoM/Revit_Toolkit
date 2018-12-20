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

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.Adapter.Revit;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter : InternalRevitAdapter
    {
        /***************************************************/
        /**** Private Properties                        ****/
        /***************************************************/

        private Document m_Document;


        /***************************************************/
        /**** Public Constructors                       ****/
        /***************************************************/
        
        public RevitUIAdapter(Document document)
            : base()
        {
            AdapterId = BH.Engine.Adapters.Revit.Convert.AdapterId;
            Config.UseAdapterId = false;
            Config.ProcessInMemory = false;
            Config.SeparateProperties = true;
            Config.CloneBeforePush = true;
            m_Document = document;
        }


        /***************************************************/
        /**** Public Properties                        ****/
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
    }
}