
using BH.oM.Base;

namespace BH.oM.Revit
{
    public class RevitSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public int PushPort { get; set; } = 14128;
        public int PullPort { get; set; } = 14129;
        public int MaxMinutesToWait { get; set; } = 10;
        public WorksetSettings WorksetSettings = new WorksetSettings();
        public SelectionSettings SelectionSettings = new SelectionSettings();
        public FamilyLibrary FamilyLibrary = new FamilyLibrary();
        public Discipline DefaultDiscipline = Discipline.Environmental;

        /***************************************************/
    }
}
