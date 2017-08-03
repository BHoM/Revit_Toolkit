using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHoM;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Revit2017_Adapter.Base;
using Engine.Convert;
using BHE = BHoM.Structural.Elements;
using BHP = BHoM.Structural.Properties;
using System.Reflection;

namespace SportVenueEvent_Cobra.Engine.Convert
{
    public static partial class RevitElement
    {

        /**********************************************/
        /****  Generic                             ****/
        /**********************************************/


        public static object Write(BHoM.Base.BHoMObject BHobj, string lvl, Document m_document)
        {
            if (BHobj is BHoM.Structural.Elements.PrecastSeatingPlank)
            {
                return Write(BHobj as BHoM.Structural.Elements.PrecastSeatingPlank, lvl, m_document);
            }
            else if (BHobj is BHoM.Structural.Elements.ConcreteRakerBeam)
            {
                return Write(BHobj as BHoM.Structural.Elements.ConcreteRakerBeam, lvl, m_document);
            }
            return null;
        }


        /**********************************************/
        /****  PrecastSeatingPlank                 ****/
        /**********************************************/

        public static object Write(BHoM.Structural.Elements.PrecastSeatingPlank BHobj, string lvl, Document m_document)
        {

            BHP.SteelSection secProp = (BHP.SteelSection)BHobj.Bar.SectionProperty;
            string typeName = secProp.Name;

            // Get familysymbol from document

            FamilySymbol familySymbol = familySymbol = (FamilySymbol)RevitUtils.GetElement(m_document, typeof(FamilySymbol), typeName);

            // Get familysymbol from path

            string FamilyName;
            if (typeName.Contains("PL"))
            {
                FamilyName = "Terrace_Precast_L";
            }
            else
            {
                FamilyName = "Terrace_Precast_U";
            }

            if (familySymbol == null)
            {
                familySymbol = RevitUtils.GetFamilySymbolfromPath(typeName, FamilyName, m_document);
            }

            if (familySymbol == null)
            {
                FamilySymbol familyParentType = familySymbol = (FamilySymbol)RevitUtils.GetElement(m_document, typeof(FamilySymbol), typeName);
                if (familySymbol != null)
                {
                    // Create new familytype by duplicating Parent type;
                    ElementType newFamilyType = familyParentType.Duplicate(typeName);
                    if (typeName.Contains("PL"))
                    {
                        string[] SplitName = typeName.Split("x".ToCharArray());
                        newFamilyType.GetParameters("RiserHeight")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[1])));
                        newFamilyType.GetParameters("RiserThickness")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[2])));
                        newFamilyType.GetParameters("BeamDepth")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[3])));
                        newFamilyType.GetParameters("GoingDepth")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[4])));
                        newFamilyType.GetParameters("GoingThickness")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[5])));
                        newFamilyType.GetParameters("Chamfer")[0].Set(RevitGeometry.MeterToFeet(BHobj.Chamfer));
                        newFamilyType.GetParameters("RakerCutout")[0].Set(RevitGeometry.MeterToFeet(BHobj.RakerCutout));
                    }
                    else
                    {
                        string[] SplitName = typeName.Split("x".ToCharArray());
                        newFamilyType.GetParameters("RiserHeight")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[1])));
                        newFamilyType.GetParameters("RiserThickness")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[2])));
                        newFamilyType.GetParameters("GoingDepth")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[3])));
                        newFamilyType.GetParameters("GoingThickness")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[4])));
                        newFamilyType.GetParameters("FrontThickness")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[5])));
                        newFamilyType.GetParameters("FrontHeight")[0].Set(RevitGeometry.MmToFeet(double.Parse(SplitName.ToList()[6])));
                        newFamilyType.GetParameters("Chamfer")[0].Set(RevitGeometry.MeterToFeet(BHobj.Chamfer));
                    }
                    familySymbol = familySymbol = (FamilySymbol)RevitUtils.GetElement(m_document, typeof(FamilySymbol), typeName);
                }
            }

            if (!familySymbol.IsActive)
            { familySymbol.Activate(); m_document.Regenerate(); }

            FamilyInstance beam = m_document.Create.NewFamilyInstance(RevitGeometry.Write(BHobj.Bar.Line), familySymbol, (Level)RevitUtils.GetElement(m_document, typeof(Level), lvl), Autodesk.Revit.DB.Structure.StructuralType.Beam);
            RevitUtils.DisableEndJoin(beam);
            RevitUtils.SetElementParameter(beam, BuiltInParameter.Z_JUSTIFICATION, "2");
            RevitUtils.SetElementParameter(beam, "EndAngle", RevitGeometry.RadToDeg(BHobj.StartAngle).ToString());
            RevitUtils.SetElementParameter(beam, "StartAngle", RevitGeometry.RadToDeg(BHobj.StartAngle).ToString());
            return beam.Id;
        }

        /**********************************************/
        /****  ConcreteRakerBeam                   ****/
        /**********************************************/

        public static object Write(BHoM.Structural.Elements.ConcreteRakerBeam BHobj, string lvl, Document m_document)
        {

            string typeName = BHobj.Name;

            FamilySymbol familySymbol = familySymbol = (FamilySymbol)RevitUtils.GetElement(m_document, typeof(FamilySymbol), typeName);

            // Get familysymbol from path.

            if (familySymbol == null)
            {
                familySymbol = RevitUtils.GetFamilySymbolfromPath(typeName, typeName, m_document);
            }

            // Create familysymbol from section

            if (familySymbol == null)
            {
                familySymbol = SportVenueEvent_Cobra.Utils.CreateExtrusionFamilySymbol(BHobj, m_document);
            }

            if (familySymbol != null)
            {
                if (!familySymbol.IsActive)
                { familySymbol.Activate(); m_document.Regenerate(); }

                RevitUtils.SetElementParameter(familySymbol, "Type Mark", BHobj.Name.ToString());

                XYZ locpt = RevitGeometry.Write(BHobj.Location);
                XYZ oript = RevitGeometry.Write(BHobj.Orientation,false);
                Level level = (Level)RevitUtils.GetElement(m_document, typeof(Level), lvl);
                
                FamilyInstance inst = m_document.Create.NewFamilyInstance(new XYZ(locpt.X,locpt.Y,locpt.Z - level.Elevation), familySymbol,  level,  Autodesk.Revit.DB.Structure.StructuralType.Beam);
                ElementTransformUtils.RotateElement(m_document, inst.Id, Line.CreateBound(locpt, new XYZ(locpt.X, locpt.Y, locpt.Z + 10)), oript.AngleOnPlaneTo(XYZ.BasisX, - XYZ.BasisZ));
                //Transform trans = inst.GetTransform();
                //trans = Transform.CreateRotation(XYZ.BasisZ, oript.AngleOnPlaneTo(XYZ.BasisX,XYZ.BasisZ));

                if (BHobj.Bars != null)
                {
                    RevitUtils.SetElementParameter(inst, "BeamLength", BHobj.Bars[0].Length.ToString());
                }
                RevitUtils.SetElementParameter(inst, BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM, level.Id);

                Material mat = RevitUtils.GetMaterial(m_document, "Precast Concrete");
                if (mat != null)
                {
                    Parameter param = inst.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                    if (param != null)
                    {
                        RevitUtils.SetElementParameter(inst, BuiltInParameter.STRUCTURAL_MATERIAL_PARAM, mat.Id);
                    }
                }
                return inst.Id.IntegerValue;
                
            }
            return null;
        }
    }
}
