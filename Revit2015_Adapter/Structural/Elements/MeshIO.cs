using Autodesk.Revit.DB;
using Revit2015_Adapter.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit2015_Adapter.Structural.Elements
{
    public class MeshIO
    {
        //public static bool SetMesh(Document revit, List<BHoM.Structural.Elements.FEMesh> meshes, out List<string> ids)
        //{
        //    ids = new List<string>();
        //    for (int i = 0; i < meshes.Count; i++)
        //    {
        //        BHoM.Geometry.Mesh mesh = meshes[i].GetGeometry() as BHoM.Geometry.Mesh;
        //        mesh.RemoveDuplicateVertices(0.01);
        //        Transaction t = new Transaction(revit, "Set mesh");
        //        t.Start();

        //        DirectShape ds = DirectShape.CreateElement(revit, new ElementId(BuiltInCategory.OST_GenericModel), "Application id", "Geometry object id");

        //        ds.SetShape(GeometryUtils.Convert(mesh, Base.RevitUtils.GetMaterial(revit, meshes[i].PanelProperty.Material.Type).Id));
                
        //        ids.Add(ds.Id.ToString());
        //        t.Commit();
        //    }
        //    return true;
        //}

    }
}
