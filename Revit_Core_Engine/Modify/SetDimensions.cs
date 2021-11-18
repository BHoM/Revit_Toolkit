using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using BH.oM.Spatial.ShapeProfiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        //[Description("Updates the type of the Revit FamilyInstance based on the given BHoM IFramingElement.")]
        //[Input("element", "Revit FamilyInstance to have its type updated.")]
        //[Input("bHoMObject", "BHoM IFramingElement, based on which the Revit element type will be updated.")]
        //[Input("settings", "Revit adapter settings to be used while performing the action.")]
        //[Output("success", "True if the type of the FamilyInstance has been successfully updated.")]
        public static bool SetDimensions(this FamilyInstance familyInstance, BH.oM.Architecture.BuildersWork.Opening opening, RevitSettings settings)
        {
            bool success = false;
            settings = settings.DefaultIfNull();

            // Set dimensions based on parameter mapping.
            HashSet<string> parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), nameof(oM.Architecture.BuildersWork.Opening.Depth));
            if (parameterNames == null || !familyInstance.SetParameters(parameterNames, opening.Depth))
                familyInstance.DimensionNotSetWarning("depth");
            else
                success = true;

            if (opening.Profile is RectangleProfile)
            {
                parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(RectangleProfile.Width)}");
                if (parameterNames == null || !familyInstance.SetParameters(parameterNames, ((RectangleProfile)opening.Profile).Width))
                    familyInstance.DimensionNotSetWarning("width");
                else
                    success = true;

                parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(RectangleProfile.Height)}");
                if (parameterNames == null || !familyInstance.SetParameters(parameterNames, ((RectangleProfile)opening.Profile).Height))
                    familyInstance.DimensionNotSetWarning("height");
                else
                    success = true;
            }
            else if (opening.Profile is CircleProfile)
            {
                parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(CircleProfile.Diameter)}");
                if (parameterNames == null || !familyInstance.SetParameters(parameterNames, ((CircleProfile)opening.Profile).Diameter))
                    familyInstance.DimensionNotSetWarning("diameter");
                else
                    success = true;
            }

            return success;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void DimensionNotSetWarning(this Element element, string dimension)
        {
            BH.Engine.Reflection.Compute.RecordWarning($"The family instance has been created, but its {dimension} could not be set. ElementId: {element.Id}");
        }

        /***************************************************/
    }
}
