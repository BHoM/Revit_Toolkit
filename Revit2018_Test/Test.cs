using System;
using System.Windows.Media.Imaging;
using System.Windows;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace Revit2018_Test
{
    /// <remarks>
    /// This application's main class.
    /// </remarks>
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class Test : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication UIControlledApplication)
        {
            RibbonPanel aRibbonPanel = UIControlledApplication.CreateRibbonPanel("Test");
            Add_Test(aRibbonPanel);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication UIControlledApplication)
        {

            return Result.Succeeded;
        }

        private void Add_Test(RibbonPanel RibbonPanel)
        {
            PushButton aPushButton = RibbonPanel.AddItem(new PushButtonData("Test", "Test", System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(Test).Namespace + ".DoTest")) as PushButton;
            aPushButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(TestResource.Test.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            aPushButton.ToolTip = "Test";
        }
    }


    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class DoTest : IExternalCommand
    {
        public Result Execute(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            return Result.Succeeded;
        }
    }

}
