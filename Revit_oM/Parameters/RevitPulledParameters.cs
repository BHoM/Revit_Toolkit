/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BH.oM.Adapters.Revit.Parameters
{
    [Description("An entity containing parameters attached to the BHoM object on pull from Revit, when a Revit element was converted to that object.")]
    public class RevitPulledParameters : BHoMObject, IRevitParameterFragment, IImmutable
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Collection of parameters pulled from Revit.")]
        public virtual IList<RevitParameter> Parameters { get; }


        /***************************************************/
        /****            Public Constructors            ****/
        /***************************************************/

        public RevitPulledParameters(IEnumerable<RevitParameter> parameters)
        {
            Parameters = new ReadOnlyCollection<RevitParameter>(parameters.ToList());
        }

        /***************************************************/
    }
}




