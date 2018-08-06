using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.oM.Structural.Properties;
using System.Collections.Generic;
using BHS = BH.Engine.Structure;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static ISectionProperty ToBHoMSectionProperty(this FamilyInstance familyInstance, bool copyCustomData = true, bool convertUnits = true)
        {
            ISectionProperty aSectionProperty = null;

            string materialGrade = familyInstance.MaterialGrade();
            oM.Common.Materials.Material aMaterial = familyInstance.StructuralMaterialType.ToBHoMMaterial(materialGrade) as oM.Common.Materials.Material;

            string name = familyInstance.Symbol.Name;
            IProfile aSectionDimensions = Library.Query.Match("SectionProfiles", name) as IProfile;
            
            if (aSectionDimensions == null) aSectionDimensions = familyInstance.Symbol.ToBHoMProfile(copyCustomData, convertUnits);
            
            if (aSectionDimensions.Edges.Count != 0)
            {
                //TODO: shouldn't we have AluminiumSection and TimberSection at least?
                if (aMaterial == null)
                {
                    familyInstance.UnknownMaterialWarning();
                    aSectionProperty = BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
                }
                else if (aMaterial.Type == oM.Common.Materials.MaterialType.Concrete)
                {
                    aSectionProperty = BHS.Create.ConcreteSectionFromProfile(aSectionDimensions, aMaterial, name);
                }
                else if (aMaterial.Type == oM.Common.Materials.MaterialType.Steel)
                {
                    aSectionProperty = BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
                }
                else
                {
                    familyInstance.UnknownMaterialWarning();
                    aSectionProperty = BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
                }
            }

            else
            {
                List<oM.Geometry.ICurve> profileCurves = new List<oM.Geometry.ICurve>();
                if (familyInstance.HasSweptProfile())
                {
                    profileCurves = familyInstance.GetSweptProfile().GetSweptProfile().Curves.ToBHoM();
                }
                else
                {
                    foreach (GeometryObject obj in familyInstance.Symbol.get_Geometry(new Options()))
                    {
                        if (obj is Solid)
                        {
                            XYZ direction = familyInstance.StructuralType == StructuralType.Column ? new XYZ(0, 0, 1) : new XYZ(1, 0, 0);
                            foreach (Face face in (obj as Solid).Faces)
                            {
                                if (face is PlanarFace && (face as PlanarFace).FaceNormal.Normalize().IsAlmostEqualTo(direction, 0.001) || (face as PlanarFace).FaceNormal.Normalize().IsAlmostEqualTo(-direction, 0.001))
                                {
                                    foreach (EdgeArray curveArray in (face as PlanarFace).EdgeLoops)
                                    {
                                        foreach (Edge c in curveArray)
                                        {
                                            profileCurves.Add(c.AsCurve().ToBHoM());
                                        }
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }

                //TODO: shouldn't we have AluminiumSection and TimberSection at least?
                if (aMaterial == null)
                {
                    familyInstance.UnknownMaterialWarning();
                    if (profileCurves.Count == 0)
                    {
                        familyInstance.Symbol.ConvertProfileFailedWarning();
                        aSectionProperty = new SteelSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    }
                    else aSectionProperty = BHS.Create.SteelFreeFormSection(profileCurves, aMaterial, name);
                }
                else if (aMaterial.Type == oM.Common.Materials.MaterialType.Concrete)
                {
                    if (profileCurves.Count == 0)
                    {
                        familyInstance.Symbol.ConvertProfileFailedWarning();
                        aSectionProperty = new ConcreteSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    }
                    else aSectionProperty = BHS.Create.ConcreteFreeFormSection(profileCurves, aMaterial, name);
                }
                else if (aMaterial.Type == oM.Common.Materials.MaterialType.Steel)
                {
                    if (profileCurves.Count == 0)
                    {
                        familyInstance.Symbol.ConvertProfileFailedWarning();
                        aSectionProperty = new SteelSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    }
                    else aSectionProperty = BHS.Create.SteelFreeFormSection(profileCurves, aMaterial, name);
                }
                else
                {
                    familyInstance.UnknownMaterialWarning();
                    if (profileCurves.Count == 0)
                    {
                        familyInstance.Symbol.ConvertProfileFailedWarning();
                        aSectionProperty = new SteelSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    }
                    else aSectionProperty = BHS.Create.SteelFreeFormSection(profileCurves, aMaterial, name);
                }
            }

            aSectionProperty = Modify.SetIdentifiers(aSectionProperty, familyInstance) as ISectionProperty;
            if (copyCustomData)
                aSectionProperty = Modify.SetCustomData(aSectionProperty, familyInstance, convertUnits) as ISectionProperty;

            return aSectionProperty;
        }

        /***************************************************/
    }
}
