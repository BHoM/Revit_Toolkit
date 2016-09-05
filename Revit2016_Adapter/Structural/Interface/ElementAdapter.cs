using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHoM.Structural.Loads;
using BHoM.Structural;
using Autodesk.Revit.DB;
using BHoMB = BHoM.Base;
using BHoME = BHoM.Structural.Elements;

using Revit2016_Adapter.Structural.Elements;
using Revit2016_Adapter.Geometry;
using BHoM.Structural.Interface;

namespace Revit2016_Adapter.Structural
{
    public partial class RevitAdapter : BHoM.Structural.Interface.IElementAdapter
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

        public ObjectSelection Selection
        {
            get; set;
        }

        public List<string> GetNodes(out List<BHoME.Node> nodes, List<string> ids = null)
        {
            throw new NotImplementedException();
        }

        public List<string> GetBars(out List<BHoME.Bar> bars, List<string> ids = null)
        {
            bars = new List<BHoME.Bar>();

            List<BHoME.Bar> barList = null;
            BarIO.GetBeams(out barList, m_Revit, ids, m_Rounding);
            bars.AddRange(barList);
            BarIO.GetColumns(out barList, m_Revit, ids, m_Rounding);
            bars.AddRange(barList);
            BarIO.GetPiles(out barList, m_Revit, ids, m_Rounding);
            bars.AddRange(barList);
            List<string> outids = new List<string>();
            for (int i = 0; i < bars.Count; i++)
            {
                outids.Add(bars[i][Base.RevitUtils.REVIT_ID_KEY].ToString());
            }
            return outids;
        }

        public List<string> GetPanels(out List<BHoME.Panel> panels, List<string> ids = null)
        {
            panels = new List<BHoME.Panel>();

            List<BHoME.Panel> barList = null;
            PanelIO.GetSlabs(out barList, m_Revit, ids);
            panels.AddRange(barList);
            PanelIO.GetWalls(out barList, m_Revit, ids);
            panels.AddRange(barList);
            PanelIO.GetFoundations(out barList, m_Revit, ids);
            panels.AddRange(barList);
            return ids;
        }

        public List<string> GetOpenings(out List<BHoME.Opening> opening, List<string> ids = null)
        {
            throw new NotImplementedException();
        }

        public List<string> GetLevels(out List<BHoME.Storey> levels, List<string> ids = null)
        {
            BHoMB.ObjectManager<BHoME.Storey> stories = new BHoMB.ObjectManager<BHoME.Storey>();
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
                BHoME.Storey storey = new BHoME.Storey();
                storey.Elevation = Math.Round(revitLevels[i].ProjectElevation * GeometryUtils.FeetToMetre, m_Rounding);
                storey.Height = Math.Round(height, m_Rounding);
                storey.Name = revitLevels[i].Name;
                stories.Add(revitLevels[i].Name, storey);
            }
            levels = stories.ToList();
            return ids;
        }

        public List<string> GetGrids(out List<BHoME.Grid> grids, List<string> ids = null)
        {
            BHoMB.ObjectManager<BHoME.Grid> bhGrids = new BHoMB.ObjectManager<BHoME.Grid>();
            List<Autodesk.Revit.DB.Grid> revitGrids = new FilteredElementCollector(m_Revit).OfClass(typeof(Autodesk.Revit.DB.Grid)).Cast<Autodesk.Revit.DB.Grid>().ToList();

            foreach (Autodesk.Revit.DB.Grid grid in revitGrids)
            {
                Curve location = grid.Curve;
                if (location is Line)
                {
                    Line c = location as Line;
                    bhGrids.Add(grid.Name, new BHoME.Grid(grid.Name, GeometryUtils.Convert(c.Origin, m_Rounding), GeometryUtils.ConvertVector(c.Direction)));
                }
            }
            grids = bhGrids.ToList();
            return ids;
        }

        List<string> IElementAdapter.GetLoadcases(out List<ICase> cases)
        {
            throw new NotImplementedException();
        }

        public bool GetLoads(out List<ILoad> loads, List<string> ids = null)
        {
            throw new NotImplementedException();
        }

        public bool SetNodes(List<BHoME.Node> nodes, out List<string> ids)
        {
            throw new NotImplementedException();
        }

        public bool SetBars(List<BHoME.Bar> bars, out List<string> ids)
        {
            throw new NotImplementedException();
        }

        public bool SetPanels(List<BHoME.Panel> panels, out List<string> ids)
        {
            throw new NotImplementedException();
        }

        public bool SetOpenings(List<BHoME.Opening> opening, out List<string> ids)
        {
            throw new NotImplementedException();
        }

        public bool SetLevels(List<BHoME.Storey> stores, out List<string> ids)
        {
            throw new NotImplementedException();
        }

        public bool SetGrids(List<BHoME.Grid> grid, out List<string> ids)
        {
            throw new NotImplementedException();
        }

        public bool SetLoads(List<ILoad> loads)
        {
            throw new NotImplementedException();
        }

        public bool SetLoadcases(List<ICase> cases)
        {
            throw new NotImplementedException();
        }
    }
}
