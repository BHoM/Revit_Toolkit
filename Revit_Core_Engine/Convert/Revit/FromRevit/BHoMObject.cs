/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/
        
        [Description("Converts a Revit Element to a generic BHoM object, either ModelInstance or DraftingInstance (if the element has location in space) or a BHoMObject otherwise.")]
        [Input("element", "Revit Element to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("object", "BHoM object resulting from converting the given Revit Element.")]
        public static IBHoMObject ObjectFromRevit(this Element element, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            IBHoMObject iBHoMObject = refObjects.GetValue<IBHoMObject>(element.Id);
            if (iBHoMObject != null)
                return iBHoMObject;

            IGeometry iGeometry = null;

            FamilyInstance familyInstance = element as FamilyInstance;
            if (familyInstance != null && AdaptiveComponentInstanceUtils.IsAdaptiveComponentInstance(familyInstance))
            {
                IEnumerable<BH.oM.Geometry.Point> pts = AdaptiveComponentInstanceUtils.GetInstancePointElementRefIds(familyInstance).Select(x => ((ReferencePoint)familyInstance.Document.GetElement(x)).Position.PointFromRevit());
                iGeometry = new CompositeGeometry { Elements = new List<IGeometry>(pts) };
            }

            if (iGeometry == null)
                iGeometry = element.Location.IFromRevit();

            if (iGeometry != null)
            {
                ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
                if (elementType != null)
                {
                    InstanceProperties objectProperties = elementType.InstancePropertiesFromRevit(settings, refObjects) as InstanceProperties;
                    if (objectProperties != null)
                    {
                        if (element.ViewSpecific)
                            iBHoMObject = BH.Engine.Adapters.Revit.Create.DraftingInstance(objectProperties, element.Document.GetElement(element.OwnerViewId).Name, iGeometry as dynamic);
                        else
                            iBHoMObject = BH.Engine.Adapters.Revit.Create.ModelInstance(objectProperties, iGeometry as dynamic);

                        if (iGeometry is BH.oM.Geometry.Point)
                        {
                            Basis orientation = null;
                            if (element is FamilyInstance)
                            {
                                FamilyInstance fi = ((FamilyInstance)element);
                                XYZ x = fi.HandOrientation;
                                XYZ y = fi.FacingOrientation;
                                XYZ z = fi.GetTotalTransform().BasisZ;

                                orientation = new Basis(new Vector { X = x.X, Y = x.Y, Z = x.Z }, new Vector { X = y.X, Y = y.Y, Z = y.Z }, new Vector { X = z.X, Y = z.Y, Z = z.Z });
                            }
                            
                            ((IInstance)iBHoMObject).Orientation = orientation;
                        }
                    }
                }
            }

            if (iBHoMObject == null)
                iBHoMObject = new BHoMObject();

            iBHoMObject.Name = element.Name;
            iBHoMObject.SetIdentifiers(element);
            iBHoMObject.CopyParameters(element, settings.MappingSettings);
            iBHoMObject.SetProperties(element, settings.MappingSettings);

            refObjects.AddOrReplace(element.Id, iBHoMObject);

            return iBHoMObject;
        }

        /***************************************************/
    }
}



