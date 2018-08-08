using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.oM.Structural.Properties;
using System.Collections.Generic;
using BHS = BH.Engine.Structure;
using BH.oM.Base;
using System.Linq;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static ISectionProperty ToBHoMSectionProperty(this FamilyInstance familyInstance, Dictionary<ElementId, List<IBHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            ISectionProperty aSectionProperty = null;
            
            oM.Common.Materials.Material aMaterial = null;
            bool materialFound = false;
            if (objects != null)
            {
                List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                if (objects.TryGetValue(familyInstance.StructuralMaterialId, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                    {
                        aMaterial = aBHoMObjectList.First() as oM.Common.Materials.Material;
                        materialFound = true;
                    }
            }

            if (!materialFound)
            {
                ElementId materialId = familyInstance.StructuralMaterialId;

                //TODO: can materialId == -1 && StructuralMaterialType and MaterialGrade be assigned?
                if (materialId.IntegerValue == -1)
                {
                    string materialGrade = familyInstance.MaterialGrade();
                    aMaterial = familyInstance.StructuralMaterialType.ToBHoMMaterial(materialGrade);
                }
                else
                {
                    aMaterial = (familyInstance.Document.GetElement(materialId) as Material).ToBHoMMaterial();
                    if (objects != null)
                        objects.Add(familyInstance.StructuralMaterialId, new List<IBHoMObject>(new IBHoMObject[] { aMaterial }));
                }
            }

            IProfile aSectionDimensions = null;
            if (objects != null)
            {
                List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                if (objects.TryGetValue(familyInstance.Symbol.Id, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aSectionDimensions = aBHoMObjectList.First() as IProfile;
            }

            if (aSectionDimensions == null)
            {
                string symbolName = familyInstance.Symbol.Name;
                aSectionDimensions = Library.Query.Match("SectionProfiles", symbolName) as IProfile;

                if (aSectionDimensions == null) aSectionDimensions = familyInstance.Symbol.ToBHoMProfile(false, convertUnits);

                if (aSectionDimensions.Edges.Count == 0)
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
                    aSectionDimensions = new FreeFormProfile(profileCurves);
                }

                aSectionDimensions = Modify.SetIdentifiers(aSectionDimensions, familyInstance.Symbol) as IProfile;
                if (copyCustomData)
                    aSectionDimensions = Modify.SetCustomData(aSectionDimensions, familyInstance.Symbol, convertUnits) as IProfile;

                if (objects != null)
                    objects.Add(familyInstance.Symbol.Id, new List<IBHoMObject>(new IBHoMObject[] { aSectionDimensions }));
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
            
            aSectionProperty = Modify.SetIdentifiers(aSectionProperty, familyInstance) as ISectionProperty;
            if (copyCustomData)
                aSectionProperty = Modify.SetCustomData(aSectionProperty, familyInstance, convertUnits) as ISectionProperty;

            return aSectionProperty;
        }

        /***************************************************/
    }
}
