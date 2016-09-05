using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHoM.Structural.Elements;
using BHoM.Structural.Properties;
using BHoM.Geometry;
using Revit2016_Adapter.Structural.Elements;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Group<Curve> crvs = new Group<Curve>();
            crvs.Add(new Line(new Point(0, 0, 0), new Point(0.5, 0, 0)));
            crvs.Add(new Line(new Point(0.5, 0, 0), new Point(0.5, 4, 0)));
            crvs.Add(new Line(new Point(0.5, 4, 0), new Point(0, 4, 0)));
            crvs.Add(new Line(new Point(0, 4, 0), new Point(0, 0, 0)));


            Bar b = new Bar(new BHoM.Geometry.Point(0, 0, 0), new BHoM.Geometry.Point(0, 0, 5));
            SectionProperty p = new SectionProperty(crvs, ShapeType.Rectangle, SectionType.ConcreteColumn);

            b.SectionProperty = p;
            b.OrientationAngle = 60;
            BarIO.BHomColumnsToBHoMPanels(new List<Bar>() { b });
        }
    }
}
