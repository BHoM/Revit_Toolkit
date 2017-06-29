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
using BHoM.Global;
using BHoM.Base;

namespace Cobra2017.Structural.Forms
{
    public partial class ImportForm : Form
    {
        private string m_Filename;
        private Autodesk.Revit.DB.Document m_Document;

        public ImportForm(AD.Document document)
        {
            InitializeComponent();
            InitializeComponent();
            m_Document = document;
            m_Filename = Path.Combine(Path.GetTempPath(), "RevitExchange");
            //checkRound.Checked = true;
            //checkBox1.Checked = true;
            //textLevelTolerance.Text = "500";
            //textGridTolerance.Text = "300";
            //textWallTolerance.Text = "300";
            //comboExportOption.SelectedIndex = 0;
            //LoadRevitElements();
            //LoadRevitLevels();
            //LoadRevitGrids();
        }

        private void ImportForm_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(Path.Combine(Path.Combine(Path.GetTempPath(), "RevitExchange"), "In"));
            List<BHoMObject> objlist = new List<BHoMObject>();
            foreach (string path in files)
            {
                using (StreamReader fs = new StreamReader(path))
                {
                    objlist.AddRange(BHoM.Base.BHoMJSON.ReadPackage(fs.ReadToEnd()).Cast<BHoMObject>().ToList());
                    fs.Close();
                }
            }
            foreach (BHoMObject obj in objlist)
            {
                Engine.Convert.RevitElement.Write(obj, m_Document);
            }
            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
