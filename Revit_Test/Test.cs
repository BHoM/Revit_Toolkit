/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.UI.Revit.Adapter;
using BH.Engine.Adapters.Revit;
using System.IO;
using BH.Adapter.Revit;
using BH.oM.Environment.Elements;

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
            RevitAdapter aRevitAdapter = new RevitAdapter(null, true);
            //FilterQuery aFilterQuery = new FilterQuery() { Type = typeof(BuildingElement) };
            //FilterQuery aFilterQuery = Create.LogicalOrFilterQuery(new List<FilterQuery>() { Create.LogicalAndFilterQuery(new List<FilterQuery>() { Create.SelectionFilterQuery(typeof(BuildingElement)), Create.CategoryFilterQuery("Walls") }), Create.CategoryFilterQuery("Sheets") });
            aRevitAdapter.RevitSettings.GeneralSettings.AdapterMode = AdapterMode.Update;

            FilterQuery aFilterQuery = Create.LogicalAndFilterQuery(Create.CategoryFilterQuery("Mechanical Equipment"), Create.TypeNameFilterQuery("FamilyInstance"));

            List<IBHoMObject> aIBHoMObjectList = aRevitAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            List<ModelInstance> aModelInstanceList = aIBHoMObjectList.ConvertAll(x => x as ModelInstance);
            aModelInstanceList.RemoveAll(x => x == null);

            for(int i=0; i < aModelInstanceList.Count; i++)
                aModelInstanceList[i] = aModelInstanceList[i].Move(BH.Engine.Geometry.Create.Vector(100, 0, 0));

            aRevitAdapter.Push(aModelInstanceList);

            return Result.Succeeded;
        }

        public Result Execute_Old_1(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            //Creating Revit Adapter for active Revit Document
            RevitUIAdapter pRevitInternalAdapter = new RevitUIAdapter(null, ExternalCommandData.Application.ActiveUIDocument.Document);

            FilterQuery aFilterQuery = null;
            List<IBHoMObject> aBHoMObjectList = null;

            RevitSettings aRevitSetting = new RevitSettings();
            //aRevitSetting.SelectionSettings.ElementIds = new List<int>() { 2354 };
            //aRevitSetting.WorksetSettings.WorksetIds = new List<int>() { 0 };
            aRevitSetting.GeneralSettings.DefaultDiscipline = Discipline.Structural;

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

        public Result Execute_Old_2(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            //Creating Revit Adapter for active Revit Document
            RevitUIAdapter pRevitInternalAdapter = new RevitUIAdapter(null, ExternalCommandData.Application.ActiveUIDocument.Document);

            FamilyLibrary aFamilyLibrary = Create.FamilyLibrary(@"C:\Users\jziolkow\Desktop\Families");

            RevitSettings aRevitSetting = Create.RevitSettings();
            aRevitSetting.GeneralSettings.AdapterMode = AdapterMode.Update;
            aRevitSetting = aRevitSetting.SetFamilyLibrary(aFamilyLibrary);

            List<IBHoMObject> aIBHoMObjectList = new List<IBHoMObject>();

            Sheet aSheet = Create.Sheet("Test", "100");
            //FloorPlan aFloorPlan = Create.FloorPlan("New Floor Plan", "Level 01");


            //aIBHoMObjectList.Add(Create.DraftingObject("BHE_GenericAnnotations_PipeAccessories_BibTap", "Bib Tap", BH.Engine.Geometry.Create.Point(0, 0, 0), "Drafting View"));
            //aIBHoMObjectList.Add(Create.ModelInstance(BH.Engine.Geometry.Create.Point(0, 10, 0), "BHE_MechanicalEquipment_AHUPlant_AHUSideBySide_New", "AHU New Type"));
            //aIBHoMObjectList.Add(Create.ModelInstance(BH.Engine.Geometry.Create.Point(0, 20, 0), "BHE_MechanicalEquipment_AHUPlant_AHUSideBySide_New", "AHU"));
            aIBHoMObjectList.Add(aSheet);
            //aIBHoMObjectList.Add(aFloorPlan);

            pRevitInternalAdapter.RevitSettings = aRevitSetting;

            List<IObject> aIObjects = pRevitInternalAdapter.Push(aIBHoMObjectList);

            //

            int aIndex = aIObjects.FindIndex(x => x is Sheet);
            if (aIndex != -1)
                aIObjects[aIndex] = (aIObjects[aIndex] as IBHoMObject).UpdateCustomDataValue("TEST", "OK");

            pRevitInternalAdapter.Push(aIObjects);



            return Result.Succeeded;
        }

        public Result Execute_Old_3(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            //Creating Revit Adapter for active Revit Document
            RevitUIAdapter pRevitInternalAdapter = new RevitUIAdapter(null, ExternalCommandData.Application.ActiveUIDocument.Document);

            FamilyLibrary aFamilyLibrary = Create.FamilyLibrary(@"C:\Users\jziolkow\Desktop\Families");

            RevitSettings aRevitSetting = Create.RevitSettings();
            aRevitSetting.GeneralSettings.AdapterMode = AdapterMode.Update;
            aRevitSetting = aRevitSetting.SetFamilyLibrary(aFamilyLibrary);

            pRevitInternalAdapter.RevitSettings = aRevitSetting;

            List<IBHoMObject> aIBHoMObjectList = new List<IBHoMObject>();

            //FilterQuery aFilterQuery = new FilterQuery() { Type = typeof(Building) };
            FilterQuery aFilterQuery = new FilterQuery() { Type = typeof(BHoMObject) };
            aIBHoMObjectList = pRevitInternalAdapter.Pull(aFilterQuery).Cast<IBHoMObject>().ToList();

            return Result.Succeeded;
        }

        public Result Execute_Old_4(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            List<string> aResult = new List<string>();
            DirectoryInfo aDirectoryInfo = Directory.CreateDirectory(@"C:\Users\jziolkow\Documents\Tasks\Families\2018 Families");
            foreach (FileInfo aFileInfo in aDirectoryInfo.GetFiles("*.rfa", SearchOption.AllDirectories))
            {
                try
                {
                    RevitFilePreview aReviFilePreview = Create.RevitFilePreview(aFileInfo.FullName);
                    if (aReviFilePreview == null)
                        continue;

                    string aCategoryName = Query.CategoryName(aReviFilePreview);
                    string aOmniClass = Query.OmniClass(aReviFilePreview);
                    List<string> aTypeNameList = Query.FamilyTypeNames(aReviFilePreview);
                    foreach (string aTypeName in aTypeNameList)
                    {
                        aResult.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\n", aFileInfo.FullName, Path.GetFileNameWithoutExtension(aFileInfo.FullName), aTypeName, aCategoryName, aOmniClass));
                    }


                }
                catch (Exception aException)
                {
                    aResult.Add(string.Format("{0}\t{1}\n", aFileInfo.FullName, aException.Message));
                }
            }

            File.WriteAllLines(@"C:\Users\jziolkow\Desktop\Report.txt", aResult);

            return Result.Succeeded;
        }
    }

}
