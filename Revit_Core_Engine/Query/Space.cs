using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the Revit Space that contains the given element.")]
        [Input("element", "The Revit element for which to find the containing Space.")]
        [Input("spaces", "An optional collection of Revit Spaces to search. If not provided, all Spaces in the element's document will be used.")]
        [Input("useRoomCalculationPoint", "If true and the element is a FamilyInstance with a spatial element calculation point, that point will be used for containment checks.")]
        [Output("space", "The Revit Space containing the element, or the element itself if it is a Space. Returns null if no containing Space is found.")]
        public static Space Space(this Element element, IEnumerable<Space> spaces = null, bool useRoomCalculationPoint = false)
        {
            if (element == null)
                return null;

            // 1. If the element is a Space, return itself
            if (element is Space space)
                return space;

            // 2. If the element is a FamilyInstance, try the .Space property
            if (element is FamilyInstance fi && fi.Space != null)
                return fi.Space;

            // 3. Use location point and check which space contains it
            if (spaces == null)
            {
                Document doc = element.Document;
                spaces = new FilteredElementCollector(doc)
                    .OfClass(typeof(SpatialElement))
                    .OfType<Space>()
                    .ToList();
            }

            foreach (var sp in spaces)
            {
                if (element.IsInSpace(sp, useRoomCalculationPoint))
                    return sp;
            }

            // 4. Not found
            return null;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static bool IsInSpace(this Element element, Space space, bool useRoomCalculationPoint)
        {
            if (element is FamilyInstance && element.Document.Equals(space.Document))
                return (element as FamilyInstance).Space?.Id == space.Id;

            Transform elementTransform = element.Document.IsLinked ? element.Document.LinkInstance().GetTotalTransform() : Transform.Identity;
            Transform spaceTransform = space.Document.IsLinked ? space.Document.LinkInstance().GetTotalTransform() : Transform.Identity;

            XYZ locationPoint = element.LocationPoint(useRoomCalculationPoint);

            if (!elementTransform.IsIdentity)
                locationPoint = elementTransform.OfPoint(locationPoint);
            if (!spaceTransform.IsIdentity)
                locationPoint = spaceTransform.Inverse.OfPoint(locationPoint);

            return space.IsPointInSpace(locationPoint);
        }

        /***************************************************/
    }
}
