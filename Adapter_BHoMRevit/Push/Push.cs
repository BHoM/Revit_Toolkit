using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BH.oM.Base;

namespace BH.UI.Revit.Adapter
{
    public partial class CobraAdapter
    {

        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/

        public override List<IObject> Push(IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null)
        {
            bool aSuccess = true;

            List<IObject> objectsToPush = Config.CloneBeforePush ? objects.Select(x => x is BHoMObject ? ((BHoMObject)x).GetShallowClone() : x).ToList() : objects.ToList(); //ToList() necessary for the return collection to function properly for cloned objects

            Type iBHoMObjectType = typeof(IBHoMObject);
            MethodInfo miToList = typeof(Enumerable).GetMethod("Cast");
            List<IObject> aResult = new List<IObject>();
            foreach (var typeGroup in objectsToPush.GroupBy(x => x.GetType()))
            {
                MethodInfo miListObject = miToList.MakeGenericMethod(new[] { typeGroup.Key });

                var list = miListObject.Invoke(typeGroup, new object[] { typeGroup });

                if (iBHoMObjectType.IsAssignableFrom(typeGroup.Key))
                    aSuccess &= Create(list as dynamic);                    
            }

            return aSuccess ? objectsToPush : new List<IObject>();
        }

    }
}
