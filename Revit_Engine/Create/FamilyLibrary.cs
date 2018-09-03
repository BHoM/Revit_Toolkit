using System.Collections.Generic;
using System.IO;

using BH.oM.Revit;


namespace BH.Engine.Revit
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
