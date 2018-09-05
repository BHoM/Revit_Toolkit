using BH.oM.Adapters.Revit;

namespace BH.Adapter.Revit
{
    public abstract class InternalRevitAdapter : BHoMAdapter
    {

        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public RevitSettings RevitSettings { get; set; }

        /***************************************************/
    }
}
