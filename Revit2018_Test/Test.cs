using System;
using System.Windows.Media.Imaging;
using System.Windows;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

using BH.Adapter.Revit;

using BH.oM.Environmental.Elements;
using BH.Engine.Environment;
using BH.oM.Structural.Elements;
using BH.Engine.Revit;
using System.Collections.Generic;
using BH.oM.Base;

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
            //Path to the file with Level names (name per line)
            string aPath = @"C:\Users\jziolkow\Desktop\LevelList.txt";

            //Creating Revit Adapter for active Revit Document
            RevitAdapter pRevitAdapter = new RevitAdapter(ExternalCommandData.Application.ActiveUIDocument.Document);

            //Reading existing Revit model and creating BHoM objects
            Building Building = pRevitAdapter.ReadBuilidng(true, true);

            //Storey offset
            double aOffset = 9.84252;

            //Defining base Storey
            Storey aStorey = Building.Storeys[0];

            double aIndex = 1;
            foreach(string aName in System.IO.File.ReadAllLines(aPath))
            {
                //Coping base Storey
                Storey aStorey_New = aStorey.Copy(aName, aOffset * aIndex);
                Utilis.BHoM.RemoveIdentifiers(aStorey_New);

                //Extracting Building Elements from Spaces on base Storey
                List<BHoMObject> aBHoMObjectList = new List<BHoMObject>();
                foreach (Space aSpace in Building.Spaces.FindAll(x => x.Storey == aStorey))
                    foreach (BuildingElement aBuildingElement in aSpace.BuildingElements)
                    {
                        //Coping BuilidngElement objects
                        BuildingElement aBuildingElement_New = aBuildingElement.Copy(aStorey_New);
                        Utilis.BHoM.RemoveIdentifiers(aBuildingElement_New);
                        aBHoMObjectList.Add(aBuildingElement_New);
                    }

                //Creating new Revit Elements from copied BHoMObjects
                pRevitAdapter.Create(aBHoMObjectList, true, false);

                aIndex++;
            }





            return Result.Succeeded;
        }
    }

}
