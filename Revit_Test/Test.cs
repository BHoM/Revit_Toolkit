using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.UI.Cobra.Adapter;
using BH.Engine.Adapters.Revit;

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

            FamilyLibrary aFamilyLibrary = Create.FamilyLibrary(@"C:\Users\jziolkow\Desktop\Families");

            RevitSettings aRevitSetting = Create.RevitSettings();
            aRevitSetting = aRevitSetting.SetFamilyLibrary(aFamilyLibrary);

            List<IBHoMObject> aIBHoMObjectList = new List<IBHoMObject>();

            Sheet aSheet = Create.Sheet("Test", "100");
            FloorPlan aFloorPlan = Create.FloorPlan("New Floor Plan", "Level 01");


            aIBHoMObjectList.Add(Create.GenericObject(BH.Engine.Geometry.Create.Point(0, 0, 0), "BHE_MechanicalEquipment_AHUPlant_AHUStacked", "AHU New Type"));
            aIBHoMObjectList.Add(Create.GenericObject(BH.Engine.Geometry.Create.Point(0, 10, 0), "BHE_MechanicalEquipment_AHUPlant_AHUSideBySide_New", "AHU New Type"));
            aIBHoMObjectList.Add(Create.GenericObject(BH.Engine.Geometry.Create.Point(0, 20, 0), "BHE_MechanicalEquipment_AHUPlant_AHUSideBySide_New", "AHU"));
            aIBHoMObjectList.Add(aSheet);
            aIBHoMObjectList.Add(aFloorPlan);

            pRevitInternalAdapter.RevitSettings = aRevitSetting;

            List<IObject> aIObjects = pRevitInternalAdapter.Push(aIBHoMObjectList);


            return Result.Succeeded;
        }

        public Result Execute_Old(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            //Creating Revit Adapter for active Revit Document
            CobraAdapter pRevitInternalAdapter = new CobraAdapter(ExternalCommandData.Application.ActiveUIDocument.Document);

            FilterQuery aFilterQuery = null;
            List<IBHoMObject> aBHoMObjectList = null;

            RevitSettings aRevitSetting = new RevitSettings();
            aRevitSetting.SelectionSettings.IncludeSelected = true;
            //aRevitSetting.SelectionSettings.ElementIds = new List<int>() { 2354 };
            //aRevitSetting.WorksetSettings.WorksetIds = new List<int>() { 0 };
            aRevitSetting.DefaultDiscipline = Discipline.Structural;

            pRevitInternalAdapter.RevitSettings = aRevitSetting;

            //pRevitInternalAdapter.RevitSettings.SelectionSettings.IncludeSelected = true;

            //pRevitInternalAdapter.RevitSettings.WorksetSettings.OpenWorksetsOnly = true;

            aFilterQuery = new FilterQuery() { Type = typeof(BHoMObject) };
            aBHoMObjectList = pRevitInternalAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            //pRevitInternalAdapter.Delete(aBHoMObjectList.Cast<BHoMObject>());

            //pRevitInternalAdapter.UpdateProperty(aFilterQuery, "Structural", true);

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
