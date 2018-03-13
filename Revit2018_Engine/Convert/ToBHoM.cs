using System.Collections.Generic;
using System.Linq;
using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using BH.oM.Environmental.Properties;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using BH.oM.Environmental.Elements;

using BH.Engine.Environment;
using BHS = BH.Engine.Structure;
using BH.oM.Base;
using Autodesk.Revit.DB.Structure.StructuralSections;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Convert Methods
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        /// <summary>
        /// Gets BHoM Point from Revit (XYZ) Point
        /// </summary>
        /// <param name="xyz">Revit Point (XYZ)</param>
        /// <returns name="Point">BHoM Point</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Point, Revit Point, XYZ 
        /// </search>
        public static oM.Geometry.Point ToBHoM(this XYZ xyz)
        {
            return Geometry.Create.Point(xyz.X, xyz.Y, xyz.Z);
        }

        /// <summary>
        /// Gets BHoM Point from Revit LocationPoint
        /// </summary>
        /// <param name="locationPoint">Revit LocationPoint</param>
        /// <returns name="Point">BHoM Point</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Point, Revit LocationPoint, LocationPoint 
        /// </search>
        public static oM.Geometry.Point ToBHoM(this LocationPoint locationPoint)
        {
            return ToBHoM(locationPoint.Point);
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit Curve
        /// </summary>
        /// <param name="curve">Revit Curve</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Curve, Revit Curve, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this Curve curve)
        {
            if (curve is Line)
                return Geometry.Create.Line(ToBHoM(curve.GetEndPoint(0)), ToBHoM(curve.GetEndPoint(1)));

            if (curve is Arc)
                return Geometry.Create.Arc(ToBHoM(curve.GetEndPoint(0)), ToBHoM(curve.Evaluate(0.5, true)), ToBHoM(curve.GetEndPoint(1)));

            if (curve is NurbSpline)
            {
                NurbSpline aNurbSpline = curve as NurbSpline;
                return Geometry.Create.NurbCurve(aNurbSpline.CtrlPoints.Cast<XYZ>().ToList().ConvertAll(x => ToBHoM(x)), aNurbSpline.Weights.Cast<double>(), aNurbSpline.Degree);
            }

            if(curve is Ellipse)
            {
                Ellipse aEllipse = curve as Ellipse;
                return Geometry.Create.Ellipse(ToBHoM(aEllipse.Center), aEllipse.RadiusX, aEllipse.RadiusY);
            }

            return null;
        }

        public static List<oM.Geometry.ICurve> ToBHoM(this List<Curve> curves)
        {
            return curves.Select(c => c.ToBHoM()).ToList();
        }

        public static List<oM.Geometry.ICurve> ToBHoM(this CurveArray curves)
        {
            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            for (int i = 0; i < curves.Size; i++)
            {
                result.Add(curves.get_Item(i).ToBHoM());
            }
            return result;
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit Edge
        /// </summary>
        /// <param name="edge">Revit Edge</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Curve, Revit Edge, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this Autodesk.Revit.DB.Edge edge)
        {
            return ToBHoM(edge.AsCurve());
        }

        /// <summary>
        /// Gets BHoM PolyCurve from Revit CurveLoop
        /// </summary>
        /// <param name="curveLoop">Revit CurveLoop</param>
        /// <returns name="PolyCurve">BHoM PolyCurve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM PolyCurve, Revit CurveLoop, PolyCurve, ICurve
        /// </search>
        public static oM.Geometry.PolyCurve ToBHoM(this CurveLoop curveLoop)
        {
            if (curveLoop == null)
                return null;

            List<oM.Geometry.ICurve> aICurveList = new List<oM.Geometry.ICurve>();
            foreach (Curve aCurve in curveLoop)
                aICurveList.Add(aCurve.ToBHoM());

            return Geometry.Create.PolyCurve(aICurveList);
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit LocationCurve
        /// </summary>
        /// <param name="locationCurve">Revit LocationCurve</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM ICurve, Revit LocationCurve, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this LocationCurve locationCurve)
        {
            return ToBHoM(locationCurve.Curve);
        }

        /// <summary>
        /// Gets BHoM BuildingElementPanels from Revit PlanarFace
        /// </summary>
        /// <param name="planarFace">Revit PlanarFace</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <returns name="BuildingElementPanel">BHoM BuildingElementPanels</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementPanel, Revit PlanarFace
        /// </search>
        public static List<BHoMObject> ToBHoM(this PlanarFace planarFace, Discipline discipline = Discipline.Environmental)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        EdgeArrayArray aEdgeArrayArray = planarFace.EdgeLoops;
                        if (aEdgeArrayArray != null && aEdgeArrayArray.Size > 0)
                        {
                            List<BHoMObject> aResult = new List<BHoMObject>();
                            for (int i = 0; i < aEdgeArrayArray.Size; i++)
                            {
                                EdgeArray aEdgeArray = aEdgeArrayArray.get_Item(i);
                                List<oM.Geometry.ICurve> aCurveList = new List<oM.Geometry.ICurve>();
                                foreach (Autodesk.Revit.DB.Edge aEdge in aEdgeArray)
                                {
                                    Curve aCurve = aEdge.AsCurve();
                                    if (aCurve != null)
                                        aCurveList.Add(aCurve.ToBHoM());
                                }

                                if (aCurveList != null && aCurveList.Count > 0)
                                {
                                    BuildingElementPanel aBuildingElementPanel = new BuildingElementPanel();
                                    aBuildingElementPanel = aBuildingElementPanel.SetGeometry(Geometry.Create.PolyCurve(aCurveList));
                                    aResult.Add(aBuildingElementPanel);
                                }
                            }
                            return aResult;
                        }
                        return null;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElementCurve from Revit Wall
        /// </summary>
        /// <param name="wall">Revit Wall</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <returns name="BuildingElementCurve">BHoM BuildingElementCurve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementCurve, Revit Wall, BuildingElementCurve
        /// </search>
        public static BuildingElementCurve ToBHoMBuildingElementCurve(this Wall wall, Discipline discipline = Discipline.Environmental)
        {
            LocationCurve aLocationCurve = wall.Location as LocationCurve;
            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve
            {
                Curve = ToBHoM(aLocationCurve)
            };
            return aBuildingElementCurve;
        }

        /// <summary>
        /// Gets BHoM BuildingElementPanels from Revit Floor
        /// </summary>
        /// <param name="floor">Revit Floor</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <returns name="BuildingElementPanels">BHoM BuildingElementPanels</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementPanels, Revit Wall, BuildingElementPanel, ToBHoMBuildingElementPanels
        /// </search>
        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this Floor floor)
        {
            return ToBHoMBuildingElementPanels(floor.get_Geometry(new Options()));
        }

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this RoofBase roofBase)
        {
            return ToBHoMBuildingElementPanels(roofBase.get_Geometry(new Options()));
        }

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this GeometryElement geometryElement)
        {
            List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                Solid aSolid = aGeometryObject as Solid;
                if (aSolid == null)
                    continue;

                PlanarFace aPlanarFace = Query.Top(aSolid);
                if (aPlanarFace == null)
                    continue;

                List<BHoMObject> aBHoMObjectList = aPlanarFace.ToBHoM(Discipline.Environmental);
                if (aBHoMObjectList == null || aBHoMObjectList.Count < 1)
                    continue;

                List<BuildingElementPanel> aBuildingElementPanelList = aBHoMObjectList.Cast<BuildingElementPanel>().ToList();
                if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                    aResult.AddRange(aBuildingElementPanelList);
            }

            return aResult;

        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM Building from Site Location
        /// </summary>
        /// <param name="siteLocation">Revit Site Location</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Site Location to CustomData of BHoMObjects</param>
        /// <returns name="Building">BHoM Building</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Building, Revit SiteLocation, Site Location
        /// </search>
        public static BHoMObject ToBHoM(this SiteLocation siteLocation, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            if (siteLocation == null)
                return null;

            Building aBuilding = new Building
            {
                Elevation = siteLocation.Elevation,
                Longitude = siteLocation.Longitude,
                Latitude = siteLocation.Latitude,
                Location = new oM.Geometry.Point()
            };

            aBuilding = Modify.SetIdentifiers(aBuilding, siteLocation) as Building;
            if (copyCustomData)
                aBuilding = Modify.SetCustomData(aBuilding, siteLocation) as Building;

            return aBuilding;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this FamilyInstance familyInstance, Discipline discipline = Discipline.Structural, bool copyCustomData = true)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    {
                        StructuralType structuralType = ((FamilyInstance)familyInstance).StructuralType;
                        if (structuralType == StructuralType.Beam || structuralType == StructuralType.Brace || structuralType == StructuralType.Column)
                        {
                            AnalyticalModel analyticalModel = familyInstance.GetAnalyticalModel();
                            if (analyticalModel == null) return null;

                            oM.Geometry.Line barCurve = Geometry.Modify.Scale(analyticalModel.GetCurve().ToBHoM() as oM.Geometry.Line, origin, feetToMetreVector);
                            ISectionProperty aSectionProperty = familyInstance.ToBHoMSection(barCurve, copyCustomData) as ISectionProperty;

                            double rotation;
                            if (familyInstance.Location is LocationPoint)
                            {
                                int multiplier = familyInstance.FacingOrientation.DotProduct(new XYZ(1, 0, 0)) < 0 ? 1 : -1;
                                rotation = familyInstance.FacingOrientation.AngleTo(new XYZ(0, 1, 0)) * multiplier;
                            }
                            else rotation = -familyInstance.LookupParameter("Cross-Section Rotation").AsDouble();
                            
                            Bar aBar = BHS.Create.Bar(barCurve, aSectionProperty);
                            aBar.OrientationAngle = rotation;
                            aBar.Offset = new Offset();

                            aBar = Modify.SetIdentifiers(aBar, familyInstance) as Bar;
                            if (copyCustomData)
                                aBar = Modify.SetCustomData(aBar, familyInstance) as Bar;

                            return aBar;
                        }
                        return null;
                    }
                case Discipline.Environmental:
                    {
                        //TODO: add code for Environmental FamilyInstances (Door, Window)
                        return null;
                    }
                case Discipline.Architecture:
                    {
                        //TODO: add code for Architectural FamilyInstances
                        return null;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElement from Revit Wall
        /// </summary>
        /// <param name="wall">Revit Wall</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit Wall
        /// </search>
        /// 
        //TODO: split this to multiple panels? if a panel is separate here, it will cause issues in laundry? how about pulling back to revit?
        public static BHoMObject ToBHoM(this Wall wall, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {

                        BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoM(discipline, copyCustomData) as BuildingElementProperties;

                        BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, ToBHoMBuildingElementCurve(wall), ToBHoM(wall.Document.GetElement(wall.LevelId) as Level, discipline, copyCustomData) as BH.oM.Architecture.Elements.Level);

                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
                        if (copyCustomData)
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, wall) as BuildingElement;

                        return aBuildingElement;
                    }

                case Discipline.Structural:
                    {
                        string materialGrade = wall.GetMaterialGrade();

                        Property2D aProperty2D = wall.WallType.ToBHoM(discipline, copyCustomData, materialGrade) as Property2D;
                        List<oM.Geometry.Polyline> outlines = wall.GetBHOutlines();
                        
                        PanelPlanar aPanelPlanar = BHS.Create.PanelPlanar(outlines)[0];       // this is a temporary cheat!
                        aPanelPlanar.Property = aProperty2D;

                        aPanelPlanar = Modify.SetIdentifiers(aPanelPlanar, wall) as PanelPlanar;
                        if (copyCustomData)
                            aPanelPlanar = Modify.SetCustomData(aPanelPlanar, wall) as PanelPlanar;

                        return aPanelPlanar;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElement from Revit Floor
        /// </summary>
        /// <param name="floor">Revit Floor</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit Floor
        /// </search>
        public static List<BHoMObject> ToBHoM(this Floor floor, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        // we need the same like this for walls?


                        List<BHoMObject> aResult = new List<BHoMObject>();
                        BuildingElementProperties aBuildingElementProperties = floor.FloorType.ToBHoM(discipline, copyCustomData) as BuildingElementProperties;
                        foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(floor))
                        {
                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(floor.Document.GetElement(floor.LevelId) as Level, discipline) as BH.oM.Architecture.Elements.Level);

                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, floor) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, floor) as BuildingElement;

                            aResult.Add(aBuildingElement);
                        }
                        return aResult;
                    }
                case Discipline.Structural:
                    {
                        string materialGrade = floor.GetMaterialGrade();

                        Property2D aProperty2D = floor.FloorType.ToBHoM(discipline, copyCustomData, materialGrade) as Property2D;
                        List<oM.Geometry.Polyline> outlines = floor.GetBHOutlines();

                        List<BHoMObject> aResult = new List<BHoMObject>();
                        List<PanelPlanar> aPanelsPlanar = BHS.Create.PanelPlanar(outlines);
                        for (int i = 0; i < aPanelsPlanar.Count; i++)
                        {
                            PanelPlanar pp = aPanelsPlanar[i];
                            pp.Property = aProperty2D;
                            pp = Modify.SetIdentifiers(pp, floor) as PanelPlanar;

                            if (copyCustomData)
                                pp = Modify.SetCustomData(pp, floor) as PanelPlanar;
                            aResult.Add(pp);
                        }

                        return aResult;
                    }
            }

            return null;
        }

        public static List<BHoMObject> ToBHoM(this RoofBase roofBase, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        // we need the same like this for walls?


                        List<BHoMObject> aResult = new List<BHoMObject>();
                        BuildingElementProperties aBuildingElementProperties = roofBase.RoofType.ToBHoM(discipline, copyCustomData) as BuildingElementProperties;
                        foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(roofBase))
                        {
                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(roofBase.Document.GetElement(roofBase.LevelId) as Level, discipline) as BH.oM.Architecture.Elements.Level);

                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, roofBase) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, roofBase) as BuildingElement;

                            aResult.Add(aBuildingElement);
                        }
                        return aResult;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit WallType
        /// </summary>
        /// <param name="wallType">Revit WallType</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit WallType
        /// </search>
        public static BHoMObject ToBHoM(this WallType wallType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, string materialGrade = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, wallType.Name);

                        aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, wallType) as BuildingElementProperties;
                        if (copyCustomData)
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType) as BuildingElementProperties;

                        return aBuildingElementProperties;
                    }

                case Discipline.Structural:
                    {
                        Document document = wallType.Document;
                        double aThickness = 0;
                        oM.Common.Materials.Material aMaterial = new oM.Common.Materials.Material();
                        foreach (CompoundStructureLayer csl in wallType.GetCompoundStructure().GetLayers())
                        {
                            if (csl.Function == MaterialFunctionAssignment.Structure)
                            {
                                aThickness = csl.Width * feetToMetre;
                                Material m = document.GetElement(csl.MaterialId) as Material;
                                aMaterial = m.ToBHoM(materialGrade);         // this is dangerous for multilayer panels?
                                break;
                            }
                        }

                        ConstantThickness aProperty2D = new ConstantThickness { Type = oM.Structural.Properties.PanelType.Wall, Thickness = aThickness, Material = aMaterial };

                        aProperty2D = Modify.SetIdentifiers(aProperty2D, wallType) as ConstantThickness;
                        if (copyCustomData)
                            aProperty2D = Modify.SetCustomData(aProperty2D, wallType) as ConstantThickness;

                        return aProperty2D;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit FloorType
        /// </summary>
        /// <param name="floorType">Revit FloorType</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit FloorType
        /// </search>
        public static BHoMObject ToBHoM(this FloorType floorType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, string materialGrade = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Floor, floorType.Name);

                        aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, floorType) as BuildingElementProperties;
                        if (copyCustomData)
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType) as BuildingElementProperties;

                        return aBuildingElementProperties;
                    }

                case Discipline.Structural:
                    {
                        Document document = floorType.Document;
                        double aThickness = 0;
                        oM.Common.Materials.Material aMaterial = new oM.Common.Materials.Material();
                        foreach (CompoundStructureLayer csl in floorType.GetCompoundStructure().GetLayers())
                        {
                            if (csl.Function == MaterialFunctionAssignment.Structure)
                            {
                                aThickness = csl.Width * feetToMetre;
                                Material m = document.GetElement(csl.MaterialId) as Material;
                                aMaterial = m.ToBHoM(materialGrade);         // this is dangerous for multilayer panels?
                                break;
                            }
                        }
                            
                        ConstantThickness aProperty2D = new ConstantThickness { Type = oM.Structural.Properties.PanelType.Slab, Thickness = aThickness, Material = aMaterial };

                        aProperty2D = Modify.SetIdentifiers(aProperty2D, floorType) as ConstantThickness;
                        if (copyCustomData)
                            aProperty2D = Modify.SetCustomData(aProperty2D, floorType) as ConstantThickness;

                        return aProperty2D;
                    }

            }
            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit CeilingType
        /// </summary>
        /// <param name="ceilingType">Revit FloorType</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit CeilingType
        /// </search>
        public static BHoMObject ToBHoM(this CeilingType ceilingType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Ceiling, ceilingType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, ceilingType) as BuildingElementProperties;
            if (copyCustomData)
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType) as BuildingElementProperties;

            return aBuildingElementProperties;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit RoofType
        /// </summary>
        /// <param name="roofType">Revit FloorType</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit RoofType
        /// </search>
        public static BHoMObject ToBHoM(this RoofType roofType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Roof, roofType.Name);

                        aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, roofType) as BuildingElementProperties;
                        if (copyCustomData)
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType) as BuildingElementProperties;

                        return aBuildingElementProperties;
                    }
            }

            return null;
        }

        public static ISectionProperty ToBHoMSection(this FamilyInstance familyInstance, oM.Geometry.Line centreLine, bool copyCustomData = true)
        {
            try
            {
                string materialGrade = familyInstance.GetMaterialGrade();

                oM.Common.Materials.Material aMaterial = familyInstance.StructuralMaterialType.ToBHoM(materialGrade);
                ISectionProperty aSectionProperty;
                ISectionDimensions aSectionDimensions;

                string name = familyInstance.Symbol.Name;
                aSectionDimensions = BH.Engine.Library.Query.Match("UK_SteelSectionDimensions", name) as ISectionDimensions;

                if (aSectionDimensions == null)
                {
                    aSectionDimensions = familyInstance.Symbol.ToBHoMSectionDimensions();
                }
                
                if (aSectionDimensions != null)
                {
                    //TODO: shouldn't we have AluminiumSection and TimberSection at least?
                    if (aMaterial.Type == oM.Common.Materials.MaterialType.Concrete)
                    {
                        return BHS.Create.ConcreteSectionFromDimensions(aSectionDimensions, aMaterial, name);
                    }
                    else if (aMaterial.Type == oM.Common.Materials.MaterialType.Steel)
                    {
                        return BHS.Create.SteelSectionFromDimensions(aSectionDimensions, aMaterial, name);
                    }
                    else throw new Exception("Material not implemented yet.");
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
                                XYZ direction = (centreLine.ToRevit() as Line).Direction;
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
                    profileCurves = profileCurves.Select(c => Geometry.Modify.IScale(c, origin, feetToMetreVector)).ToList();

                    //TODO: shouldn't we have AluminiumSection and TimberSection at least?
                    if (aMaterial.Type == oM.Common.Materials.MaterialType.Concrete)
                    {
                        return BHS.Create.ConcreteFreeFormSection(profileCurves, aMaterial, name);
                    }
                    else if (aMaterial.Type == oM.Common.Materials.MaterialType.Steel)
                    {
                        return BHS.Create.SteelFreeFormSection(profileCurves, aMaterial, name);
                    }
                    else throw new Exception("Material not implemented yet.");
                }
            }
            catch
            {
                return null;
            }
        }

        //TODO: change return type to BHoMObject, add discipine as parameter
        public static oM.Common.Materials.Material ToBHoM(this StructuralMaterialType structuralMaterialType, string materialGrade)
        {
            switch (structuralMaterialType)
            {
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum:
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.PrecastConcrete:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject concrete in Library.Query.Match("MaterialsEurope", "Type", "Concrete"))
                        {
                            if (materialGrade.Contains((concrete).Name))
                            {
                                return concrete as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject steel in Library.Query.Match("MaterialsEurope", "Type", "Steel"))
                        {
                            if (materialGrade.Contains((steel).Name))
                            {
                                return steel as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "S355") as oM.Common.Materials.Material;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood:
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                default:
                    return new oM.Common.Materials.Material();
            }
        }

        //TODO: change return type to BHoMObject, add discipine as parameter
        public static oM.Common.Materials.Material ToBHoM(this Material material, string materialGrade)
        {
            switch (material.MaterialClass)
            {
                case "Aluminium":
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;
                case "Concrete":
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject concrete in Library.Query.Match("MaterialsEurope", "Type", "Concrete"))
                        {
                            if (materialGrade.Contains((concrete).Name))
                            {
                                return concrete as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                case "Steel":
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject steel in Library.Query.Match("MaterialsEurope", "Type", "Steel"))
                        {
                            if (materialGrade.Contains((steel).Name))
                            {
                                return steel as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "S355") as oM.Common.Materials.Material;
                case "Wood":
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                default:
                    return new oM.Common.Materials.Material();
            }
        }

        /// <summary>
        /// Gets BHoM Storey from Revit Level
        /// </summary>
        /// <param name="Level">Revit Level</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Storey">BHoM Storey</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Storey, Revit Level
        /// </search>
        public static BHoMObject ToBHoM(this Level Level, Discipline discipline = Discipline.Environmental, bool CopyCustomData = true)
        {
            switch(discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                    //TODO: Update constructor for Level to include Name
                    oM.Architecture.Elements.Level aLevel = Architecture.Elements.Create.Level(Level.Elevation);
                    aLevel.Name = Level.Name;

                    aLevel = Modify.SetIdentifiers(aLevel, Level) as oM.Architecture.Elements.Level;
                    if (CopyCustomData)
                        aLevel = Modify.SetCustomData(aLevel, Level) as oM.Architecture.Elements.Level;

                    return aLevel;
                case Discipline.Structural:
                    Storey aStorey = Structure.Create.Storey(Level.Name, Level.Elevation, 0);

                    aStorey = Modify.SetIdentifiers(aStorey, Level) as Storey;
                    if (CopyCustomData)
                        aStorey = Modify.SetCustomData(aStorey, Level) as Storey;

                    return aStorey;
            }

            return null;
        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="objects">BHoM Objects</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, Dictionary<ElementId, List<BHoMObject>> objects, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                        aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                        aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

                        SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, aSpatialElementBoundaryOptions);

                        return ToBHoM(spatialElement, aSpatialElementGeometryCalculator, null, null, discipline, copyCustomData);
                    }
            }
            return null;
        }

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="spatialElementBoundaryOptions">Revit SpatialElementBoundaryOptions</param>
        /// <param name="objects"> BHoM Objects</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, Dictionary<ElementId, List<BHoMObject>> objects, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        if (spatialElement == null || spatialElementBoundaryOptions == null)
                            return null;

                        Document aDocument = spatialElement.Document;

                        oM.Architecture.Elements.Level aLevel = null;
                        if (objects != null)
                        {
                            List<BHoMObject> aBHoMObjectList = new List<BHoMObject>();
                            if (objects.TryGetValue(spatialElement.LevelId, out aBHoMObjectList))
                                if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                                    aLevel = aBHoMObjectList.First() as oM.Architecture.Elements.Level;
                        }

                        if (aLevel == null)
                        {
                            aLevel = spatialElement.Level.ToBHoM() as oM.Architecture.Elements.Level;
                            if (objects != null)
                                objects.Add(spatialElement.LevelId, new List<BHoMObject>(new BHoMObject[] { aLevel }));
                        } 

                        List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
                        IList<IList<BoundarySegment>> aBoundarySegmentListList = spatialElement.GetBoundarySegments(spatialElementBoundaryOptions);
                        if (aBoundarySegmentListList != null)
                            foreach (IList<BoundarySegment> aBoundarySegmentList in aBoundarySegmentListList)
                                foreach (BoundarySegment aBoundarySegment in aBoundarySegmentList)
                                {
                                    oM.Geometry.ICurve aICurve = aBoundarySegment.GetCurve().ToBHoM();
                                    Element aElement = aDocument.GetElement(aBoundarySegment.ElementId);
                                    ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                                    BuildingElementProperties aBuildingElementProperties = null;
                                    if (objects != null)
                                    {
                                        List<BHoMObject> aBHoMObjectList = new List<BHoMObject>();
                                        if (objects.TryGetValue(aElementType.Id, out aBHoMObjectList))
                                            if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                                                aBuildingElementProperties = aBHoMObjectList.First() as BuildingElementProperties;
                                    }

                                    if (aBuildingElementProperties == null)
                                    {
                                        if (aElement is Wall)
                                            aBuildingElementProperties = (aElement as Wall).WallType.ToBHoM() as BuildingElementProperties;
                                        else if (aElement is Floor)
                                            aBuildingElementProperties = (aElement as Floor).FloorType.ToBHoM() as BuildingElementProperties;
                                        else if (aElement is Ceiling)
                                            aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as CeilingType).ToBHoM() as BuildingElementProperties;

                                        if (objects != null)
                                            objects.Add(aElementType.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElementProperties }));
                                    }

                                    BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementCurve(aICurve), aLevel);
                                    aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                                    if (copyCustomData)
                                        aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement) as BuildingElement;
                                    aBuildingElmementList.Add(aBuildingElement);
                                }

                        Space aSpace = new Space
                        {
                            Level = aLevel,
                            BuildingElements = aBuildingElmementList,
                            Name = spatialElement.Name,
                            Location = (spatialElement.Location as LocationPoint).ToBHoM()

                        };

                        aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
                        if (copyCustomData)
                            aSpace = Modify.SetCustomData(aSpace, spatialElement) as Space;

                        return aSpace;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="spatialElementGeometryCalculator">Revit SpatialElementGeometryCalculator</param>
        /// <param name="buildingElementProperties">Revit BuildingElementProperties</param>
        /// <param name="levels">BHoM Levels</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, IEnumerable<BuildingElementProperties> buildingElementProperties, IEnumerable<oM.Architecture.Elements.Level> levels, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        if (spatialElement == null || spatialElementGeometryCalculator == null)
                            return null;

                        if (!SpatialElementGeometryCalculator.CanCalculateGeometry(spatialElement))
                            return null;

                        SpatialElementGeometryResults aSpatialElementGeometryResults = spatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);

                        Solid aSolid = aSpatialElementGeometryResults.GetGeometry();
                        if (aSolid == null)
                            return null;

                        oM.Architecture.Elements.Level aLevel = null;
                        if (buildingElementProperties != null)
                        {
                            foreach (oM.Architecture.Elements.Level aLevel_Temp in levels)
                            {
                                if (aLevel_Temp.Elevation == spatialElement.Level.Elevation)
                                {
                                    aLevel = aLevel_Temp;
                                    break;
                                }
                            }
                        }

                        if (aLevel == null)
                            aLevel = spatialElement.Level.ToBHoM(discipline, copyCustomData) as oM.Architecture.Elements.Level;


                        List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
                        foreach (Face aFace in aSolid.Faces)
                        {
                            foreach (SpatialElementBoundarySubface aSpatialElementBoundarySubface in aSpatialElementGeometryResults.GetBoundaryFaceInfo(aFace))
                            {
                                LinkElementId aLinkElementId = aSpatialElementBoundarySubface.SpatialBoundaryElement;
                                Document aDocument = null;
                                if (aLinkElementId.LinkInstanceId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                                    aDocument = (spatialElement.Document.GetElement(aLinkElementId.LinkInstanceId) as RevitLinkInstance).GetLinkDocument();
                                else
                                    aDocument = spatialElement.Document;

                                Element aElement = null;
                                if (aLinkElementId.LinkedElementId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                                    aElement = aDocument.GetElement(aLinkElementId.LinkedElementId);
                                else
                                    aElement = aDocument.GetElement(aLinkElementId.HostElementId);

                                ElementType aElementType = null;
                                if (aElement != null)
                                    aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                                BuildingElementProperties aBuildingElementProperties = null;
                                if (aElementType != null && buildingElementProperties != null)
                                {
                                    foreach (BuildingElementProperties aBuildingElementProperties_Temp in buildingElementProperties)
                                    {
                                        if (aBuildingElementProperties_Temp.Name == aElementType.Name)
                                        {
                                            aBuildingElementProperties = aBuildingElementProperties_Temp;
                                            break;
                                        }
                                    }
                                }

                                //Face aFace_BoundingElementFace = aSpatialElementBoundarySubface.GetBoundingElementFace();
                                //Face aFace_Subface = aSpatialElementBoundarySubface.GetSubface();
                                //Face aFace_SpatialElementFace = aSpatialElementBoundarySubface.GetSpatialElementFace();
                                Face aFace_BuildingElement = aSpatialElementBoundarySubface.GetSubface();
                                if(aFace_BuildingElement == null)
                                    aFace_BuildingElement = aSpatialElementBoundarySubface.GetSpatialElementFace();

                                if (aFace_BuildingElement != null)
                                    foreach (CurveLoop aCurveLoop in aFace_BuildingElement.GetEdgesAsCurveLoops())
                                    {
                                        BuildingElement aBuildingElement = null;
                                        if (aBuildingElementProperties == null)
                                        {
                                            if (aElement is Wall)
                                                aBuildingElementProperties = (aElement as Wall).WallType.ToBHoM(discipline, copyCustomData) as BuildingElementProperties;
                                            else if (aElement is Floor)
                                                aBuildingElementProperties = (aElement as Floor).FloorType.ToBHoM(discipline, copyCustomData) as BuildingElementProperties;
                                            else if (aElement is Ceiling)
                                                aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as CeilingType).ToBHoM(discipline, copyCustomData) as BuildingElementProperties;
                                            else if (aElement is FootPrintRoof || aElement is ExtrusionRoof)
                                                aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as RoofType).ToBHoM(discipline, copyCustomData) as BuildingElementProperties;
                                        }

                                        aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementPanel(aCurveLoop.ToBHoM()));
                                        aBuildingElement.Level = aLevel;
                                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                                        if (copyCustomData)
                                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement) as BuildingElement;
                                        aBuildingElmementList.Add(aBuildingElement);
                                    }
                            }
                        }

                        Space aSpace = new Space
                        {
                            Level = aLevel,
                            BuildingElements = aBuildingElmementList,
                            Name = spatialElement.Name,
                            Location = (spatialElement.Location as LocationPoint).ToBHoM()

                        };

                        aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
                        if (copyCustomData)
                            aSpace = Modify.SetCustomData(aSpace, spatialElement) as Space;

                        return aSpace;
                    }
            }

            return null;

        }

        /***************************************************/

        public static BHoMObject ToBHoM(this Grid grid, Discipline discipline = Discipline.Architecture, bool copyCustomData = true)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                    {
                        Line gridLine = grid.Curve as Line;
                        oM.Architecture.Elements.Grid aGrid = Architecture.Elements.Create.Grid(Geometry.Modify.IScale(gridLine.ToBHoM(), origin, feetToMetreVector));
                        aGrid.Name = grid.Name;
                        return aGrid;
                    }
            }
            return null;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        //TODO: Move to Revit2018_Engine.Query
        private static List<oM.Geometry.Polyline> GetBHOutlines(this Wall wall)
        {
            List<Curve> curves = wall.GetAnalyticalModel().GetCurves(AnalyticalCurveType.RawCurves).ToList();
            List<oM.Geometry.Line> lines = curves.Select(c => BH.Engine.Geometry.Modify.Scale((oM.Geometry.Line)c.ToBHoM(), origin, feetToMetreVector)).ToList();
            return Geometry.Modify.Join(lines);
        }

        /***************************************************/

        //TODO: Move to Revit2018_Engine.Query
        private static List<oM.Geometry.Polyline> GetBHOutlines(this Floor floor)
        {
            List<Curve> curves = floor.GetAnalyticalModel().GetCurves(AnalyticalCurveType.RawCurves).ToList();
            List<oM.Geometry.Line> lines = curves.Select(c => BH.Engine.Geometry.Modify.Scale((oM.Geometry.Line)c.ToBHoM(), origin, feetToMetreVector)).ToList();
            return Geometry.Modify.Join(lines);
        }

        private static string GetMaterialGrade(this Element element)
        {
            string materialGrade;
            try
            {
                materialGrade = element.LookupParameter("BHE_Material Grade").AsString();
            }
            catch
            {
                materialGrade = null;
            }
            return materialGrade;
        }
        
        private static double feetToMetre = UnitUtils.ConvertFromInternalUnits(1, DisplayUnitType.DUT_METERS);
        private static oM.Geometry.Point origin = new oM.Geometry.Point { X = 0, Y = 0, Z = 0 };
        private static oM.Geometry.Vector feetToMetreVector = new oM.Geometry.Vector { X = feetToMetre, Y = feetToMetre, Z = feetToMetre };

        private static ISectionDimensions ToBHoMSectionDimensions(this FamilySymbol familySymbol)
        {
            StructuralSectionShape sectionShape = (StructuralSectionShape)familySymbol.LookupParameter("Section Shape").AsInteger();
            List<Type> aTypes = Engine.Revit.Query.BHoMTypes(sectionShape);

            if (aTypes.Count == 0) return null;

            if (aTypes.Contains(typeof(CircleDimensions)))
            {
                double diameter = familySymbol.LookupParameterDouble(diameterNames, true);
                if (!double.IsNaN(diameter))
                {
                    return new CircleDimensions(diameter);
                }

                double radius = familySymbol.LookupParameterDouble(radiusNames, true);
                if (!double.IsNaN(radius))
                {
                    return new CircleDimensions(radius * 2);
                }
            }

            else if (aTypes.Contains(typeof(FabricatedISectionDimensions)))
            {
                double height = familySymbol.LookupParameterDouble(heightNames, true);
                double topFlangeWidth = familySymbol.LookupParameterDouble(topFlangeWidthNames, true);
                double botFlangeWidth = familySymbol.LookupParameterDouble(botFlangeWidthNames, true);
                double webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                double topFlangeThickness = familySymbol.LookupParameterDouble(topFlangeThicknessNames, true);
                double botFlangeThickness = familySymbol.LookupParameterDouble(botFlangeThicknessNames, true);
                double weldSize = familySymbol.LookupParameterDouble(weldSizeNames1, true);

                if (double.IsNaN(weldSize))
                {
                    weldSize = familySymbol.LookupParameterDouble(weldSizeNames2, true);
                    if (!double.IsNaN(weldSize) && !double.IsNaN(webThickness))
                    {
                        weldSize = (weldSize - webThickness) / (Math.Sqrt(2));
                    }
                    else
                    {
                        weldSize = 0;
                    }
                }

                if (!double.IsNaN(height) && !double.IsNaN(topFlangeWidth) && !double.IsNaN(botFlangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(topFlangeThickness) && !double.IsNaN(botFlangeThickness))
                {
                    return new FabricatedISectionDimensions(height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize);
                }
            }

            else if (aTypes.Contains(typeof(RectangleSectionDimensions)))
            {
                double height = familySymbol.LookupParameterDouble(heightNames, true);
                double width = familySymbol.LookupParameterDouble(widthNames, true);
                double cornerRadius = familySymbol.LookupParameterDouble(cornerRadiusNames, true);

                if (double.IsNaN(cornerRadius)) cornerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(width))
                {
                    return new RectangleSectionDimensions(height, width, cornerRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardAngleSectionDimensions)))
            {
                double height = familySymbol.LookupParameterDouble(heightNames, true);
                double width = familySymbol.LookupParameterDouble(widthNames, true);
                double webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                double flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                double rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                double toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return new StandardAngleSectionDimensions(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardBoxDimensions)))
            {
                double height = familySymbol.LookupParameterDouble(heightNames, true);
                double width = familySymbol.LookupParameterDouble(widthNames, true);
                double thickness = familySymbol.LookupParameterDouble(wallThicknessNames, true);
                double outerRadius = familySymbol.LookupParameterDouble(outerRadiusNames, true);
                double innerRadius = familySymbol.LookupParameterDouble(innerRadiusNames, true);

                if (double.IsNaN(outerRadius)) outerRadius = 0;
                if (double.IsNaN(innerRadius)) innerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(thickness) && !double.IsNaN(outerRadius) && !double.IsNaN(innerRadius))
                {
                    return new StandardBoxDimensions(height, width, thickness, outerRadius, innerRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardChannelSectionDimensions)))
            {
                double height = familySymbol.LookupParameterDouble(heightNames, true);
                double flangeWidth = familySymbol.LookupParameterDouble(widthNames, true);
                double webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                double flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                double rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                double toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && double.IsNaN(toeRadius))
                {
                    return new StandardChannelSectionDimensions(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardISectionDimensions)))
            {
                double height = familySymbol.LookupParameterDouble(heightNames, true);
                double width = familySymbol.LookupParameterDouble(widthNames, true);
                double webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                double flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                double rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                double toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return new StandardISectionDimensions(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardTeeSectionDimensions)))
            {
                double height = familySymbol.LookupParameterDouble(heightNames, true);
                double width = familySymbol.LookupParameterDouble(widthNames, true);
                double webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                double flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                double rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                double toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return new StandardTeeSectionDimensions(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardZedSectionDimensions)))
            {
                double height = familySymbol.LookupParameterDouble(heightNames, true);
                double flangeWidth = familySymbol.LookupParameterDouble(widthNames, true);
                double webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                double flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                double rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                double toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return new StandardZedSectionDimensions(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(TubeDimensions)))
            {
                double thickness = familySymbol.LookupParameterDouble(wallThicknessNames, true);
                double diameter = familySymbol.LookupParameterDouble(diameterNames, true);
                if (!double.IsNaN(diameter) && !double.IsNaN(thickness))
                {
                    return new TubeDimensions(diameter, thickness);
                }

                double radius = familySymbol.LookupParameterDouble(radiusNames, true);
                if (!double.IsNaN(radius) && !double.IsNaN(thickness))
                {
                    return new TubeDimensions(radius * 2, thickness);
                }
            }

            return null;
        }

        public static string[] diameterNames = { "BHE_Diameter", "Diameter", "diameter", "DIAMETER", "D", "d" };
        public static string[] radiusNames = { "BHE_Radius", "Radius", "radius", "RADIUS", "R", "r" };
        public static string[] heightNames = { "BHE_Height", "Height", "height", "HEIGHT", "H", "h", "d" };
        public static string[] widthNames = { "BHE_Width", "Width", "width", "WIDTH", "b", "B", "w", "W", "bf" };
        public static string[] cornerRadiusNames = { "Corner Radius", "r", "r1" };
        public static string[] topFlangeWidthNames = { "Top Flange Width", "bt", "tf_b", "b1", "b", "B" };
        public static string[] botFlangeWidthNames = { "Bottom Flange Width", "bb", "bf_b", "b2", "b", "B" };
        public static string[] webThicknessNames = { "Web Thickness", "tw", "t" };
        public static string[] topFlangeThicknessNames = { "Top Flange Thickness", "tft", "tf_t", "tf", "t", "T" };
        public static string[] botFlangeThicknessNames = { "Bottom Flange Thickness", "tfb", "tf_b", "tf", "t", "T" };
        public static string[] flangeThicknessNames = { "Flange Thickness", "tf", "T", "t" };
        public static string[] weldSizeNames1 = { "Weld Size" };                                            // weld size, diagonal
        public static string[] weldSizeNames2 = { "k" };                                                    // weld size counted from bar's vertical axis
        public static string[] rootRadiusNames = { "Root Radius", "r", "r1", "R1" };
        public static string[] toeRadiusNames = { "Toe Radius", "r2", "R2" };
        public static string[] innerRadiusNames = { "Inner Radius", "r1", "R1" };
        public static string[] outerRadiusNames = { "Outer Radius", "r2", "R2" };
        public static string[] wallThicknessNames = { "Wall Nominal Thickness", "Wall Thickness", "t" };

    }
}
 