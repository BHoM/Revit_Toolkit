using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Family Library Class which holds information about families can be loaded to model.")]
        [Input("directory", "Directory from where famlies will be loaded if not exists in model")]
        [Input("topDirectoryOnly", "Search through top dilectory folder and skip subfolders")]
        [Output("FamilyLibrary")]
        public static FamilyLibrary FamilyLibrary(string directory, bool topDirectoryOnly = false)
        {
            return new FamilyLibrary().Append(directory, topDirectoryOnly);
        }

        /***************************************************/
    }
}
