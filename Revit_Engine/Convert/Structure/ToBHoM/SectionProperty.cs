using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.oM.Structural.Properties;
using System;
using System.Collections.Generic;
using BHS = BH.Engine.Structure;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static ISectionProperty ToBHoMSectionProperty(this FamilyInstance familyInstance, bool copyCustomData = true, bool convertUnits = true)
        {
            try
            {
                string materialGrade = familyInstance.MaterialGrade();

                oM.Common.Materials.Material aMaterial = familyInstance.StructuralMaterialType.ToBHoMMaterial(materialGrade) as oM.Common.Materials.Material;
                IProfile aSectionDimensions = null;

                string name = familyInstance.Symbol.Name;
                aSectionDimensions = BH.Engine.Library.Query.Match("SectionProfiles", name) as IProfile;

                if (aSectionDimensions == null)
                {
                    aSectionDimensions = familyInstance.Symbol.ToBHoMProfile();
                }

                if (aSectionDimensions != null)
                {
                    //TODO: shouldn't we have AluminiumSection and TimberSection at least?
                    if (aMaterial == null)
                    {
                        familyInstance.UnknownMaterialWarning();
                        return BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
                    }
                    else if (aMaterial.Type == oM.Common.Materials.MaterialType.Concrete)
                    {
                        return BHS.Create.ConcreteSectionFromProfile(aSectionDimensions, aMaterial, name);
                    }
                    else if (aMaterial.Type == oM.Common.Materials.MaterialType.Steel)
                    {
                        return BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
                    }
                    else
                    {
                        familyInstance.UnknownMaterialWarning();
                        return BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
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
                                            foreach (Autodesk.Revit.DB.Edge c in curveArray)
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
                        return BHS.Create.SteelFreeFormSection(profileCurves, aMaterial, name);
                    }
                    else if (aMaterial.Type == oM.Common.Materials.MaterialType.Concrete)
                    {
                        return BHS.Create.ConcreteFreeFormSection(profileCurves, aMaterial, name);
                    }
                    else if (aMaterial.Type == oM.Common.Materials.MaterialType.Steel)
                    {
                        return BHS.Create.SteelFreeFormSection(profileCurves, aMaterial, name);
                    }
                    else
                    {
                        familyInstance.UnknownMaterialWarning();
                        return BHS.Create.SteelFreeFormSection(profileCurves, aMaterial, name);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /***************************************************/
    }
}
