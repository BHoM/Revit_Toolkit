using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Properties;
using BHS = BH.Engine.Structure;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static ISectionProperty ToBHoMSectionProperty(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            ISectionProperty aSectionProperty = pullSettings.FindRefObject<ISectionProperty>(familyInstance.Id.IntegerValue);
            if (aSectionProperty != null)
                return aSectionProperty;

            oM.Common.Materials.Material aMaterial = pullSettings.FindRefObject<oM.Common.Materials.Material>(familyInstance.StructuralMaterialId.IntegerValue);

            if (aMaterial == null)
            {
                ElementId materialId = familyInstance.StructuralMaterialId;

                //TODO: can materialId == -1 && StructuralMaterialType and MaterialGrade be assigned?
                if (materialId.IntegerValue == -1)
                {
                    string materialGrade = familyInstance.MaterialGrade();
                    aMaterial = Query.Material( familyInstance.StructuralMaterialType, materialGrade);
                }

                if(aMaterial == null)
                    aMaterial = (familyInstance.Document.GetElement(materialId) as Material).ToBHoMMaterial(pullSettings);
            }

            string symbolName = familyInstance.Symbol.Name;
            aSectionProperty = BH.Engine.Library.Query.Match("SectionProperties", symbolName) as ISectionProperty;

            if (aSectionProperty != null)
            {
                aSectionProperty = aSectionProperty.GetShallowClone() as ISectionProperty;
                aSectionProperty.Material = aMaterial;
                aSectionProperty.Name = symbolName;
            }
            else
            {
                IProfile aSectionDimensions = familyInstance.Symbol.ToBHoMProfile(pullSettings);

                if (aSectionDimensions.Edges.Count == 0)
                {
                    List<oM.Geometry.ICurve> profileCurves = new List<oM.Geometry.ICurve>();
                    if (familyInstance.HasSweptProfile())
                    {
                        profileCurves = familyInstance.GetSweptProfile().GetSweptProfile().Curves.ToBHoM(pullSettings);
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
                                                profileCurves.Add(c.AsCurve().ToBHoM(pullSettings));
                                            }
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }

                    aSectionDimensions = new FreeFormProfile(profileCurves);

                    aSectionDimensions = Modify.SetIdentifiers(aSectionDimensions, familyInstance.Symbol) as IProfile;
                    if (pullSettings.CopyCustomData)
                        aSectionDimensions = Modify.SetCustomData(aSectionDimensions, familyInstance.Symbol, pullSettings.ConvertUnits) as IProfile;

                    pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSectionDimensions);
                }

                string name = familyInstance.Name;
                bool emptyProfile = aSectionDimensions.Edges.Count == 0;
                if (emptyProfile) familyInstance.Symbol.ConvertProfileFailedWarning();

                //TODO: shouldn't we have AluminiumSection and TimberSection at least?
                if (aMaterial == null)
                {
                    familyInstance.UnknownMaterialWarning();
                    if (emptyProfile)
                    {
                        aSectionProperty = new SteelSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        aSectionProperty.Name = name;
                    }
                    else aSectionProperty = BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
                }
                else if (aMaterial.Type == oM.Common.Materials.MaterialType.Concrete)
                {
                    if (emptyProfile)
                    {
                        aSectionProperty = new ConcreteSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        aSectionProperty.Name = name;
                    }
                    else aSectionProperty = BHS.Create.ConcreteSectionFromProfile(aSectionDimensions, aMaterial, name);
                }
                else if (aMaterial.Type == oM.Common.Materials.MaterialType.Steel)
                {
                    if (emptyProfile)
                    {
                        aSectionProperty = new SteelSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        aSectionProperty.Name = name;
                    }
                    else aSectionProperty = BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
                }
                else
                {
                    familyInstance.UnknownMaterialWarning();
                    if (emptyProfile)
                    {
                        aSectionProperty = new SteelSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        aSectionProperty.Name = name;
                    }
                    else aSectionProperty = BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
                }
            }

            aSectionProperty = Modify.SetIdentifiers(aSectionProperty, familyInstance) as ISectionProperty;
            if (pullSettings.CopyCustomData)
                aSectionProperty = Modify.SetCustomData(aSectionProperty, familyInstance, pullSettings.ConvertUnits) as ISectionProperty;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSectionProperty);

            return aSectionProperty;
        }

        /***************************************************/
    }
}