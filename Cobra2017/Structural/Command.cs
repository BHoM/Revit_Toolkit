using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Cobra2017.Structural.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BHoM.Global;
using BHoM.Base;
using BHoM.Structural.Elements;
using Revit2017_Adapter.Structural;
using BHoM.Structural;

namespace Cobra2017.Structural
{

    /**********************************************/
    /****  Export Element                      ****/
    /**********************************************/

    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            new ExportForm(commandData.Application.ActiveUIDocument.Document).ShowDialog();

            return Result.Succeeded;
        }
    }

    /**********************************************/
    /****  Import Element                      ****/
    /**********************************************/
    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class ImportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string filename = Path.Combine(Path.Combine(Path.GetTempPath(), "RevitExchange"),"In");

            List<BHoMObject> objlist = new List<BHoMObject>();
            using (StreamReader fs = new StreamReader(Path.Combine(filename, "Bar" + ".txt")))
            {
                objlist = BHoM.Base.BHoMJSON.ReadPackage(fs.ReadToEnd()).Cast<BHoMObject>().ToList();
                fs.Close();
            }

            Autodesk.Revit.UI.UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;
            foreach (BHoMObject obj in objlist)
            {
                Engine.Convert.RevitElement.Write(obj, doc);
            }
            return Result.Succeeded;
        }
    }

    /**********************************************/
    /****  Set Parameters                      ****/
    /**********************************************/
    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class SetParameters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            string path = Path.Combine(Path.GetTempPath(), "RevitParams.csv");

            using (StreamReader fs = new StreamReader(path))
            {
                string[] headings = fs.ReadLine().Split(',');

                while (fs.Peek() >= 0)
                {
                    string[] data = fs.ReadLine().Split(',');

                    string id = data[0];

                    ElementId revitId = new ElementId(int.Parse(id));

                    Element e = doc.GetElement(revitId);
                    
                    if (e != null)
                    {
                        for (int i = 1; i < headings.Length; i++)
                        {
                            Parameter p = e.LookupParameter(headings[i]);
                            if (p != null)
                            {
                                bool succeeded = false;
                                try
                                {
                                    switch (p.StorageType)
                                    {
                                        case StorageType.Double:
                                            succeeded = e.LookupParameter(headings[i]).Set(double.Parse(data[i]));
                                            break;
                                        case StorageType.Integer:
                                            succeeded = e.LookupParameter(headings[i]).Set(int.Parse(data[i]));
                                            break;
                                        case StorageType.String:
                                            succeeded = e.LookupParameter(headings[i]).Set(data[i]);
                                            break;
                                        case StorageType.ElementId:
                                            succeeded = e.LookupParameter(headings[i]).Set(new ElementId(int.Parse(data[i])));
                                            break;
                                    }
                                }
                                catch
                                {
                                    message += "Error: Parameter data " + headings[i] + "->" + data[i] + " on Element " + e.Name + " (Id=" + e.Id + ") could not be cast to a " + p.StorageType + "\n";
                                }
                            }
                            else
                            {
                                message += "Error: Parameter " + headings[i] + " does not exists on Element " + e.Name;
                            }
                        }
                    }
                    else
                    {
                        message += "Error: Element " + id + " does not exists in the project";
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
