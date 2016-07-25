using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
namespace InventorAddInBasicGUI2
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    public class hackDemJoints : AssemblyJoint
    {
        AssemblyJoint joint;
        public ComponentOccurrence AffectedOccurrenceOne
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ComponentOccurrence AffectedOccurrenceTwo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object Application
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AttributeSets AttributeSets
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AssemblyJointDefinition Definition
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public DriveSettings DriveSettings
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public HealthStatusEnum HealthStatus
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Locked
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ComponentOccurrence OccurrenceOne
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ComponentOccurrence OccurrenceTwo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AssemblyComponentDefinition Parent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Protected
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Suppressed
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ObjectTypeEnum Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Visible
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void GetReferenceKey(ref byte[] ReferenceKey, int KeyContext = 0)
        {
            throw new NotImplementedException();
        }
    }

}