using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHoM.Structural.Loads;
using BHoM.Structural;
using Autodesk.Revit.DB;
using BH = BHoM.Structural;
using BHoM.Global;

namespace RevitToolkit2015
{
    public class RevitAdapter : IStructuralAdapter
    {
        private Document m_Revit;
        private int m_Rounding = 9;

        public RevitAdapter(Document document, int rounding)
        {
            m_Revit = document;
            m_Rounding = rounding;
        }

        public string Filename
        {
            get
            {
                return m_Revit.PathName;
            }
        }

        public bool GetBars(out List<Bar> bars, string option = "")
        {
            bars = new List<Bar>();

            List<Bar> barList = null;
            BarIO.GetBeams(out barList, m_Revit, m_Rounding);
            bars.AddRange(barList);
            BarIO.GetColumns(out barList, m_Revit, m_Rounding);
            bars.AddRange(barList);
            return true;
        }

        public bool GetLevels(out List<Storey> levels, string options = "")
        {
            ObjectManager<Storey> stories = new ObjectManager<Storey>();
            List<Level> revitLevels = new FilteredElementCollector(m_Revit).OfClass(typeof(Level)).Cast<Level>().ToList();

            revitLevels.Sort(delegate (Level l1, Level l2)
            {
                return l1.ProjectElevation.CompareTo(l2.Elevation);
            });

            double height = 0;
            for (int i = 0; i < revitLevels.Count; i++)
            {
                if (i < revitLevels.Count - 1)
                {
                    height = (revitLevels[i + 1].Elevation - revitLevels[i].Elevation) * GeometryUtils.FeetToMetre;
                }
                BH.Storey storey = new BH.Storey();
                storey.Elevation = Math.Round(revitLevels[i].ProjectElevation * GeometryUtils.FeetToMetre, m_Rounding);
                storey.Height = Math.Round(height, m_Rounding);
                storey.Name = revitLevels[i].Name;
                stories.Add(revitLevels[i].Name, storey);
            }
            levels = stories.ToList();
            return true;
        }

        public bool GetGrids(out List<BH.Grid> grids, string options = "")
        {
            ObjectManager<BH.Grid> bhGrids = new ObjectManager<BH.Grid>();
            List<Autodesk.Revit.DB.Grid> revitGrids = new FilteredElementCollector(m_Revit).OfClass(typeof(Autodesk.Revit.DB.Grid)).Cast<Autodesk.Revit.DB.Grid>().ToList();

            foreach (Autodesk.Revit.DB.Grid grid in revitGrids)
            {
                Curve location = grid.Curve;
                if (location is Line)
                {
                    Line c = location as Line;
                    bhGrids.Add(grid.Name, new BHoM.Structural.Grid(grid.Name, GeometryUtils.Convert(c.Origin, m_Rounding), GeometryUtils.ConvertVector(c.Direction)));
                }
            }
            grids = bhGrids.ToList();
            return true;
        }

        public bool GetLoadcases(out List<ICase> cases)
        {
            throw new NotImplementedException();
        }

        public bool GetLoads(out List<ILoad> loads, string option = "")
        {
            throw new NotImplementedException();
        }

        public bool GetNodes(out List<Node> nodes, string option = "")
        {
            throw new NotImplementedException();
        }

        public bool GetOpenings(out List<BH.Opening> opening, string option = "")
        {
            throw new NotImplementedException();
        }

        public bool GetPanels(out List<BH.Panel> panels, string option = "")
        {
            panels = new List<BH.Panel>();

            List<BH.Panel> barList = null;
            PanelIO.GetSlabs(out barList, m_Revit, m_Rounding);
            panels.AddRange(barList);
            PanelIO.GetWalls(out barList, m_Revit, m_Rounding);
            panels.AddRange(barList);
            return true;
        }

        public bool SetBars(List<Bar> bars, out List<string> ids, string option = "")
        {
            throw new NotImplementedException();
        }

        public bool SetLevels(List<Storey> stores, out List<string> ids, string option = "")
        {
            throw new NotImplementedException();
        }

        public bool SetLoadcases(List<ICase> cases)
        {
            throw new NotImplementedException();
        }

        public bool SetLoads(List<ILoad> loads, string option = "")
        {
            throw new NotImplementedException();
        }

        public bool SetNodes(List<Node> nodes, out List<string> ids, string option = "")
        {
            throw new NotImplementedException();
        }

        public bool SetOpenings(List<BH.Opening> opening, out List<string> ids, string option = "")
        {
            throw new NotImplementedException();
        }

        public bool SetPanels(List<BH.Panel> panels, out List<string> ids, string option = "")
        {
            throw new NotImplementedException();
        }

        public bool SetGrids(List<BH.Grid> grid, out List<string> ids, string option = "")
        {
            throw new NotImplementedException();
        }
    }
}
