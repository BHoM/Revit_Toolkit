using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.Revit
{
    public class SelectionSettings
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public IEnumerable<int> ElementIds { get; set; } = new List<int>();

        public IEnumerable<string> UniqueIds { get; set; } = new List<string>();

        /***************************************************/
    }
}
