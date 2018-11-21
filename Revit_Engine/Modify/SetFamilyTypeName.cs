using System.Collections.Generic;
using BH.oM.Adapters.Revit;
using System.IO;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Properties;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Family name for Builiding Element Properties")]
        [Input("buildingElementProperties", "Building Element Properties")]
        [Input("familyName", "Revit Family Name")]
        [Output("BuildingElementProperties")]
        public static BuildingElementProperties SetFamilyName(this BuildingElementProperties buildingElementProperties, string familyName)
        {
            if (buildingElementProperties == null)
                return null;

            BuildingElementProperties aBuildingElementProperties = buildingElementProperties.GetShallowClone() as BuildingElementProperties;

            if (aBuildingElementProperties.CustomData.ContainsKey(Convert.FamilyName))
                aBuildingElementProperties.CustomData[Convert.FamilyName] = familyName;
            else
                aBuildingElementProperties.CustomData.Add(Convert.FamilyName, familyName);

            return aBuildingElementProperties;
        }

        /***************************************************/
    }
}
