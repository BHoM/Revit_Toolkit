
using BH.oM.Base;

namespace BH.oM.Adapters.Revit.Settings
{
    public class RevitSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public int PushPort { get; set; } = 14128;
        public int PullPort { get; set; } = 14129;
        public int MaxMinutesToWait { get; set; } = 10;
        public Generic.FamilyLibrary FamilyLibrary = new Generic.FamilyLibrary();
        public Enums.Discipline DefaultDiscipline = Enums.Discipline.Environmental;
        public bool Replace = true;

        /***************************************************/
    }
}
