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
            if (BHobj is BHoM.Structural.Elements.Panel)
            {
                return Write(BHobj as BHoM.Structural.Elements.Panel, m_document, lvl);
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
            if (BHobj.SectionProperty != null)
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
                    level = RevitUtils.GetLevel(m_document, BHobj.StartPoint.Z, 2);
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
            }
            return null;
        }

        /**********************************************/
        /****  Panel                               ****/
        /**********************************************/
        /// <summary>
        /// BHomPanel to Revit Element.
        /// </summary>
        public static object Write(BHoM.Structural.Elements.Panel BHobj, Document m_document, string lvl = null)
        {

            // Check if floor.
            if (RevitGeometry.IsHorizontal(BHobj.External_Contours))
            {
                Floor floor = m_document.Create.NewFloor(RevitGeometry.Write(BHobj.External_Contours).get_Item(0), true);
                if (BHobj.Internal_Contours != null)
                {
                    CurveArrArray crvarr = RevitGeometry.Write(BHobj.Internal_Contours);
                    for (int i = 0; i < crvarr.Size; i++)
                    {
                        m_document.Regenerate();
                        m_document.Create.NewOpening(floor, crvarr.get_Item(i), false);
                    }
                 }
                 return floor.Id.IntegerValue;
            }

            // Check if Vertical Wall.
            if (RevitGeometry.IsVertical(BHobj.External_Contours))
            {
                Wall wall = null;
                if (BHobj.External_Contours.Count == 4)
                {
                    if (RevitGeometry.CurveVerticalorHorizontal(BHobj.External_Contours))
                    {
                        List<double> ptZlist = new List<double>();
                        foreach (BHoM.Geometry.Curve crv in BHobj.External_Contours)
                        {
                            foreach (BHoM.Geometry.Point pt in crv.ControlPoints)
                            {
                                ptZlist.Add(pt.Z);
                            }
                        }
                        ptZlist.Sort();
                        double min = ptZlist.First();
                        double max = ptZlist.Last();

                        Curve botcrv = null;
                        foreach (BHoM.Geometry.Curve crv in BHobj.External_Contours)
                        {
                            if (crv.StartPoint.Z == min && crv.EndPoint.Z == min)
                            {
                                botcrv = RevitGeometry.Write(crv);
                                break;
                            }
                        }
                        Level lvlbot = RevitUtils.GetLevel(m_document, Convert.RevitGeometry.MeterToFeet(min), 2);
                        Level lvltop = RevitUtils.GetLevel(m_document, Convert.RevitGeometry.MeterToFeet(max), 2);


                        IList<Element> walltypes = RevitUtils.GetElement(m_document, typeof(WallType));
                        WallType walltype = (WallType)walltypes[0];
                        if (BHobj.PanelProperty != null)
                        {
                            foreach (WallType type in walltypes)
                            {
                                if (type.Width == BHobj.PanelProperty.Thickness)
                                {
                                    walltype = type;
                                    break;
                                }
                            }
                        }
                        if (botcrv != null)
                        {
                            wall = Wall.Create(m_document, botcrv, walltype.Id, lvlbot.Id, Convert.RevitGeometry.MeterToFeet(BHobj.External_Contours.Bounds().Extents.Z), Convert.RevitGeometry.MeterToFeet(min) - lvlbot.ProjectElevation, false, true);
                            RevitUtils.SetElementParameter(wall, "Top Constraint", lvltop.Id);
                            RevitUtils.SetElementParameter(wall, BuiltInParameter.WALL_TOP_OFFSET, (Convert.RevitGeometry.MeterToFeet(max) - lvltop.ProjectElevation).ToString());

                        }
                    }
                }
                if (wall != null)
                {
                    if (BHobj.Internal_Contours != null)
                    {
                        CurveArrArray crvarr = RevitGeometry.Write(BHobj.Internal_Contours);
                        for (int i = 0; i < crvarr.Size; i++)
                        {
                            m_document.Regenerate();
                            m_document.Create.NewOpening(wall, crvarr.get_Item(i), false);
                        }
                    }
                    return wall.Id.IntegerValue;
                }
            }
            return null;
        }
    }
}
