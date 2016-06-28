////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Philippe Leefsma 2011 - ADN/Developer Technical Services
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Inventor;

namespace Autodesk.ADN.Utility.Interaction
{
    /////////////////////////////////////////////////////////////////
    // A wrapper class to handle Interaction Events
    //
    /////////////////////////////////////////////////////////////////
    public class AdnInteractionManager
    {
        /////////////////////////////////////////////////////////////
        // Members
        //
        /////////////////////////////////////////////////////////////
        private Inventor.Application _Application;

        private InteractionEvents _InteractionEvents;
        private SelectEvents _SelectEvents;
        private MouseEvents _MouseEvents;

        private bool _AllowCleanup;

        private List<System.Object> _SelectedEntities;

        private List<ObjectTypeEnum> _PreSelectFilters;

        /////////////////////////////////////////////////////////////
        // Constructor
        //
        /////////////////////////////////////////////////////////////
        public AdnInteractionManager(Inventor.Application Application)
        {
            _Application = Application;

            _InteractionEvents = null;

            _SelectedEntities = new List<System.Object>();

            _PreSelectFilters = new List<ObjectTypeEnum>();
        }

        public InteractionEvents InteractionEvents
        {
            get
            {
                return _InteractionEvents;
            }
        }

        public SelectEvents SelectEvents
        {
            get
            {
                return _SelectEvents;
            }
        }

        public MouseEvents MouseEvents
        {
            get
            {
                return _MouseEvents;
            }
        }

        /////////////////////////////////////////////////////////////
        // Use: Returns list of currently selected entities
        //
        /////////////////////////////////////////////////////////////
        public List<System.Object> SelectedEntities
        {
            get
            {
                return _SelectedEntities;
            }
        }

        /////////////////////////////////////////////////////////////
        // use: Initializes event handlers
        //
        /////////////////////////////////////////////////////////////
        public void Initialize()
        {
            _InteractionEvents =
               _Application.CommandManager.CreateInteractionEvents();

            _SelectEvents = _InteractionEvents.SelectEvents;
            _MouseEvents = _InteractionEvents.MouseEvents;

            _SelectEvents.Enabled = true;
            _SelectEvents.SingleSelectEnabled = false;

            _SelectEvents.OnPreSelect +=
                new SelectEventsSink_OnPreSelectEventHandler(
                    SelectEvents_OnPreSelect);

            _SelectEvents.OnSelect +=
                new SelectEventsSink_OnSelectEventHandler(
                    SelectEvents_OnSelect);

            _SelectEvents.OnUnSelect +=
                new SelectEventsSink_OnUnSelectEventHandler(
                    SelectEvents_OnUnSelect);

            _InteractionEvents.OnTerminate +=
                new InteractionEventsSink_OnTerminateEventHandler(
                    InteractionEvents_OnTerminate);
        }

        /////////////////////////////////////////////////////////////
        // Use: Add selection filter.
        //
        /////////////////////////////////////////////////////////////
        public void AddSelectionFilter(SelectionFilterEnum filter)
        {
            if (_SelectEvents != null)
            {
                _SelectEvents.AddSelectionFilter(filter);
            }
        }

        /////////////////////////////////////////////////////////////
        // Use: Add pre-selection filter.
        //
        /////////////////////////////////////////////////////////////
        public void AddPreSelectionFilter(
            ObjectTypeEnum preSelectFilter)
        {
            _PreSelectFilters.Add(preSelectFilter);
        }

        /////////////////////////////////////////////////////////////
        // Use: Clear all selection filters
        //
        /////////////////////////////////////////////////////////////
        public void ClearSelectionFilters()
        {
            _PreSelectFilters.Clear();

            if (_SelectEvents != null)
            {
                _SelectEvents.ClearSelectionFilter();
            }
        }

        /////////////////////////////////////////////////////////////
        // Use: Starts selection.
        //
        /////////////////////////////////////////////////////////////
        public void Start(string statusbartxt)
        {
            _SelectedEntities.Clear();

            _InteractionEvents.StatusBarText = statusbartxt;

            _InteractionEvents.Start();

            _AllowCleanup = true;
        }

        public void Stop()
        {
            _AllowCleanup = false;

            _InteractionEvents.Stop();
        }

        /////////////////////////////////////////////////////////////
        // Use: OnPreSelect Handler.
        //
        /////////////////////////////////////////////////////////////
        private void SelectEvents_OnPreSelect(
            ref object PreSelectEntity,
            out bool DoHighlight,
            ref ObjectCollection MorePreSelectEntities,
            SelectionDeviceEnum SelectionDevice,
            Point ModelPosition,
            Point2d ViewPosition,
            View View)
        {
            if (_PreSelectFilters.Count != 0 &&
                !_PreSelectFilters.Contains(
                    AdnInteractionManager.GetInventorType(PreSelectEntity)))
            {
                DoHighlight = false;
                return;
            }

            DoHighlight = true;
        }

        /////////////////////////////////////////////////////////////
        // Use: OnSelect Handler.
        //
        /////////////////////////////////////////////////////////////
        private void SelectEvents_OnSelect(
            ObjectsEnumerator JustSelectedEntities,
            SelectionDeviceEnum SelectionDevice,
            Point ModelPosition,
            Point2d ViewPosition,
            View View)
        {
            if (JustSelectedEntities.Count != 0)
            {
                System.Object selectedObj = JustSelectedEntities[1];

                _SelectedEntities.Add(selectedObj);
            }
        }

        /////////////////////////////////////////////////////////////
        // Use: OnUnSelect Handler.
        //
        /////////////////////////////////////////////////////////////
        void SelectEvents_OnUnSelect(
            ObjectsEnumerator UnSelectedEntities,
            SelectionDeviceEnum SelectionDevice,
            Point ModelPosition,
            Point2d ViewPosition,
            View View)
        {
            foreach (System.Object unselectedObj in
                UnSelectedEntities)
            {
                if (_SelectedEntities.Contains(unselectedObj))
                {
                    _SelectedEntities.Remove(unselectedObj);
                }
            }
        }

        /////////////////////////////////////////////////////////////
        // Use: Performs interaction cleanup.
        //
        /////////////////////////////////////////////////////////////
        private void CleanUp()
        {
            if (_InteractionEvents != null)
            {
                _SelectedEntities.Clear();

                _PreSelectFilters.Clear();

                _SelectEvents.OnPreSelect -=
                   new SelectEventsSink_OnPreSelectEventHandler(
                       SelectEvents_OnPreSelect);

                _SelectEvents.OnSelect -=
                    new SelectEventsSink_OnSelectEventHandler(
                        SelectEvents_OnSelect);

                _SelectEvents.OnUnSelect -=
                   new SelectEventsSink_OnUnSelectEventHandler(
                       SelectEvents_OnUnSelect);

                _InteractionEvents.OnTerminate -=
                    new InteractionEventsSink_OnTerminateEventHandler(
                        InteractionEvents_OnTerminate);

                Marshal.ReleaseComObject(_SelectEvents);
                _SelectEvents = null;

                Marshal.ReleaseComObject(_InteractionEvents);
                _InteractionEvents = null;
            }
        }

        /////////////////////////////////////////////////////////////
        // Use: Terminates Interaction event.
        //
        /////////////////////////////////////////////////////////////
        public void Terminate()
        {
            if (_InteractionEvents != null)
            {
                _AllowCleanup = true;

                _InteractionEvents.Stop();
            }
        }

        /////////////////////////////////////////////////////////////
        // Use: OnTerminate handler.
        //
        /////////////////////////////////////////////////////////////
        private void InteractionEvents_OnTerminate()
        {
            if (_AllowCleanup)
            {
                CleanUp();

                OnTerminate(new OnTerminateEventArgs());
            }
        }

        /////////////////////////////////////////////////////////////
        // Use: Exposes OnTerminate Event for InteractionManager.
        //
        /////////////////////////////////////////////////////////////
        public class OnTerminateEventArgs : EventArgs
        {
            public OnTerminateEventArgs()
            {

            }
        }

        private void OnTerminate(OnTerminateEventArgs e)
        {
            if (OnTerminateEvent != null)
                OnTerminateEvent(this, e);
        }

        public delegate void OnTerminateHandler(object o,
            OnTerminateEventArgs e);

        public event OnTerminateHandler OnTerminateEvent = null;

        /////////////////////////////////////////////////////////////
        // Use: Late-binded method to retrieve ObjectType property
        //
        /////////////////////////////////////////////////////////////
        private static ObjectTypeEnum GetInventorType(Object obj)
        {
            try
            {
                System.Object objType = obj.GetType().InvokeMember(
                    "Type",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    obj,
                    null,
                    null, null, null);

                return (ObjectTypeEnum)objType;
            }
            catch
            {
                //error...
                return ObjectTypeEnum.kGenericObject;
            }
        }
    }
}
