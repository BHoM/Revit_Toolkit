/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Removes Revit-related identifiers fragment from a BHoM object.")]
        [Input("bHoMObject", "BHoMObject to be cleaned.")]
        [Output("bHoMObject")]
        public static IBHoMObject RemoveIdentifiers(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            if (bHoMObject.Fragments == null)
                return bHoMObject;

            if (bHoMObject.Fragments.Any(x => x is RevitIdentifiers))
            {
                IBHoMObject obj = bHoMObject.ShallowClone();
                obj.Fragments = new FragmentSet(bHoMObject.Fragments.Where(x => !(x is RevitIdentifiers)).ToList());
                return obj;
            }
            else
                return bHoMObject;
        }

        /***************************************************/
    }
}
