using System;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

using BH.UI.Revit.Adapter;
using BH.oM.Environment.Elements;
using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit;

namespace Revit_Test
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
            //Creating Revit Adapter for active Revit Document
            CobraAdapter pRevitInternalAdapter = new CobraAdapter(ExternalCommandData.Application.ActiveUIDocument.Document);

            FilterQuery aFilterQuery = null;
            List<IBHoMObject> aBHoMObjectList = null;

            RevitSettings aRevitSetting = new RevitSettings();
            aRevitSetting.SelectionSettings.ElementIds = new List<int>() { 2354 };
            //aRevitSetting.WorksetSettings.WorksetIds = new List<int>() { 0 };

            pRevitInternalAdapter.RevitSettings = aRevitSetting;

            //pRevitInternalAdapter.RevitSettings.SelectionSettings.IncludeSelected = true;

            pRevitInternalAdapter.RevitSettings.WorksetSettings.OpenWorksetsOnly = true;

            aFilterQuery = new FilterQuery() { Type = typeof(BuildingElement) };
            aBHoMObjectList = pRevitInternalAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            //pRevitInternalAdapter.Delete(aBHoMObjectList.Cast<BHoMObject>());

            pRevitInternalAdapter.UpdateProperty(aFilterQuery, "Structural", true);

            //pRevitInternalAdapter.Push(aBHoMObjectList);

            //aFilterQuery = new FilterQuery() { Type = typeof(Space) };
            //aBHoMObjectList = pRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            //aFilterQuery = new FilterQuery() { Type = typeof(BuildingElement) };
            //aBHoMObjectList = pRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            //aFilterQuery = new FilterQuery() { Type = typeof(Building) };
            //aBHoMObjectList = pRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            //aFilterQuery = new FilterQuery() { Type = typeof(PanelPlanar) };
            //aBHoMObjectList = pRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            //aFilterQuery = new FilterQuery() { Type = typeof(Bar) };
            //aBHoMObjectList = pRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            //aFilterQuery = new FilterQuery() { Type = typeof(BH.oM.Architecture.Elements.Grid) };
            //aBHoMObjectList = pRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            //aFilterQuery = new FilterQuery() { Type = typeof(Beam) };
            //aBHoMObjectList = pRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            //aFilterQuery = new FilterQuery() { Type = typeof(Storey) };
            //aBHoMObjectList = pRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();
            //Storey aStorey = aBHoMObjectList.First() as Storey;
            //aStorey = aStorey.Copy("Level 2", aStorey.Elevation + 9.84252);

            //aFilterQuery = new FilterQuery() { Type = typeof(BuildingElement) };
            //aBHoMObjectList = pRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();
            //List<IObject> aObjectList = new List<IObject>();
            //foreach (BuildingElement aBuildingElement in aBHoMObjectList)
            //    aObjectList.Add(aBuildingElement.Move(aStorey));

            //pRevitAdapter.Push(aObjectList);



            return Result.Succeeded;
        }
    }

}
