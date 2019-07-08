using System;
using System.Collections.Generic;
using Inventor;

namespace BxDRobotExporter
{
    internal static class InventorUtils
    {
        /// <summary>
        /// Selects a list of nodes in Inventor.
        /// </summary>
        /// <param name="nodes">List of nodes to select.</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        public static void FocusAndHighlightNodes(List<RigidNode_Base> nodes, Camera camera, double zoom)
        {
            if (nodes == null)
            {
                return;
            }
            var occurrences = GetComponentOccurrencesFromNodes(nodes);
            if (occurrences == null)
            {
                return;
            }
            
            StandardAddInServer.Instance.ChildHighlight.Clear();
            // Highlighting must occur after the camera is moved, as inventor clears highlight objects when the camera is moved
            FocusCameraOnOccurrences(occurrences, 15, camera, zoom, StandardAddInServer.ViewDirection.Y);
            HighlightOccurrences(occurrences);
        }
        
        /// <summary>
        /// Selects a list of nodes in Inventor.
        /// </summary>
        /// <param name="nodes">List of nodes to select.</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        public static void FocusAndHighlightNodes(List<RigidNode_Base> nodes, Camera camera)
        {
            if (nodes == null)
            {
                return;
            }
            var occurrences = GetComponentOccurrencesFromNodes(nodes);
            if (occurrences == null)
            {
                return;
            }
            
            StandardAddInServer.Instance.ChildHighlight.Clear();
            // Highlighting must occur after the camera is moved, as inventor clears highlight objects when the camera is moved
            FocusCameraOnOccurrences(occurrences, 15, camera, StandardAddInServer.ViewDirection.Y);
            HighlightOccurrences(occurrences);
        }

        public static void HighlightOccurrences(List<ComponentOccurrence> occurrences)
        {
            StandardAddInServer.Instance.ClearDOFHighlight();
            StandardAddInServer.Instance.ChildHighlight.Clear();

            foreach (var componentOccurrence in occurrences)
            {
                StandardAddInServer.Instance.ChildHighlight.AddItem(componentOccurrence);
            }
        }

        public static List<ComponentOccurrence> GetComponentOccurrencesFromNodes(List<RigidNode_Base> nodes)
        {
            if (nodes == null)
            {
                return null;
            }

            // Get all node ID's
            List<string> nodeIDs = new List<string>();
            ;
            foreach (RigidNode_Base node in nodes)
                nodeIDs.AddRange(node.GetModelID().Split(new String[] {"-_-"}, StringSplitOptions.RemoveEmptyEntries));

            // Select all nodes
            List<ComponentOccurrence> occurrences = new List<ComponentOccurrence>();
            foreach (string id in nodeIDs)
            {
                ComponentOccurrence occurrence = GetOccurrence(id);

                if (occurrence != null)
                {
                    occurrences.Add(occurrence);
                }
            }

            return occurrences;
        }

        /// <summary>
        /// Public method used to select a node.
        /// </summary>
        /// <param name="node">Node to select.</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        public static void FocusAndHighlightNode(RigidNode_Base node, Camera camera, double zoom)
        {
            FocusAndHighlightNodes(new List<RigidNode_Base> {node}, camera, zoom);
        }
        
        /// <summary>
        /// Public method used to select a node.
        /// </summary>
        /// <param name="node">Node to select.</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        public static void FocusAndHighlightNode(RigidNode_Base node, Camera camera)
        {
            FocusAndHighlightNodes(new List<RigidNode_Base> {node}, camera);
        }

        /// <summary>
        /// Gets the <see cref="ComponentOccurrence"/> of the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ComponentOccurrence GetOccurrence(string name)
        {
            foreach (ComponentOccurrence component in StandardAddInServer.Instance.AsmDocument.ComponentDefinition.Occurrences)
            {
                if (component.Name == name)
                    return component;
            }

            return null;
        }

        /// <summary>
        /// Sets the position and target of the camera.
        /// </summary>
        /// <param name="focus">Point that camera should look at.</param>
        /// <param name="viewDistance">Distance the camera should be from that point</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">Direction to view the point from.</param>
        /// <param name="animate">True to animate movement of camera.</param>
        public static void SetCameraView(Vector focus, double viewDistance, Camera camera, double zoom, StandardAddInServer.ViewDirection viewDirection = StandardAddInServer.ViewDirection.Y, bool animate = true)
        {
            camera.Fit(); // TODO: Determine model size properly
            camera.GetExtents(out var width, out var height);

            SetCameraView(focus, viewDistance, height * zoom * 3, height * zoom, camera, viewDirection, animate);
        }
        
        /// <summary>
        /// Sets the position and target of the camera.
        /// </summary> // TODO: Add support for other directions
        /// <param name="focus">Point that camera should look at.</param>
        /// <param name="viewDistance">Distance the camera should be from that point</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">Direction to view the point from.</param>
        /// <param name="animate">True to animate movement of camera.</param>
        public static void SetCameraView(Vector focus, Box boundingBox, double viewDistance, Camera camera, bool animate = true)
        {
            var boxVector = boundingBox.MinPoint.VectorTo(boundingBox.MaxPoint);
            SetCameraView(focus, viewDistance, boxVector.X, boxVector.Z, camera, StandardAddInServer.ViewDirection.Y, animate);
        }
        
        /// <summary>
        /// Sets the position and target of the camera.
        /// </summary>
        /// <param name="focus">Point that camera should look at.</param>
        /// <param name="viewDistance">Distance the camera should be from that point</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">Direction to view the point from.</param>
        /// <param name="animate">True to animate movement of camera.</param>
        public static void SetCameraView(Vector focus, double viewDistance, double width, double height, Camera camera, StandardAddInServer.ViewDirection viewDirection = StandardAddInServer.ViewDirection.Y, bool animate = true)
        {
            Point focusPoint = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreatePoint(focus.X, focus.Y, focus.Z);

            camera.SetExtents(width, height);

            camera.Target = focusPoint;

            // Flip view for negative direction
            if ((viewDirection & StandardAddInServer.ViewDirection.Negative) == StandardAddInServer.ViewDirection.Negative)
                viewDistance = -viewDistance;

            UnitVector up = null;

            // Find camera position and upwards direction
            if ((viewDirection & StandardAddInServer.ViewDirection.X) == StandardAddInServer.ViewDirection.X)
            {
                focus.X += viewDistance;
                up = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreateUnitVector(0, 1, 0);
            }

            if ((viewDirection & StandardAddInServer.ViewDirection.Y) == StandardAddInServer.ViewDirection.Y)
            {
                focus.Y += viewDistance;
                up = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreateUnitVector(0, 0, 1);
            }

            if ((viewDirection & StandardAddInServer.ViewDirection.Z) == StandardAddInServer.ViewDirection.Z)
            {
                focus.Z += viewDistance;
                up = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreateUnitVector(0, 1, 0);
            }

            camera.Eye = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreatePoint(focus.X, focus.Y, focus.Z);
            camera.UpVector = up;

            // Apply settings
            if (animate)
                camera.Apply();
            else
                camera.ApplyWithoutTransition();
        }

        /// <summary>
        /// Moves the camera to the midpoint of all the specified occurrences. Used in the wizard to point out the specified occurence.
        /// </summary>
        /// <param name="occurrences">The <see cref="ComponentOccurrence"/>s for the <see cref="Camera"/> to focus on</param>
        /// <param name="viewDistance">The distence from <paramref name="occurrence"/> that the camera will be</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">The direction of the camera</param>
        /// <param name="animate">True if you want to animate the camera moving to the new position</param>
        public static void FocusCameraOnOccurrences(List<ComponentOccurrence> occurrences, double viewDistance, Camera camera, double zoom,
            StandardAddInServer.ViewDirection viewDirection = StandardAddInServer.ViewDirection.Y, bool animate = false)
        {
            if (occurrences.Count < 1)
                return;

            var translation = GetOccurrencesCenter(occurrences);

            SetCameraView(translation, viewDistance, camera, zoom, viewDirection, animate);
        }
        
        /// <summary>
        /// Moves the camera to the midpoint of all the specified occurrences. Used in the wizard to point out the specified occurence.
        /// </summary>
        /// <param name="occurrences">The <see cref="ComponentOccurrence"/>s for the <see cref="Camera"/> to focus on</param>
        /// <param name="viewDistance">The distence from <paramref name="occurrence"/> that the camera will be</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">The direction of the camera</param>
        /// <param name="animate">True if you want to animate the camera moving to the new position</param>
        public static void FocusCameraOnOccurrences(List<ComponentOccurrence> occurrences, double viewDistance, Camera camera,
            StandardAddInServer.ViewDirection viewDirection = StandardAddInServer.ViewDirection.Y, bool animate = false)
        {
            if (occurrences.Count < 1)
                return;

            var translation = GetOccurrencesCenter(occurrences);
            var combineBoundingBoxes = CombineBoundingBoxes(occurrences);

            SetCameraView(translation, combineBoundingBoxes, viewDistance, camera, animate);
        }

        public static Vector GetOccurrencesCenter(List<ComponentOccurrence> occurrences)
        {
            double xSum = 0, ySum = 0, zSum = 0;
            int i = 0;
            foreach (ComponentOccurrence occurrence in occurrences)
            {
                xSum += occurrence.Transformation.Translation.X;
                ySum += occurrence.Transformation.Translation.Y;
                zSum += occurrence.Transformation.Translation.Z;

                i++;
            }


            Vector translation = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreateVector((xSum / i), (ySum / i), (zSum / i));
            return translation;
        }

        public static Box CombineBoundingBoxes(List<ComponentOccurrence> occurrences)
        {
            var resultBox = occurrences[0].RangeBox.Copy();
            foreach (var occurrence in occurrences.GetRange(1, occurrences.Count-1))
            {
                resultBox.Extend(occurrence.RangeBox.MaxPoint);
                resultBox.Extend(occurrence.RangeBox.MinPoint);
            }
            return resultBox;
        }
    }
}