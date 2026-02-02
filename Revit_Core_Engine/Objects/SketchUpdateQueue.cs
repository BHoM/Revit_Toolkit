using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static class SketchUpdateQueue
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Queue containing actions to update floor sketches. These updates are deferred until after the main push transaction to avoid conflicts with sketch editing.")]
        public static Queue<Action> SketchUpdates { get; } = new Queue<Action>();

        /***************************************************/
    }
}
