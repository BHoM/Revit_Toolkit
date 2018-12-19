using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BH.UI.Revit.Forms;

namespace BH.UI.Revit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UpdatePorts : IExternalCommand
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            new UpdatePortsForm().ShowDialog();

            return Result.Succeeded;
        }

        /***************************************************/
    }
}