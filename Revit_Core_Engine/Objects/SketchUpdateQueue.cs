using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static class SketchUpdateQueue
    {
        public static Queue<Action> SketchUpdates { get; } = new Queue<Action>();
    }
}
