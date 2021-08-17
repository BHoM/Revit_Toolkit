﻿using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit
{
    public class RevitHostFragment : IFragment, IImmutable
    {
        public virtual int HostId { get; } = -1;

        public virtual int LinkDocumentId { get; } = -1;

        public RevitHostFragment(int hostId, int linkDocumentId = -1)
        {
            HostId = hostId;
            LinkDocumentId = linkDocumentId;
        }
    }
}
