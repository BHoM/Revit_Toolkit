using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BH.oM.Base.Attributes;
using BH.Revit.Engine.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Sets the selection to the specified elements in the Revit UI.")]
        [Input("uiDoc", "The UIDocument to set selection in.")]
        [Input("elements", "List of elements to set as selected.")]
        public static void SetSelection(this UIDocument uiDoc, List<Element> elements)
        {
            if (uiDoc == null || elements == null || !elements.Any())
                return;

#if REVIT2022
            List<Element> hostElements = elements.Where(x => !x.Document.IsLinked).ToList();

            if (hostElements.Any())
                uiDoc.Selection.SetElementIds(hostElements.Select(x => x.Id).ToList());
#else
            List<Reference> references = new List<Reference>();

            foreach (Element element in elements)
            {
                if (element.Document.IsLinked)
                {
                    // Get reference for a linked element
                    RevitLinkInstance linkInstance = element.Document.LinkInstance();
                    if (linkInstance != null)
                    {
                        Reference linkRef = new Reference(element);
                        linkRef = linkRef.CreateLinkReference(linkInstance);
                        references.Add(linkRef);
                    }
                }
                else
                {
                    // Get reference for a host element
                    references.Add(new Reference(element));
                }
            }

            if (references.Any())
            {
                uiDoc.Selection.SetReferences(references);
            }
#endif
        }

        /***************************************************/
    }
}