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
using System;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****                   Update                  ****/
        /***************************************************/

        //CODE TAKEN FROM CREATE, LEFT FOR REFERENCE.
        //    element = element.SetParameters(bhomObject);
        //    if (element != null && element.Location != null)
        //    {
        //        try
        //        {
        //            Location location = element.Move(bhomObject);
        //        }
        //        catch
        //        {
        //            ObjectNotMovedWarning(bhomObject);
        //        }
        //    }

        //    if (bhomObject is IView || bhomObject is oM.Adapters.Revit.Elements.Family || bhomObject is InstanceProperties)
        //        element.Name = bhomObject.Name;

        //FOR REFERENCE, IF OBJECT IS REVITFILEPREVIEW:
        //TODO: this value should come from adapter PushType?
        //bool updateFamilies = true;
        //FamilyLoadOptions familyLoadOptions = new FamilyLoadOptions(updateFamilies);
        //if (document.LoadFamily(revitFilePreview.Path, out family))
        //{
        //    SetIdentifiers(bhomObject, family);
        //    element = family;
        //}
        //}

        /***************************************************/
        /****              UpdateProperty               ****/
        /***************************************************/

        // This method used to be called from the UpdateProperty component
        // Now it is never called as it doesn't override anymore
        // This logic should be called from the Push component instead


        /***************************************************/
        /****              Private Classes              ****/
        /***************************************************/

        private class FamilyLoadOptions : IFamilyLoadOptions
        {
            private bool m_Update;

            public FamilyLoadOptions(bool update)
            {
                this.m_Update = update;
            }

            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                if (m_Update)
                {
                    overwriteParameterValues = false;
                    return false;

                }

                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                if (m_Update)
                {
                    overwriteParameterValues = false;
                    source = FamilySource.Project;
                    return false;

                }

                overwriteParameterValues = true;
                source = FamilySource.Family;
                return true;
            }
        }

        /***************************************************/
    }
}