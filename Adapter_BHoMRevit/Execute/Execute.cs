using System.Collections.Generic;

namespace BH.UI.Revit.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** IAdapter Interface                        ****/
        /***************************************************/

        public override bool Execute(string command, Dictionary<string, object> parameters = null, Dictionary<string, object> config = null)
        {
            string commandUpper = command.ToUpper();

            if (commandUpper == "DONOTHING")
                return DoNothing();

            return false;
        }
        
        /***************************************************/

        public bool DoNothing()
        {
            return true;
        }

        /***************************************************/
    }
}