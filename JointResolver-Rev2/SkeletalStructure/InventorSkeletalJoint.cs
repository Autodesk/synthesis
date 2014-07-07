using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface InventorSkeletalJoint
{
    SkeletalJoint getWrapped();

    void determineLimits();
}
