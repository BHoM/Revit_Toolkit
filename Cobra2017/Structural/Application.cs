using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Resource = Cobra2017.Properties.Resources;
using System.Globalization;

namespace Cobra2017.Structural
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

            PushButton exportButton = importExport.AddItem(exportGeometry) as PushButton;
            PushButton importButton = importExport.AddItem(importGeometry) as PushButton;
            PushButton setParamsButton = importExport.AddItem(setParams) as PushButton;
            exportButton.LargeImage = BmpImageSource("Cobra2017.Resources.BHoM_Uppload.png");
            importButton.LargeImage = BmpImageSource("Cobra2017.Resources.BHoM_Download.png");
            return Result.Succeeded;
        }

        private System.Windows.Media.ImageSource BmpImageSource(string embeddedPath)
        {
            Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            return decoder.Frames[0];
        }
    }

}
