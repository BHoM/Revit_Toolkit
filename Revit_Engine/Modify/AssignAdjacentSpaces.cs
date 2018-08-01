using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Modify Methods
    /// </summary>
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Assigns Adjacent Spaces
        /// </summary>
        /// <param name="building">BHoM Building</param>
        /// <param name="removeDuplicates">Remove duplicated Building Elements</param>
        /// <returns name="Building">BHoM Building</returns>
        /// <search>
        /// Modify, BHoM, AssignAdjacentSpaces, Assign Adjacent Spaces, building
        /// </search>
        public static Building AssignAdjacentSpaces(this Building building, bool removeDuplicates = true)
        {
            if (building == null)
                return null;

            Building aBuilding = building.GetShallowClone() as Building;

            
            Dictionary<int, List<BuildingElement>> aData = new Dictionary<int, List<BuildingElement>>();
            foreach (BuildingElement aBuildingElement in aBuilding.BuildingElements)
            {
                object aObject;
                if(aBuildingElement.CustomData.TryGetValue(Convert.ElementId, out aObject))
                {
                    if(aObject is int)
                    {
                        int aId = (int)aObject;
                        if (!aData.ContainsKey(aId))
                            aData.Add(aId, building.BuildingElements(aId));
                    }
                }
            }

            List<BuildingElement> aResult = new List<BuildingElement>();
            if (aData != null && aData.Count > 0)
            {
                foreach(KeyValuePair<int, List<BuildingElement>> aKeyValuePair in aData)
                {
                    List< BuildingElement > aBuildingElementList = AssignAdjacentSpaces(aKeyValuePair.Value, removeDuplicates);
                    if (aKeyValuePair.Value != null)
                        aResult.AddRange(aKeyValuePair.Value);
                }
            }

            aBuilding.BuildingElements = aResult;

            return aBuilding;

        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<BuildingElement> AssignAdjacentSpaces(List<BuildingElement> builidingElements, bool removeDuplicates = true)
        {
            List<Tuple<oM.Geometry.Point, BuildingElement>> aTupleList = new List<Tuple<oM.Geometry.Point, BuildingElement>>();
            foreach (BuildingElement aBuildingElement in builidingElements)
            {
                ICurve aICurve = Environment.Query.ICurve(aBuildingElement.BuildingElementGeometry);
                BoundingBox aBoundingBox = Geometry.Query.IBounds(aICurve);
                oM.Geometry.Point aPoint = Geometry.Query.Centre(aBoundingBox);
                aTupleList.Add(new Tuple<oM.Geometry.Point, BuildingElement>(aPoint, aBuildingElement));
            }

            List<BuildingElement> aResult = new List<BuildingElement>();
            for (int i =0; i < aTupleList.Count; i++)
            {
                double aDistance = double.MaxValue;
                BuildingElement aBuildingElement = null;
                for (int j = i + 1; j < aTupleList.Count; j++)
                {
                    double aDistance_Temp = Geometry.Query.Distance(aTupleList[i].Item1, aTupleList[j].Item1);
                    if (aDistance_Temp < aDistance && aDistance_Temp <= Tolerance.MicroDistance)
                    {
                        aDistance = aDistance_Temp;
                        aBuildingElement = aTupleList[i].Item2;
                    }
                }

                if(aBuildingElement != null)
                {
                    aBuildingElement = aBuildingElement.GetShallowClone() as BuildingElement;
                    foreach(Guid aGuid in aTupleList[i].Item2.AdjacentSpaces)
                    {
                        Guid aGuid_Temp = aBuildingElement.AdjacentSpaces.Find(x => x == aGuid);
                        if (aGuid_Temp != null && aGuid_Temp != Guid.Empty)
                            aBuildingElement.AdjacentSpaces.Add(aGuid_Temp);
                    }
                    aResult.Add(aBuildingElement);

                    if(!removeDuplicates)
                    {
                        BuildingElement aBuildingElement_Temp = aTupleList[i].Item2.GetShallowClone() as BuildingElement;
                        foreach (Guid aGuid in aBuildingElement.AdjacentSpaces)
                        {
                            Guid aGuid_Temp = aBuildingElement_Temp.AdjacentSpaces.Find(x => x == aGuid);
                            if (aGuid_Temp != null && aGuid_Temp != Guid.Empty)
                                aBuildingElement_Temp.AdjacentSpaces.Add(aGuid_Temp);
                        }
                        aResult.Add(aBuildingElement_Temp);
                    }

                }
                else
                {
                    aBuildingElement = aTupleList[i].Item2.GetShallowClone() as BuildingElement;
                    aResult.Add(aBuildingElement);
                }
            }
            return aResult;
        }

        /***************************************************/
    }
}

