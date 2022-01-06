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
using BH.oM.Reflection.Attributes;
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

        [Description("Creates a Revit shared parameter Definition based on the given properties.")]
        [Input("document", "Revit document, in which the new parameter Definition will be created.")]
        [Input("parameterName", "Name of the created parameter.")]
        [Input("parameterType", "Type of the created parameter. One of the UI dropdown values, e.g. Length, Area, Currency etc.")]
        [Input("parameterGroup", "Name of the parameter group, to which the created parameter belongs.")]
        [Input("instance", "If true, the created parameter will be an instance parameter, otherwise it will be a type parameter.")]
        [Input("categories", "Categories, to which the created parameter is bound. It will get bound to all categories if this value is null.")]
        [Output("definition", "Revit shared parameter Definition created based on the input properties.")]
        public static Definition SharedParameter(Document document, string parameterName, ParameterType parameterType, string parameterGroup, bool instance, IEnumerable<Category> categories)
        {
            // Inspired by https://github.com/DynamoDS/DynamoRevit/blob/master/src/Libraries/RevitNodes/Elements/Parameter.cs

            if (categories != null && !categories.Any())
            {
                BH.Engine.Reflection.Compute.RecordError($"Parameter {parameterName} of type {LabelUtils.GetLabelFor(parameterType)} could not be created because no category bindings were provided.");
                return null;
            }

            // get current shared parameter file
            string sharedParameterFile = document.Application.SharedParametersFilename;

            // if the file does not exist, throw an error
            if (string.IsNullOrWhiteSpace(sharedParameterFile) || !System.IO.File.Exists(sharedParameterFile))
            {
                BH.Engine.Reflection.Compute.RecordError("The shared parameters file specified in the document does not exist.");
                return null;
            }

            // Create new parameter group if it does not exist yet
            DefinitionGroup groupDef = document.Application.OpenSharedParameterFile().Groups.get_Item(parameterGroup);

            if (groupDef == null)
            {
                try
                {
                    groupDef = document.Application.OpenSharedParameterFile().Groups.Create(parameterGroup);
                }
                catch
                {
                    BH.Engine.Reflection.Compute.RecordError("New group could not be created in the active document's shared parameter file. Please try using an existing group or unlocking the shared parameter file.");
                    return null;
                }
            }

            // If the parameter definition does exist return it
            bool bindings = false;
            Definition def = groupDef.Definitions.get_Item(parameterName);
            if (def != null)
            {
                if (document.ParameterBindings.Contains(def))
                {
                    BH.Engine.Reflection.Compute.RecordWarning($"Parameter {parameterName} already exists in group {parameterGroup}. It already has category bindings, they were not updated - please make sure they are correct.");
                    bindings = true;
                }
                else
                    BH.Engine.Reflection.Compute.RecordWarning($"Parameter {parameterName} already exists in group {parameterGroup}. It did not have any category bindings, so input bindings were applied.");
            }
            else
                def = groupDef.Definitions.Create(new ExternalDefinitionCreationOptions(parameterName, parameterType)) as ExternalDefinition;

            if (!bindings)
            {
                // Apply instance or type binding
                CategorySet paramCategories = (categories == null) ? document.CategoriesWithBoundParameters() : Create.CategorySet(document, categories);

                Binding bin = (instance) ?
                    (Binding)document.Application.Create.NewInstanceBinding(paramCategories) :
                    (Binding)document.Application.Create.NewTypeBinding(paramCategories);

                document.ParameterBindings.Insert(def, bin);
            }

            return def;
        }
        
        /***************************************************/
    }
}
