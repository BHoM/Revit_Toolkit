using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHoM;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Revit2017_Adapter.Base;
using Revit2017_Adapter.Geometry;
using BHE = BHoM.Structural.Elements;
using BHP = BHoM.Structural.Properties;
using System.Reflection;

namespace Engine.Convert
{
    public static partial class RevitElement
    {

        /**********************************************/
        /****  Generic                             ****/
        /**********************************************/

        /// <summary>
        /// BHomObject to Revit Element Convert swicher.
        /// </summary>
        public static object Write(BHoM.Base.BHoMObject BHobj, Document m_document, string lvl = null)
        {
            if (BHobj is BHoM.Structural.Elements.Bar)
            {
                return Write(BHobj as BHoM.Structural.Elements.Bar, m_document, lvl);
            }
            return null;
        }


        /**********************************************/
        /****  Bar                                 ****/
        /**********************************************/

        /// <summary>
        /// BHomBar to Revit Element.
        /// </summary>
        public static object Write(BHoM.Structural.Elements.Bar BHobj, Document m_document, string lvl = null)
        {
            // Load Family Symbol.
            FamilySymbol familySymbol = RevitUtils.GetFamilySymbol(m_document, new SectionPropertyName(BHobj.SectionProperty.Name));

            // Create familysymbol from section.
            if (familySymbol == null)
            {
                familySymbol = RevitUtils.CreateExtrusionFamilySymbol(BHobj,m_document);
            }

            // Insert Element in Revit Project.
            if (familySymbol != null)
            {
                // Get Level.
                Level level = null;
                if (lvl != null)
                {
                    level = (Level)RevitUtils.GetElement(m_document, typeof(Level), lvl);
                }
                else
                {
                    level = RevitUtils.GetLevel(m_document, BHobj.StartPoint.Z);
                }

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate(); m_document.Regenerate();
                }

                if (m_document.IsFamilyDocument)
                {
                    FamilyInstance beam = m_document.FamilyCreate.NewFamilyInstance(RevitGeometry.Write(BHobj.Line), familySymbol, (View)RevitUtils.GetElement(m_document, typeof(View), "Front"));
                    RevitUtils.DisableEndJoin(beam);
                    RevitUtils.SetElementParameter(beam, BuiltInParameter.Z_JUSTIFICATION, "1");
                    RevitUtils.SetElementParameter(beam, BuiltInParameter.STRUCTURAL_BEAM_ORIENTATION, RevitGeometry.RadToDeg(BHobj.OrientationAngle).ToString());
                    return beam.Id.IntegerValue;
                }
                else
                {
                    FamilyInstance beam = m_document.Create.NewFamilyInstance(RevitGeometry.Write(BHobj.Line), familySymbol, level, RevitUtils.StructuralType(BHobj.StructuralUsage));
                    RevitUtils.DisableEndJoin(beam);
                    RevitUtils.SetElementParameter(beam, BuiltInParameter.Z_JUSTIFICATION, "1");
                    RevitUtils.SetElementParameter(beam, BuiltInParameter.STRUCTURAL_BEAM_ORIENTATION, RevitGeometry.RadToDeg(BHobj.OrientationAngle).ToString());
                    return beam.Id.IntegerValue;
                }
            }
            return null;
        }
    }
}
