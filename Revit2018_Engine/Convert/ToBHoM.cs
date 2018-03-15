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

        /***************************************************/

        /// <summary>
        /// Gets BHoM BuildingElementPanels from Revit Floor
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <returns name="BuildingElementPanels">BHoM BuildingElementPanels</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementPanels, Revit Wall, BuildingElementPanel, ToBHoMBuildingElementPanels
        /// </search>
        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this Element element)
        {
            return ToBHoMBuildingElementPanels(element.get_Geometry(new Options()));
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

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this FamilyInstance familyInstance)
        {
            List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();

            //TODO: Get more accurate shape. Currently, Windows and doors goes as rectangular panel
            BoundingBoxXYZ aBoundingBoxXYZ = familyInstance.get_BoundingBox(null);

            XYZ aVector = aBoundingBoxXYZ.Max - aBoundingBoxXYZ.Min;

            double aWidth = Math.Abs(aVector.Y);
            double aHeight = Math.Abs(aVector.Z);

            XYZ aVector_Y = (aBoundingBoxXYZ.Transform.BasisY * aWidth) / 2;
            XYZ aVector_Z = (aBoundingBoxXYZ.Transform.BasisZ * aHeight) / 2;

            XYZ aMiddle = (aBoundingBoxXYZ.Max + aBoundingBoxXYZ.Min) / 2;

            XYZ aXYZ_1 = aMiddle + aVector_Z - aVector_Y;
            XYZ aXYZ_2 = aMiddle + aVector_Z + aVector_Y;
            XYZ aXYZ_3 = aMiddle - aVector_Z + aVector_Y;
            XYZ aXYZ_4 = aMiddle - aVector_Z - aVector_Y;

            List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
            aPointList.Add(aXYZ_1.ToBHoM());
            aPointList.Add(aXYZ_2.ToBHoM());
            aPointList.Add(aXYZ_3.ToBHoM());
            aPointList.Add(aXYZ_4.ToBHoM());


            BuildingElementPanel aBuildingElementPanel = Create.BuildingElementPanel(new oM.Geometry.Polyline[] { Geometry.Create.Polyline(aPointList) });
            if (aBuildingElementPanel != null)
                aResult.Add(aBuildingElementPanel);

            return aResult;
        }

        /***************************************************/

        public static BuildingElement ToBHoMBuildingElement(this Element element, BuildingElementPanel buildingElementPanel, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true)
        {
            ElementType aElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
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
                BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)aElementType.Category.Id.IntegerValue);
                if (!aBuildingElementType.HasValue)
                    return null;

                aBuildingElementProperties = Create.BuildingElementProperties(aBuildingElementType.Value, aElementType.Name);
                aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, aElementType) as BuildingElementProperties;
                if (copyCustomData)
                    aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, aElementType) as BuildingElementProperties;

                if (objects != null)
                    objects.Add(aElementType.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElementProperties }));
            }


            oM.Architecture.Elements.Level aLevel = Query.Level(element, objects);
            if (aLevel == null)
                return null;

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, buildingElementPanel);
            aBuildingElement.Level = aLevel;
            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, element) as BuildingElement;
            if (copyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, element) as BuildingElement;

            if (objects != null)
            {
                List<BHoMObject> aBHoMObjectList = null;
                if (objects.TryGetValue(element.Id, out aBHoMObjectList))
                    aBHoMObjectList.Add(aBuildingElement);
                else
                    objects.Add(element.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElement }));
            }

            return aBuildingElement;
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
            if (familyInstance == null)
                return null;

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
                        BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)familyInstance.Category.Id.IntegerValue);
                        if(aBuildingElementType.HasValue)
                        {
                            List<BuildingElementPanel> aBuildingElementPanelList = ToBHoMBuildingElementPanels(familyInstance);
                            if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                                return ToBHoMBuildingElement(familyInstance, aBuildingElementPanelList.First(), null, copyCustomData);
                        }
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
        /// Gets BHoM BuildingElement from Revit Ceiling
        /// </summary>
        /// <param name="ceiling">Revit Ceiling</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit Ceiling
        /// </search>
        public static List<BHoMObject> ToBHoM(this Ceiling ceiling, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BHoMObject> aResult = new List<BHoMObject>();
                        BuildingElementProperties aBuildingElementProperties = (ceiling.Document.GetElement( ceiling.GetTypeId()) as CeilingType).ToBHoM(discipline, copyCustomData) as BuildingElementProperties;
                        foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(ceiling))
                        {
                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(ceiling.Document.GetElement(ceiling.LevelId) as Level, discipline) as BH.oM.Architecture.Elements.Level);

                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, ceiling) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, ceiling) as BuildingElement;

                            aResult.Add(aBuildingElement);
                        }
                        return aResult;
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
                case "Metal":
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
                    oM.Architecture.Elements.Level aLevel = Architecture.Elements.Create.Level(Level.Elevation * feetToMetre);
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
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                        aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                        aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

                        SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, aSpatialElementBoundaryOptions);
                        
                        return ToBHoM(spatialElement, aSpatialElementGeometryCalculator, objects, discipline, copyCustomData);
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
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
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

                                    if (objects != null)
                                    {
                                        List<BHoMObject> aBHoMObjectList = null;
                                        if (objects.TryGetValue(aElement.Id, out aBHoMObjectList))
                                            aBHoMObjectList.Add(aBuildingElement);
                                        else
                                            objects.Add(aElement.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElement }));
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

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="spatialElementGeometryCalculator">Revit SpatialElementGeometryCalculator</param>
        /// <param name="objects">BHoM Objects</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true)
        {
            switch (discipline)
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

                        oM.Architecture.Elements.Level aLevel = Query.Level(spatialElement, objects);

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

                                if (aElement == null)
                                    continue;

                                ElementType aElementType = null;
                                if (aElement != null)
                                    aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

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

                                //Face aFace_BoundingElementFace = aSpatialElementBoundarySubface.GetBoundingElementFace();
                                //Face aFace_Subface = aSpatialElementBoundarySubface.GetSubface();
                                //Face aFace_SpatialElementFace = aSpatialElementBoundarySubface.GetSpatialElementFace();
                                Face aFace_BuildingElement = aSpatialElementBoundarySubface.GetSubface();
                                if (aFace_BuildingElement == null)
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


                                        //---- Get Hosted Building Elements -----------
                                        List<BuildingElement> aBuildingElmementList_Hosted = Query.HostedBuildingElements(aElement as HostObject, aFace_BuildingElement, objects, copyCustomData);
                                        if (aBuildingElmementList_Hosted != null && aBuildingElmementList_Hosted.Count > 0)
                                            aBuildingElmementList.AddRange(aBuildingElmementList_Hosted);
                                        //---------------------------------------------

                                        if (objects != null)
                                        {
                                            List<BHoMObject> aBHoMObjectList = null;
                                            if (objects.TryGetValue(aElement.Id, out aBHoMObjectList))
                                                aBHoMObjectList.Add(aBuildingElement);
                                            else
                                                objects.Add(aElement.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElement }));
                                        }



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
            List<Curve> curves;
            if (wall.GetAnalyticalModel() != null)
            {
                curves = wall.GetAnalyticalModel().GetCurves(AnalyticalCurveType.RawCurves).ToList();
            }
            else
            {
                curves = new List<Curve>();
                LocationCurve aLocationCurve = wall.Location as LocationCurve;
                XYZ direction = (aLocationCurve.Curve as Line).Direction;
                XYZ normal = new XYZ(-direction.Y, direction.X, 0);
                foreach (GeometryObject obj in wall.get_Geometry(new Options()))
                {
                    if (obj is Solid)
                    {
                        foreach (PlanarFace face in (obj as Solid).Faces)
                        {
                            if (face.FaceNormal.IsAlmostEqualTo(normal))
                            {
                                EdgeArrayArray aEdgeArrayArray = face.EdgeLoops;
                                if (aEdgeArrayArray != null && aEdgeArrayArray.Size > 0)
                                {
                                    for (int i = 0; i < aEdgeArrayArray.Size; i++)
                                    {
                                        EdgeArray aEdgeArray = aEdgeArrayArray.get_Item(i);
                                        foreach (Autodesk.Revit.DB.Edge aEdge in aEdgeArray)
                                        {
                                            Curve aCurve = aEdge.AsCurve();
                                            if (aCurve != null)
                                                curves.Add(aCurve);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            List<oM.Geometry.ICurve> crvs = curves.Select(c => c.ToBHoM()).ToList();
            List<oM.Geometry.Line> lines = crvs.Discretize(8).Select(l => BH.Engine.Geometry.Modify.Scale(l, origin, feetToMetreVector)).ToList();
            return Geometry.Modify.Join(lines);
        }

        /***************************************************/

        //TODO: Move to Revit2018_Engine.Query
        private static List<oM.Geometry.Polyline> GetBHOutlines(this Floor floor)
        {
            List<Curve> curves;
            if (floor.GetAnalyticalModel() != null)
            {
                curves = floor.GetAnalyticalModel().GetCurves(AnalyticalCurveType.RawCurves).ToList();
            }
            else
            {
                curves = new List<Curve>();
                foreach (GeometryObject obj in floor.get_Geometry(new Options()))
                {
                    if (obj is Solid)
                    {
                        PlanarFace top = Query.Top(obj as Solid);
                        EdgeArrayArray aEdgeArrayArray = top.EdgeLoops;
                        if (aEdgeArrayArray != null && aEdgeArrayArray.Size > 0)
                        {
                            for (int i = 0; i < aEdgeArrayArray.Size; i++)
                            {
                                EdgeArray aEdgeArray = aEdgeArrayArray.get_Item(i);
                                foreach (Autodesk.Revit.DB.Edge aEdge in aEdgeArray)
                                {
                                    Curve aCurve = aEdge.AsCurve();
                                    if (aCurve != null)
                                        curves.Add(aCurve);
                                }
                            }
                        }
                    }
                }
            }
            List<oM.Geometry.ICurve> crvs = curves.Select(c => c.ToBHoM()).ToList();
            List<oM.Geometry.Line> lines = crvs.Discretize(8).Select(l => BH.Engine.Geometry.Modify.Scale(l, origin, feetToMetreVector)).ToList();
            return Geometry.Modify.Join(lines);
        }

        //TODO: Move to Revit2018_Engine.Query
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

        private static ISectionDimensions ToBHoMSectionDimensions(this FamilySymbol familySymbol)
        {
            string familyName = familySymbol.Family.Name;
            StructuralSectionShape sectionShape = (StructuralSectionShape)familySymbol.LookupParameter("Section Shape").AsInteger();
            List<Type> aTypes = Engine.Revit.Query.BHoMTypes(familyName);
            aTypes.AddRange(Engine.Revit.Query.BHoMTypes(sectionShape));

            if (aTypes.Count == 0) return null;

            if (aTypes.Contains(typeof(CircleDimensions)))
            {
                double diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter * feetToMetre;
                else if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter * feetToMetre;
                else diameter = familySymbol.LookupParameterDouble(diameterNames, true);

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
                double height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionIWelded)
                {
                    StructuralSectionIWelded sectionType = section as StructuralSectionIWelded;
                    height = sectionType.Height * feetToMetre;
                    topFlangeWidth = sectionType.TopFlangeWidth * feetToMetre;
                    botFlangeWidth = sectionType.BottomFlangeWidth * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    topFlangeThickness = sectionType.TopFlangeThickness * feetToMetre;
                    botFlangeThickness = sectionType.BottomFlangeThickness * feetToMetre;
                    weldSize = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    topFlangeWidth = familySymbol.LookupParameterDouble(topFlangeWidthNames, true);
                    botFlangeWidth = familySymbol.LookupParameterDouble(botFlangeWidthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    topFlangeThickness = familySymbol.LookupParameterDouble(topFlangeThicknessNames, true);
                    botFlangeThickness = familySymbol.LookupParameterDouble(botFlangeThicknessNames, true);
                    weldSize = familySymbol.LookupParameterDouble(weldSizeNames1, true);
                }

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
                double height, width, cornerRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRectangle)
                {
                    StructuralSectionConcreteRectangle sectionType = section as StructuralSectionConcreteRectangle;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    cornerRadius = 0;
                }
                else if (section is StructuralSectionRectangularBar)
                {
                    StructuralSectionRectangularBar sectionType = section as StructuralSectionRectangularBar;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    cornerRadius = 0;
                }
                else if (section is StructuralSectionRectangleParameterized)
                {
                    StructuralSectionRectangleParameterized sectionType = section as StructuralSectionRectangleParameterized;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    cornerRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    cornerRadius = familySymbol.LookupParameterDouble(cornerRadiusNames, true);
                }

                if (double.IsNaN(cornerRadius)) cornerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(width))
                {
                    return new RectangleSectionDimensions(height, width, cornerRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardAngleSectionDimensions)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionLAngle)
                {
                    StructuralSectionLAngle sectionType = section as StructuralSectionLAngle;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionLProfile)
                {
                    //TODO: Implement cold-formed profiles?
                    StructuralSectionLProfile sectionType = section as StructuralSectionLProfile;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WallNominalThickness * feetToMetre;
                    flangeThickness = sectionType.WallNominalThickness * feetToMetre;
                    rootRadius = sectionType.InnerFillet * feetToMetre;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return new StandardAngleSectionDimensions(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardBoxDimensions)))
            {
                double height, width, thickness, outerRadius, innerRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionRectangleHSS)
                {
                    StructuralSectionRectangleHSS sectionType = section as StructuralSectionRectangleHSS;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    thickness = sectionType.WallNominalThickness * feetToMetre;
                    outerRadius = sectionType.OuterFillet * feetToMetre;
                    innerRadius = sectionType.InnerFillet * feetToMetre;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    thickness = familySymbol.LookupParameterDouble(wallThicknessNames, true);
                    outerRadius = familySymbol.LookupParameterDouble(outerRadiusNames, true);
                    innerRadius = familySymbol.LookupParameterDouble(innerRadiusNames, true);
                }

                if (double.IsNaN(outerRadius)) outerRadius = 0;
                if (double.IsNaN(innerRadius)) innerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(thickness) && !double.IsNaN(outerRadius) && !double.IsNaN(innerRadius))
                {
                    return new StandardBoxDimensions(height, width, thickness, outerRadius, innerRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardChannelSectionDimensions)))
            {
                double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionCParallelFlange)
                {
                    StructuralSectionCParallelFlange sectionType = section as StructuralSectionCParallelFlange;
                    height = sectionType.Height * feetToMetre;
                    flangeWidth = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionCProfile)
                {
                    //TODO: Implement cold-formed profiles?
                    StructuralSectionCProfile sectionType = section as StructuralSectionCProfile;
                    height = sectionType.Height * feetToMetre;
                    flangeWidth = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WallNominalThickness * feetToMetre;
                    flangeThickness = sectionType.WallNominalThickness * feetToMetre;
                    rootRadius = sectionType.InnerFillet * feetToMetre;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    flangeWidth = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && double.IsNaN(toeRadius))
                {
                    return new StandardChannelSectionDimensions(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardISectionDimensions)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionIParallelFlange)
                {
                    StructuralSectionIParallelFlange sectionType = section as StructuralSectionIParallelFlange;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionIWideFlange)
                {
                    StructuralSectionIWideFlange sectionType = section as StructuralSectionIWideFlange;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return new StandardISectionDimensions(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardTeeSectionDimensions)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;

                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionISplitParallelFlange)
                {
                    StructuralSectionISplitParallelFlange sectionType = section as StructuralSectionISplitParallelFlange;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionStructuralTees)
                {
                    StructuralSectionStructuralTees sectionType = section as StructuralSectionStructuralTees;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionConcreteT)
                {
                    StructuralSectionConcreteT sectionType = section as StructuralSectionConcreteT;
                    height = sectionType.Height * feetToMetre;
                    width = (sectionType.Width + 2 * sectionType.CantileverLength) * feetToMetre;
                    webThickness = sectionType.Width * feetToMetre;
                    flangeThickness = sectionType.CantileverHeight * feetToMetre;
                    rootRadius = 0;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return new StandardTeeSectionDimensions(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(StandardZedSectionDimensions)))
            {
                double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionZProfile)
                {
                    StructuralSectionZProfile sectionType = section as StructuralSectionZProfile;
                    height = sectionType.Height * feetToMetre;
                    flangeWidth = sectionType.BottomFlangeLength * feetToMetre;
                    webThickness = sectionType.WallNominalThickness * feetToMetre;
                    flangeThickness = sectionType.WallNominalThickness * feetToMetre;
                    rootRadius = sectionType.InnerFillet * feetToMetre;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    flangeWidth = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return new StandardZedSectionDimensions(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(TubeDimensions)))
            {
                double thickness, diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionPipeStandard)
                {
                    StructuralSectionPipeStandard sectionType = section as StructuralSectionPipeStandard;
                    thickness = sectionType.WallNominalThickness * feetToMetre;
                    diameter = sectionType.Diameter * feetToMetre;
                }
                else if (section is StructuralSectionRoundHSS)
                {
                    StructuralSectionRoundHSS sectionType = section as StructuralSectionRoundHSS;
                    thickness = sectionType.WallNominalThickness * feetToMetre;
                    diameter = sectionType.Diameter * feetToMetre;
                }
                else
                {
                    thickness = familySymbol.LookupParameterDouble(wallThicknessNames, true);
                    diameter = familySymbol.LookupParameterDouble(diameterNames, true);
                }
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

        public static string[] diameterNames = { "BHE_Diameter", "Diameter", "d", "D", "OD" };
        public static string[] radiusNames = { "BHE_Radius", "Radius", "r", "R" };
        public static string[] heightNames = { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" };
        public static string[] widthNames = { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" };
        public static string[] cornerRadiusNames = { "Corner Radius", "r", "r1" };
        public static string[] topFlangeWidthNames = { "Top Flange Width", "bt", "bf_t", "bft", "b1", "b", "B", "Bt" };
        public static string[] botFlangeWidthNames = { "Bottom Flange Width", "bb", "bf_b", "bfb", "b2", "b", "B", "Bb" };
        public static string[] webThicknessNames = { "Web Thickness", "Stem Width", "tw", "t", "T" };
        public static string[] topFlangeThicknessNames = { "Top Flange Thickness", "tft", "tf_t", "tf", "T", "t" };
        public static string[] botFlangeThicknessNames = { "Bottom Flange Thickness", "tfb", "tf_b", "tf", "T", "t" };
        public static string[] flangeThicknessNames = { "Flange Thickness", "Slab Depth", "tf", "T", "t" };
        public static string[] weldSizeNames1 = { "Weld Size" };                                            // weld size, diagonal
        public static string[] weldSizeNames2 = { "k" };                                                    // weld size counted from bar's vertical axis
        public static string[] rootRadiusNames = { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" };
        public static string[] toeRadiusNames = { "Flange Fillet", "Toe Radius", "r2", "R2", "t" };
        public static string[] innerRadiusNames = { "Inner Fillet", "Inner Radius", "r1", "R1", "ri", "t" };
        public static string[] outerRadiusNames = { "Outer Fillet", "Outer Radius", "r2", "R2", "ro", "tr" };
        public static string[] wallThicknessNames = { "Wall Nominal Thickness", "Wall Thickness", "t", "T" };

        //TODO: Improve and move to Geometry_Engine
        private static List<oM.Geometry.Line> Discretize(this List<oM.Geometry.ICurve> curves, int segments)
        {
            List<oM.Geometry.Line> result = new List<oM.Geometry.Line>();
            List<double> parameters = new List<double>();
            double step = 1 / segments;
            for (int i = 0; i < segments + 1; i++)
            {
                parameters.Add(step * i);
            }
            foreach (oM.Geometry.ICurve curve in curves)
            {
                if (curve is oM.Geometry.Line) result.Add(curve as oM.Geometry.Line);
                else
                {
                    List<oM.Geometry.Point> points = parameters.Select(p => BH.Engine.Geometry.Query.IPointAtLength(curve, p)).ToList();
                    for (int i = 0; i < segments; i++)
                    {
                        result.Add(new oM.Geometry.Line { Start = points[i], End = points[i + 1] });
                    }
                }
            }
            return result;
        }

        private static double feetToMetre = UnitUtils.ConvertFromInternalUnits(1, DisplayUnitType.DUT_METERS);
        private static oM.Geometry.Point origin = new oM.Geometry.Point { X = 0, Y = 0, Z = 0 };
        private static oM.Geometry.Vector feetToMetreVector = new oM.Geometry.Vector { X = feetToMetre, Y = feetToMetre, Z = feetToMetre };
    }
}
 