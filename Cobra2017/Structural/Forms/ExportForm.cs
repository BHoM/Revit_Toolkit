using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using BHoM.Structural;
using BHoM.Structural.Elements;
using AD = Autodesk.Revit.DB;
using Revit2017_Adapter;
using Revit2017_Adapter.Geometry;
using Revit2017_Adapter.Structural;
using BHoM.Structural.Interface;

namespace Cobra2016.Structural.Forms
{
    public partial class ExportForm : System.Windows.Forms.Form
    {
        private string m_Filename;
        private Autodesk.Revit.DB.Document m_Document;
        private double m_SnapGridTolerance;
        private double m_SnapLevelTolerance;
        private double m_SnapWallTolerance;
        private double m_MergeTolerance;
        private int m_Rounding;
        private bool m_UnselectedElements;
        public ExportForm(Autodesk.Revit.DB.Document document)
        {
            InitializeComponent();
            m_Document = document;
            m_Filename = Path.Combine(Path.GetTempPath(), "RevitExchange");
            checkRound.Checked = true;
            checkBox1.Checked = true;
            textLevelTolerance.Text = "500";
            textGridTolerance.Text = "300";
            textWallTolerance.Text = "300";
            comboExportOption.SelectedIndex = 0;
            LoadRevitElements();
            LoadRevitLevels();
            LoadRevitGrids();
        }

        private void LoadRevitElements()
        {
            List<AD.FamilyInstance> beams = new AD.FilteredElementCollector(m_Document).OfClass(typeof(AD.FamilyInstance)).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming).Cast<AD.FamilyInstance>().ToList();
            List<AD.FamilyInstance> columns = new AD.FilteredElementCollector(m_Document).OfClass(typeof(AD.FamilyInstance)).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns).Cast<AD.FamilyInstance>().ToList();
            List<AD.FamilyInstance> foundations = new AD.FilteredElementCollector(m_Document).OfClass(typeof(AD.FamilyInstance)).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFoundation).Cast<AD.FamilyInstance>().ToList();
            List<AD.Floor> floors = new AD.FilteredElementCollector(m_Document).OfClass(typeof(AD.Floor)).Cast<AD.Floor>().ToList();
            List<AD.Wall> walls = new AD.FilteredElementCollector(m_Document).OfClass(typeof(AD.Wall)).Cast<AD.Wall>().ToList();

            treeViewSelection.Nodes.Add("Beams", "Beams");
            treeViewSelection.Nodes.Add("Columns", "Columns");
            treeViewSelection.Nodes.Add("Foundations", "Foundations");
            treeViewSelection.Nodes.Add("Floors", "Floors");
            treeViewSelection.Nodes.Add("Walls", "Walls");
            foreach (AD.FamilyInstance beam in beams)
            {
                AddChild(beam.Id.ToString(), beam.Symbol.Name, treeViewSelection.Nodes["Beams"]);
            }

            foreach (AD.FamilyInstance column in columns)
            {
                AddChild(column.Id.ToString(), column.Symbol.Name, treeViewSelection.Nodes["Columns"]);
            }

            foreach (AD.FamilyInstance foundation in foundations)
            {
                AddChild(foundation.Id.ToString(), foundation.Symbol.Name, treeViewSelection.Nodes["Foundations"]);
            }

            foreach (AD.Floor floor in floors)
            {
                AddChild(floor.Id.ToString(), floor.FloorType.Name, treeViewSelection.Nodes["Floors"]);
            }

            foreach (AD.Wall wall in walls)
            {
                AddChild(wall.Id.ToString(), wall.WallType.Name, treeViewSelection.Nodes["Walls"]);
            }

            treeViewSelection.CollapseAll();

            foreach (TreeNode node in treeViewSelection.Nodes)
            {
                node.Checked = true;
            }

            treeViewSelection.Sort();
        }

        private void AddChild(string data, string parent, TreeNode node)
        {
            if (!node.Nodes.ContainsKey(parent))
            {
                node.Nodes.Add(parent, parent);
            }
            node.Nodes[parent].Nodes.Add(data);
        }

        private void LoadRevitLevels()
        {
            ICollection<AD.Level> levels = new AD.FilteredElementCollector(m_Document).OfClass(typeof(AD.Level)).Cast<AD.Level>().ToList();
            foreach (AD.Level level in levels)
            {
                ListViewItem item = new ListViewItem(new string[] { level.Name, Math.Round(level.ProjectElevation * GeometryUtils.FeetToMetre, 3).ToString() });
                item.Name = level.Id.ToString();
                listViewLevels.Items.Add(item);
            }
        }

        private void LoadRevitGrids()
        {
            ICollection<AD.Grid> grids = new AD.FilteredElementCollector(m_Document).OfClass(typeof(AD.Grid)).Cast<AD.Grid > ().ToList();
            foreach (AD.Grid grid in grids)
            {
                AD.Curve location = grid.Curve;
                if (location is AD.Line)
                {
                    AD.Line c = location as AD.Line;
                    ListViewItem item = new ListViewItem(new string[] { grid.Name, ToString(c.Direction), GeometryUtils.PointLocation(GeometryUtils.Convert(c.Origin), 3) });
                    item.Name = grid.Id.ToString();
                    listViewGrids.Items.Add(item);
                }
            }
        }

        private string ToString(AD.XYZ xyz)
        {
            return Math.Round(xyz.X, 3) + "," + Math.Round(xyz.Y, 3) + "," + Math.Round(xyz.Z, 3);
        }

        private void comboExportOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            string date = System.DateTime.Now.Year.ToString().Substring(2) + System.DateTime.Now.Month + System.DateTime.Now.Day;
            textFilename.Enabled = true;
            buttonBrowse.Enabled = true;

            if (comboExportOption.SelectedItem.ToString() == "File")
            {
                m_Filename = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), date + " - RevitExport.txt");
                saveFileDialog1.DefaultExt = "txt"; 

            }
            else if (comboExportOption.SelectedItem.ToString() == "Robot")
            {
                m_Filename = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), date + " - RevitExport.rtd");
                saveFileDialog1.DefaultExt = "rtd";
            }
            else if (comboExportOption.SelectedItem.ToString() == "Etabs")
            {
                m_Filename = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), date + " - RevitExport.edb");
                saveFileDialog1.DefaultExt = "edb";
            }
            else
            {
                m_Filename = Path.Combine(Path.GetTempPath(), "RevitExchange");
                textFilename.Enabled = false;
                buttonBrowse.Enabled = false;
            }
            textFilename.Text = m_Filename;
        }

        private List<int> GetUnselectedIds()
        {
            List<int> ids = new List<int>();
            foreach (TreeNode node in treeViewSelection.Nodes)
            {
                ids.AddRange(GetUnselectedIds(node));
            }
            return ids;
        }

        private List<int> GetUnselectedIds(TreeNode node)
        {
            List<int> ids = new List<int>();       
            int id = 0;
            if (!node.Checked && int.TryParse(node.Text, out id))
            {              
                ids.Add(id);                
            }
            else
            {
                foreach (TreeNode n in node.Nodes)
                    ids.AddRange(GetUnselectedIds(n));
            }
            return ids;
        }

        private List<string> GetCheckedIds(ListView listView)
        {
            List<string> ids = new List<string>();
            foreach (ListViewItem item in listView.Items)
            {
                if (item.Checked == true)
                {
                    ids.Add(item.Name);
                }
            }
            return ids;
        }

        private List<string> GetCheckedIds(TreeNode node)
        {
            List<string> ids = new List<string>();
            foreach (TreeNode n in node.Nodes)
            {
                if (n.Checked)
                {
                    ids.Add(n.Text);
                }
            }
            return ids;
        }
        private List<string> GetSelectedBars()
        {
            List<string> ids = new List<string>();
            foreach (TreeNode n in treeViewSelection.Nodes["Beams"].Nodes)
            {
                ids.AddRange(GetCheckedIds(n));
            }
            foreach (TreeNode n in treeViewSelection.Nodes["Columns"].Nodes)
            {
                ids.AddRange(GetCheckedIds(n));
            }
            foreach (TreeNode n in treeViewSelection.Nodes["Foundations"].Nodes)
            {
                ids.AddRange(GetCheckedIds(n));
            }
            return ids;
        }

        private List<string> GetSelectedPanels()
        {
            List<string> ids = new List<string>();
            foreach (TreeNode n in treeViewSelection.Nodes["Floors"].Nodes)
            {
                ids.AddRange(GetCheckedIds(n));
            }
            foreach (TreeNode n in treeViewSelection.Nodes["Walls"].Nodes)
            {
                ids.AddRange(GetCheckedIds(n));
            }
            foreach (TreeNode n in treeViewSelection.Nodes["Foundations"].Nodes)
            {
                ids.AddRange(GetCheckedIds(n));
            }

            return ids;
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            List<Bar> bars;
            List<BHoM.Structural.Elements.Panel> panels;
            List<Storey> levels;
            List<Grid> grids;
            RevitAdapter revit = new RevitAdapter(m_Document, m_Rounding);           

            revit.GetLevels(out levels, GetCheckedIds(listViewLevels));
            revit.GetGrids(out grids, GetCheckedIds(listViewGrids));
            revit.GetBars(out bars, GetSelectedBars());
            revit.GetPanels(out panels, GetSelectedPanels());

            if (checkLevel.Checked)
            {
                SnapToLevels(m_SnapLevelTolerance);
            }

            if (checkGrid.Checked)
            {
                SnapToGrids(m_SnapGridTolerance);
            }

            if (checkWalls.Checked)
            {
                SnapToPlanarWalls(m_SnapWallTolerance);
            }
            if (checkBox1.Checked)
            {
                MergeWithin(m_MergeTolerance);
            }

            //string projectData = BHoM.Base.Project.ActiveProject.ToJSON();

            switch (comboExportOption.Text)
            {
                case "File":
                case "Rhino":
                    Export(new BHoM.Structural.FileIO(Path.Combine(m_Filename,"In"), Path.Combine(m_Filename, "Out")));
                    //using (StreamWriter fs = new StreamWriter(m_Filename))
                    //{
                    //    string json = BHoM.Global.Project.ActiveProject.ToJSON();
                    //    fs.WriteLine(json);
                    //    fs.Close();
                    //}
                    break;
                case "Etabs":
                    Export(new Etabs_Adapter.Structural.Interface.EtabsAdapter(m_Filename));
                    break;
                case "Robot":
                    Export(new Robot_Adapter.Structural.Interface.RobotAdapter());
                    break;
            }

            BHoM.Global.Project.ActiveProject.Clear();

            this.DialogResult = DialogResult.OK;
            Close();        
        }

        private void Export(IElementAdapter adapter)
        {
            List<string> ids = null;
            List<Node> nodes = new BHoM.Base.ObjectFilter<Node>().ToList();
            List<Bar> bars = new BHoM.Base.ObjectFilter<Bar>().ToList();
            List<BHoM.Structural.Elements.Panel> panels = new BHoM.Base.ObjectFilter<BHoM.Structural.Elements.Panel>().ToList();
            List<Opening> openings = new BHoM.Base.ObjectFilter<Opening>().ToList();
            List<Storey> levels = new BHoM.Base.ObjectFilter<Storey>().ToList();
            List<Grid> grids = new BHoM.Base.ObjectFilter<Grid>().ToList();

            adapter.SetLevels(levels, out ids);
            adapter.SetGrids(grids, out ids);
            adapter.SetBars(bars, out ids);
            adapter.SetPanels(panels, out ids);
            adapter.SetOpenings(openings, out ids);
        }

        private void MergeWithin(double tolerance)
        {
            List<Node> nodes = new BHoM.Base.ObjectFilter<Node>().ToList();

            nodes.Sort(delegate (Node n1, Node n2)
            {
                return n1.Point.DistanceTo(BHoM.Geometry.Point.Origin).CompareTo(n2.Point.DistanceTo(BHoM.Geometry.Point.Origin));
            });
                
            for (int i = 0; i < nodes.Count;i++)
            {
                double distance = nodes[i].Point.DistanceTo(BHoM.Geometry.Point.Origin);
                int j = i + 1;
                while (j < nodes.Count && Math.Abs(nodes[j].Point.DistanceTo(BHoM.Geometry.Point.Origin) - distance) < tolerance)
                {
                    if (nodes[i].Point.DistanceTo(nodes[j].Point) < tolerance)
                    {
                        nodes[j] = nodes[j].Merge(nodes[i]);
                        BHoM.Global.Project.ActiveProject.RemoveObject(nodes[i].BHoM_Guid);
                        break;
                    }
                    j++;
                }
            }           
        }

        private void SnapToLevels(double tolerance)
        {
            List<Storey> levels = new BHoM.Base.ObjectFilter<Storey>().ToList();
            if (levels != null)
            {
                levels.Sort(delegate (Storey s1, Storey s2)
                {
                    return s1.Elevation.CompareTo(s2.Elevation);
                });

                List<BHoM.Geometry.Plane> levelPlanes = new List<BHoM.Geometry.Plane>();
                for (int i = 0; i < levels.Count; i++)
                {
                    levelPlanes.Add(levels[i].Plane);
                }
                SnapToPlanes(levelPlanes, tolerance);
            }
        }

        private void SnapToGrids(double tolerance)
        {
            List<Grid> levels = new BHoM.Base.ObjectFilter<Grid>().ToList();
            if (levels != null)
            {                
                List<BHoM.Geometry.Plane> gridPlanes = new List<BHoM.Geometry.Plane>();
                for (int i = 0; i < levels.Count; i++)
                {
                    gridPlanes.Add(levels[i].Plane);
                }
                SnapToPlanes(gridPlanes, tolerance);
            }
        }

        private void SnapToPlanarWalls(double tolerance)
        {
            List<BHoM.Geometry.Plane> wallPlanes = new List<BHoM.Geometry.Plane>();
            List<BHoM.Geometry.BoundingBox> bounds = new List<BHoM.Geometry.BoundingBox>();
            Dictionary<string, BHoM.Geometry.BoundingBox> addedPlanes = new Dictionary<string, BHoM.Geometry.BoundingBox>();
            foreach (BHoM.Structural.Elements.Panel p in new BHoM.Base.ObjectFilter<BHoM.Structural.Elements.Panel>())
            {
                try
                {
                    BHoM.Geometry.Curve c = p.External_Contours[0];
                    BHoM.Geometry.Plane plane = null;
                    if (c.TryGetPlane(out plane) && GeometryUtils.IsVertical(plane))
                    {
                        string planeKey = GeometryUtils.PointLocation(plane.Normal, 3) + "," + Math.Round(plane.D, 3);
                        BHoM.Geometry.BoundingBox tempBounds = null;
                        if (!addedPlanes.TryGetValue(planeKey, out tempBounds))
                        {
                            BHoM.Geometry.BoundingBox b = c.Bounds();
                            wallPlanes.Add(plane);
                            addedPlanes.Add(planeKey, b);
                            bounds.Add(b);
                        }
                        else
                        {
                            tempBounds.Merge(c.Bounds());
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            for (int i = 0; i < bounds.Count; i++)
            {
                bounds[i] = bounds[i].Inflate(m_SnapWallTolerance * 1.1);
            }

            SnapToPlanes(wallPlanes, tolerance, bounds);
        }

        private void SnapToPlanes(List<BHoM.Geometry.Plane> planes, double tolerance, List<BHoM.Geometry.BoundingBox> bounds = null)
        {
            foreach (Node n in new BHoM.Base.ObjectFilter<Node>())
            {              
                for (int i = 0; i < planes.Count;i++)
                {
                    if (bounds != null && !bounds[i].Contains(n.Point)) continue;

                    if (Math.Abs(planes[i].DistanceTo(n.Point)) <= tolerance)
                    {
                        n.Point.Project(planes[i]);
                    }
                }
            }

            foreach (BHoM.Structural.Elements.Panel p in new BHoM.Base.ObjectFilter<BHoM.Structural.Elements.Panel>())
            {
                try
                {
                    //BHoM.Geometry.Curve c = BHoM.Geometry.Curve.Join(p.Edges)[0];
                    //BHoM.Geometry.Plane plane = null;
                    //if (c.TryGetPlane(out plane))
                    //{

                    BHoM.Geometry.BoundingBox curveBounds = p.External_Contours.Bounds();
                    for (int i = 0; i < planes.Count; i++)
                    {
                        if (bounds != null && !BHoM.Geometry.BoundingBox.InRange(bounds[i], curveBounds)) continue;
                        //if (BHoM.Geometry.Vector.VectorAngle(plane.Normal, planes[i].Normal) < Math.PI / 12)
                        //{
                        //    double distance = Math.Abs(planes[i].DistanceTo(plane.Origin));
                        //    if (distance > 0 && distance <= tolerance)
                        //    {
                        //        p.Edges.Project(planes[i]);
                        //        break;
                        //    }
                        //}
                        //else
                        //{
                        p.External_Contours = GeometryUtils.SnapTo(p.External_Contours, planes[i], tolerance);
                        p.Internal_Contours = GeometryUtils.SnapTo(p.Internal_Contours, planes[i], tolerance);
                        //}
                    }
                    //}
                }
                catch  (Exception ex)
                {

                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                m_SnapLevelTolerance = double.Parse(textLevelTolerance.Text) / 1000;
                textLevelTolerance.BackColor = Color.White;
            }
            catch
            {
                textLevelTolerance.BackColor = Color.Red;
            }
        }

        private void textGridTolerance_TextChanged(object sender, EventArgs e)
        {
            try
            {
                m_SnapGridTolerance = double.Parse(textGridTolerance.Text) / 1000;
                textLevelTolerance.BackColor = Color.White;
            }
            catch
            {
                textLevelTolerance.BackColor = Color.Red;
            }
        }

        private void textWallTolerance_TextChanged(object sender, EventArgs e)
        {
            try
            {
                m_SnapWallTolerance = double.Parse(textWallTolerance.Text) / 1000;
                textLevelTolerance.BackColor = Color.White;
            }
            catch
            {
                textLevelTolerance.BackColor = Color.Red;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                m_Rounding = int.Parse(textBox2.Text);
                textBox2.BackColor = Color.White;
            }
            catch
            {
                textBox2.BackColor = Color.Red;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                m_MergeTolerance = double.Parse(textBox3.Text) / 1000;
                textBox3.BackColor = Color.White;
            }
            catch
            {
                textBox3.BackColor = Color.Red;
            }
        }

        private void checkRound_CheckedChanged(object sender, EventArgs e)
        {
            if (checkRound.Checked)
            {
                textBox2.Enabled = true;
                textBox2.Text = "3";
                m_Rounding = 3;
            }
            else
            {
                textBox2.Enabled = false;
                m_Rounding = 9;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox3.Enabled = true;
                textBox3.Text = "100";
            }
            else
            {
                textBox3.Enabled = false;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonCheckAllLevels_Click(object sender, EventArgs e)
        {
            CheckAll(listViewLevels);
        }

        private void CheckAll(ListView listView)
        {
            foreach (ListViewItem item in listView.Items)
            {
                item.Checked = true;
            }
        }

        private void CheckNone(ListView listView)
        {
            foreach (ListViewItem item in listView.Items)
            {
                item.Checked = false;
            }
        }

        private void buttonCheckNoLevels_Click(object sender, EventArgs e)
        {
            CheckNone(listViewLevels);
        }

        private void buttonAllGrids_Click(object sender, EventArgs e)
        {
            CheckAll(listViewGrids);
        }

        private void buttonNoGrids_Click(object sender, EventArgs e)
        {
            CheckNone(listViewGrids);
        }

        private void treeViewSelection_AfterCheck(object sender, TreeViewEventArgs e)
        {
            SetChecked(e.Node, e.Node.Checked);
        }

        private void SetChecked(TreeNode node, bool check)
        {
            if (node.Nodes != null)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    n.Checked = check;
                }
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if( saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                m_Filename = saveFileDialog1.FileName;
                textFilename.Text = m_Filename;
            }
        }
    }
}
