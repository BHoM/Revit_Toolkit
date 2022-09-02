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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Takes a snapshot of the document with given elements")]
        public static Dictionary<int, string> GetSnapshot(IEnumerable<Element> elements)
        {
            Dictionary<int, string> snapshot = new Dictionary<int, string>();

            SHA256 hasher = SHA256Managed.Create();

            foreach (Element element in elements)
            {
                //Debug.Print( e.Id.IntegerValue.ToString() 
                //  + " " + e.GetType().Name );

                string elementState = GetElementState(element);

                if (null != elementState)
                {
                    string hashb64 = System.Convert.ToBase64String(
                      hasher.ComputeHash(GetBytes(elementState)));

                    snapshot.Add(element.Id.IntegerValue, hashb64);
                }
            }
            return snapshot;
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length
              * sizeof(char)];

            System.Buffer.BlockCopy(str.ToCharArray(),
              0, bytes, 0, bytes.Length);

            return bytes;
        }

        private static string GetPropertiesJson(IList<Parameter> parameters)
        {
            int n = parameters.Count;
            List<string> a = new List<string>(n);
            foreach (Parameter p in parameters)
            {
                a.Add(string.Format("\"{0}\":\"{1}\"",
                  p.Definition.Name, p.AsValueString()));
            }
            a.Sort();
            string s = string.Join(",", a);
            return "{" + s + "}";
        }

        private static string ElementDescription(Document doc, int element_id)
        {
            return ElementDescription(doc.GetElement(
              new ElementId(element_id)));
        }

        private static string ElementDescription(Element element)
        {
            if (null == element)
            {
                return "<null>";
            }

            // For a wall, the element name equals the
            // wall type name, which is equivalent to the
            // family name ...

            FamilyInstance fi = element as FamilyInstance;

            string typeName = element.GetType().Name;

            string categoryName = (null == element.Category)
              ? string.Empty
              : element.Category.Name + " ";

            string familyName = (null == fi)
              ? string.Empty
              : fi.Symbol.Family.Name + " ";

            string symbolName = (null == fi
              || element.Name.Equals(fi.Symbol.Name))
                ? string.Empty
                : fi.Symbol.Name + " ";

            return string.Format("{0} {1}{2}{3}<{4} {5}>",
              typeName, categoryName, familyName,
              symbolName, element.Id.IntegerValue, element.Name);
        }

        private static string LocationString(Location location)
        {
            LocationPoint lp = location as LocationPoint;
            LocationCurve lc = (null == lp)
              ? location as LocationCurve
              : null;

            return null == lp
              ? (null == lc
                ? null
                : CurveTessellateString(lc.Curve))
              : PointString(lp.Point);
        }

        private static string CurveTessellateString(
  Curve curve)
        {
            return PointArrayString(curve.Tessellate());
        }


        private static string PointArrayString(IList<XYZ> pts)
        {
            return string.Join(", ",
              pts.Select<XYZ, string>(
                p => PointString(p)));
        }

        private static string PointString(XYZ p)
        {
            return string.Format("({0},{1},{2})",
              RealString(p.X),
              RealString(p.Y),
              RealString(p.Z));
        }

        private static string RealString(double a)
        {
            return a.ToString("0.##");
        }


        public static void ReportDifferences(Document doc, Dictionary<int, string> start_state, Dictionary<int, string> end_state)
        {
            int n1 = start_state.Keys.Count;
            int n2 = end_state.Keys.Count;

            List<int> keys = new List<int>(start_state.Keys);

            foreach (int id in end_state.Keys)
            {
                if (!keys.Contains(id))
                {
                    keys.Add(id);
                }
            }

            keys.Sort();

            int n = keys.Count;

            Debug.Print(
              "{0} elements before, {1} elements after, {2} total",
              n1, n2, n);

            int nAdded = 0;
            int nDeleted = 0;
            int nModified = 0;
            int nIdentical = 0;
            List<string> report = new List<string>();

            foreach (int id in keys)
            {
                if (!start_state.ContainsKey(id))
                {
                    ++nAdded;
                    report.Add(id.ToString() + " added "
                      + ElementDescription(doc, id));
                }
                else if (!end_state.ContainsKey(id))
                {
                    ++nDeleted;
                    report.Add(id.ToString() + " deleted");
                }
                else if (start_state[id] != end_state[id])
                {
                    ++nModified;
                    report.Add(id.ToString() + " modified "
                      + ElementDescription(doc, id));
                }
                else
                {
                    ++nIdentical;
                }
            }

            string msg = string.Format(
              "Stopped tracking changes now.\r\n"
              + "{0} deleted, {1} added, {2} modified, "
              + "{3} identical elements:",
              nDeleted, nAdded, nModified, nIdentical);

            string s = string.Join("\r\n", report);

            Debug.Print(msg + "\r\n" + s);
            TaskDialog dlg = new TaskDialog("Track Changes");
            dlg.MainInstruction = msg;
            dlg.MainContent = s;
            dlg.Show();
        }
    }
}

