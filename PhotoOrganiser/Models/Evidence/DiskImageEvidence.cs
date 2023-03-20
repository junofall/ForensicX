﻿using ForensicX.Models.Disks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Evidence
{
    public class DiskImageEvidence : EvidenceItem
    {
        public Disk DiskInstance { get; private set; }

        public void Load()
        {
            DiskInstance = new Disk(Path, 512);
        }
    }
}
