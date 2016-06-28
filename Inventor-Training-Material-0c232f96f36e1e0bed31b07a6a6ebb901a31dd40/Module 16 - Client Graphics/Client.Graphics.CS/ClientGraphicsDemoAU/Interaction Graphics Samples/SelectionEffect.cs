using System;
using System.Collections.Generic;
using System.Text;

using Inventor;
using Autodesk.ADN.Utilities;

namespace ClientGraphicsDemoAU.InteractionCGDemo
{
    class SelectionEffect
    {
        private static AdnClientGraphicsManager _clientGraphicsMng;

        private static AdnInteractionManager _interactionManager;


        public static void Demo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InventorApplication;

            _interactionManager = new AdnInteractionManager(InvApp);

            _interactionManager.Initialize();

            _interactionManager.AddPreSelectionFilter(ObjectTypeEnum.kVertexObject);

            _interactionManager.SelectEvents.OnSelect += 
                new SelectEventsSink_OnSelectEventHandler(SelectEvents_OnSelect);

            _interactionManager.DoSelect("Select vertex: ");

            _clientGraphicsMng = new AdnClientGraphicsManager(
                AdnInventorUtilities.InventorApplication,
                AdnInventorUtilities.AddInGuid);

            _clientGraphicsMng.SetGraphicsSource(
                _interactionManager.InteractionEvents);
        }

        static void SelectEvents_OnSelect(
            ObjectsEnumerator JustSelectedEntities, 
            SelectionDeviceEnum SelectionDevice, 
            Point ModelPosition, 
            Point2d ViewPosition, 
            View View)
        {
            VertexSelectEffect effect = new VertexSelectEffect(
                _clientGraphicsMng,
                ModelPosition, 
                0.01, 
                0.8);

            RenderStyles styles =
                AdnInventorUtilities.GetProperty(
                    AdnInventorUtilities.InventorApplication.ActiveDocument, 
                    "RenderStyles") as RenderStyles;

            effect.GraphicsNode.RenderStyle = styles["Glass (Limo Tint)"];

            _clientGraphicsMng.DrawDynamicGraphics(effect);
        }
    }

    class VertexSelectEffect : AdnDynamicGraphic
    {
        private Point _pos;
        private double _radius;
        private double _speed;

        public VertexSelectEffect(
            AdnClientGraphicsManager clientGraphicsMng, 
            Point pos, 
            double radius, 
            double speed): 
            base(clientGraphicsMng)
        {
            _pos = pos;
            _radius = radius;
            _speed = speed;
        }

        public override void Update()
        {
            AdnInventorUtilities.Timer Timer = new AdnInventorUtilities.Timer();

            while (_radius < 0.3)
            {
                double dT = Timer.ElapsedSeconds;

                _radius += dT * _speed;

                this.DeleteGraphics();

                SurfaceBody surface =
                    AdnInventorUtilities.InventorApplication.TransientBRep.CreateSolidSphere(
                        _pos,
                        _radius);

                SurfaceGraphics surfGraph = this.GraphicsManager.DrawSurface(
                    surface, 
                    this.GraphicsNode);

                this.GraphicsManager.UpdateView();
            }

            this.DeleteGraphics();
            this.GraphicsManager.UpdateView();
        }
    }
}
