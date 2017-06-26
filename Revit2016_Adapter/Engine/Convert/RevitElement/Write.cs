﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHoM;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Revit2016_Adapter.Base;
using Revit2016_Adapter.Geometry;
using BHE = BHoM.Structural.Elements;
using BHP = BHoM.Structural.Properties;
using System.Reflection;

namespace Engine.Convert
{
    public static partial class RevitElement
    {

        /**********************************************/
        /****  Generic                             ****/
        /**********************************************/

        /// <summary>
        /// BHomObject to Revit Element Convert swicher.
        /// </summary>
        public static object Write(BHoM.Base.BHoMObject BHobj, Document m_document, string lvl = null)
        {
            if (BHobj is BHoM.Structural.Elements.Bar)
            {
                return Write(BHobj as BHoM.Structural.Elements.Bar, m_document, lvl);
            }
            return null;
        }


        /**********************************************/
        /****  Bar                                 ****/
        /**********************************************/

        /// <summary>
        /// BHomBar to Revit Element.
        /// </summary>
        public static object Write(BHoM.Structural.Elements.Bar BHobj, Document m_document, string lvl = null)
        {

            string secName = BHobj.SectionProperty.Name;
            if (secName.Contains("RHSH"))
            {
                secName = secName.Replace("RHSH", "RHS");
            }
            string typeName = secName.Replace(" ", "");

            // Get Level
            if (lvl == null)
            {
                lvl = Revit2016_Adapter.Base.RevitUtils.GetLevel(m_document, BHobj.EndPoint.Z).Name;
            }

            // Get familysymbol from document.

            FamilySymbol familySymbol = RevitUtils.GetFamilySymbolfromDocument(typeName, m_document);

            // Get familysymbol from path.

            if (familySymbol == null)
            {
                string[] divname = secName.Split(' ');
                familySymbol = RevitUtils.GetFamilySymbolfromPath(typeName,divname[0], m_document);
            }

            // Change Name and check again.
            if (familySymbol == null)
            {
                if (typeName.ElementAt(typeName.Length - 2) != '.')
                {
                    typeName += ".0";
                    // Get familysymbol from document.
                    familySymbol = RevitUtils.GetFamilySymbolfromDocument(typeName, m_document);

                    // Get familysymbol from path
                    if (familySymbol == null)
                    {
                        string[] divname = secName.Split(' ');
                        familySymbol = RevitUtils.GetFamilySymbolfromPath(typeName, divname[0], m_document);
                    }
                }
            }


            // Create familysymbol from section

            if (familySymbol == null)
            {
                familySymbol = RevitUtils.CreateExtrusionFamilySymbol(BHobj,m_document);
            }


            // Insert Element in Revit Project

            if (familySymbol != null)
            {
                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate(); m_document.Regenerate();
                }

                FamilyInstance beam = m_document.Create.NewFamilyInstance(RevitGeometry.Write(BHobj.Line), familySymbol, (Level)RevitUtils.GetElement(m_document, typeof(Level), lvl), Autodesk.Revit.DB.Structure.StructuralType.Beam);
                RevitUtils.DisableEndJoin(beam);

                string[] paramnames = { "Cross-Section Rotation", "z Justification" };
                string[] paramvalues = { UnitUtils.ConvertToInternalUnits(BHobj.OrientationAngle,DisplayUnitType.DUT_RADIANS).ToString(), "1" };
                RevitUtils.SetLookupParameter(beam, paramnames.ToList(), paramvalues.ToList());
                return beam;
            }
            else
            {
                return null;
            }
        }
    }
}
