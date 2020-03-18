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
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public class FamilyLoadOptions : IFamilyLoadOptions
    {
        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private bool m_OverwriteParameterValues;
        private bool m_OverwriteFamily;


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public FamilyLoadOptions(FamilyLoadSettings familyLoadSettings)
        {
            m_OverwriteParameterValues = familyLoadSettings.OverwriteParameterValues;
            m_OverwriteFamily = familyLoadSettings.OverwriteFamily;
        }

        /***************************************************/

        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = m_OverwriteParameterValues;
            return m_OverwriteFamily;
        }

        /***************************************************/

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            overwriteParameterValues = m_OverwriteParameterValues;
            if (m_OverwriteFamily)
            {
                source = FamilySource.Family;
                return true;
            }
            else
            {
                source = FamilySource.Project;
                return false;
            }
        }

        /***************************************************/
    }
}


