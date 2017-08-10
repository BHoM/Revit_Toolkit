using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Resource = Cobra2018.Properties.Resources;

namespace Cobra2018.Structural
{
    public class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab(Resource.StructuralTab);
            RibbonPanel importExport = application.CreateRibbonPanel(Resource.StructuralTab, "Import/Export");

            RibbonItemData exportGeometry = new PushButtonData("Export Geometry", "Export", Assembly.GetExecutingAssembly().Location, "Cobra" + Resource.Version + ".Structural.ExportCommand");
            RibbonItemData importGeometry = new PushButtonData("Import Geometry", "Import", Assembly.GetExecutingAssembly().Location, "Cobra" + Resource.Version + ".Structural.ImportCommand");
            RibbonItemData setParams = new PushButtonData("SetParams", "Set Parameters", Assembly.GetExecutingAssembly().Location, "Cobra" + Resource.Version + ".Structural.SetParameters");

            importExport.AddItem(exportGeometry);
            importExport.AddItem(importGeometry);
            importExport.AddItem(setParams);

            return Result.Succeeded;
        }
    }
}
