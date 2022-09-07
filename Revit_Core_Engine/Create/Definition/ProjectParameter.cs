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

using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a Revit project parameter Definition based on the given properties.")]
        [Input("document", "Revit document, in which the new parameter Definition will be created.")]
        [Input("parameterName", "Name of the created parameter.")]
        [Input("parameterType", "Type of the created parameter. One of the UI dropdown values, e.g. Length, Area, Currency etc.")]
        [Input("parameterGroup", "Parameter group, to which the created parameter belongs. One of the UI dropdown values for project parameters (e.g. Dimensions).")]
        [Input("instance", "If true, the created parameter will be an instance parameter, otherwise it will be a type parameter.")]
        [Input("categories", "Categories, to which the created parameter is bound. It will get bound to all categories if this value is null.")]
        [Output("definition", "Revit project parameter Definition created based on the input properties.")]
#if (REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021 || REVIT2022)
        public static Definition ProjectParameter(Document document, string parameterName, ParameterType parameterType, BuiltInParameterGroup parameterGroup, bool instance, IEnumerable<Category> categories)
#else
        public static Definition ProjectParameter(Document document, string parameterName, ForgeTypeId parameterType, BuiltInParameterGroup parameterGroup, bool instance, IEnumerable<Category> categories)
#endif
        {
            // Inspired by https://github.com/DynamoDS/DynamoRevit/blob/master/src/Libraries/RevitNodes/Elements/Parameter.cs

            if (categories != null && !categories.Any())
            {
                BH.Engine.Base.Compute.RecordError($"Parameter {parameterName} of type {parameterType.Label()} could not be created because no category bindings were provided.");
                return null;
            }

            // Look for existing parameter definition
            bool bindings = false;
            IEnumerable<Definition> existingDefs = new FilteredElementCollector(document).OfClass(typeof(SharedParameterElement)).Cast<SharedParameterElement>().Select(x => x.GetDefinition());
            Definition def = existingDefs.FirstOrDefault(x => x.Name == parameterName && x.ParameterGroup == parameterGroup);
            if (def != null)
            {
                if (document.ParameterBindings.Contains(def))
                {
                    BH.Engine.Base.Compute.RecordWarning($"Parameter {parameterName} already exists in group {LabelUtils.GetLabelFor(parameterGroup)}. It already has category bindings, they were not updated - please make sure they are correct.");
                    bindings = true;
                }
                else
                    BH.Engine.Base.Compute.RecordWarning($"Parameter {parameterName} already exists in group {LabelUtils.GetLabelFor(parameterGroup)}. It did not have any category bindings, so input bindings were applied.");
            }
            else
            {
                // buffer the current shared parameter file name and apply a new empty parameter file instead
                string sharedParameterFile = document.Application.SharedParametersFilename;
                string tempSharedParameterFile = System.IO.Path.GetTempFileName() + ".txt";
                using (System.IO.File.Create(tempSharedParameterFile)) { }
                document.Application.SharedParametersFilename = tempSharedParameterFile;
                
                // Create a new shared parameter, since the file is empty everything has to be created from scratch
                def = document.Application.OpenSharedParameterFile()
                    .Groups.Create("TempParameterGroup").Definitions.Create(new ExternalDefinitionCreationOptions(parameterName, parameterType)) as ExternalDefinition;

                // apply old shared parameter file
                document.Application.SharedParametersFilename = sharedParameterFile;

                // delete temp shared parameter file
                System.IO.File.Delete(tempSharedParameterFile);
            }

            if (!bindings)
            {
                // Apply instance or type binding
                CategorySet paramCategories = (categories == null) ? document.CategoriesWithBoundParameters() : Create.CategorySet(document, categories);

                Binding bin = (instance) ?
                    (Binding)document.Application.Create.NewInstanceBinding(paramCategories) :
                    (Binding)document.Application.Create.NewTypeBinding(paramCategories);

                document.ParameterBindings.Insert(def, bin, parameterGroup);
            }

            return def;
        }

        /***************************************************/
    }
}



