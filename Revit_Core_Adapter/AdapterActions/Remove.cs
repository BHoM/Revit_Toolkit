/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Data.Requests;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****     Revit side of Revit_Adapter Remove    ****/
        /***************************************************/

        public override int Remove(IRequest request, ActionConfig actionConfig = null)
        {
            // Check the document
            UIDocument uiDocument = this.UIDocument;
            Document document = this.Document;
            if (document == null)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be removed because Revit Document is null (possibly there is no open documents in Revit).");
                return 0;
            }

            if (document.IsReadOnly)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be removed because Revit Document is read only.");
                return 0;
            }

            if (document.IsModifiable)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be removed because another transaction is open in Revit.");
                return 0;
            }

            // Set config
            RevitRemoveConfig removeConfig = actionConfig as RevitRemoveConfig;
            if (removeConfig == null)
            {
                BH.Engine.Base.Compute.RecordNote("Revit Remove Config has not been specified. Default Revit Remove Config is used.");
                removeConfig = new RevitRemoveConfig();
            }

            // Suppress warnings
            if (UIControlledApplication != null && removeConfig.SuppressFailureMessages)
                UIControlledApplication.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;

            // Remove the objects based on the request
            int result = 0;
            using (Transaction transaction = new Transaction(document, "BHoM Remove"))
            {
                transaction.Start();
                result = Delete(request, removeConfig);
                transaction.Commit();
            }

            // Switch of warning suppression
            if (UIControlledApplication != null)
                UIControlledApplication.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;

            return result;
        }

        /***************************************************/
    }
}





