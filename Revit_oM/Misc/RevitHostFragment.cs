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

using BH.oM.Base;
using System.ComponentModel;

namespace BH.oM.Revit
{
    [Description("Fragment containing the information about the Revit element that hosts the Revit element correspondent to the BHoM object that carries this.")]
    public class RevitHostFragment : IFragment, IImmutable
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("ElementId of the Revit element that hosts the Revit element correspondent to the BHoM object that carries this.")]
        public virtual int HostId { get; } = -1;

        [Description("Name of the link document, if the host Revit element is linked.")]
        public virtual string LinkDocument { get; } = "";


        /***************************************************/
        /****            Public Constructors            ****/
        /***************************************************/

        public RevitHostFragment(int hostId, string linkDocument = "")
        {
            HostId = hostId;
            LinkDocument = linkDocument;
        }

        /***************************************************/
    }
}
