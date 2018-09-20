using BH.oM.Adapters.Revit.Generic;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static FamilyLibrary FamilyLibrary(string directory, bool topDirectoryOnly = false)
        {
            return new FamilyLibrary().Append(directory, topDirectoryOnly);
        }

        /***************************************************/
    }
}
