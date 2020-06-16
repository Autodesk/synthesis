using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

public class TouchMove : PanelRenderer
{
    private List<Vector2> m_StartPosition;
    private List<Vector2> m_PointerStartPosition;
    private VisualElement m_Container;

    // We support 4 simultaneous dragging, but we need to support
    // up to 8 fingers because when there is a rapid up/down sequence
    // the lifted finger id is not reused.
    private static readonly int s_MaxFingers = PointerId.maxPointers - PointerId.touchPointerIdBase;
    
    public new void Start()
    {
        base.Start();

        m_StartPosition = new List<Vector2>();
        m_PointerStartPosition = new List<Vector2>();
        for (var i = 0; i < s_MaxFingers; i++)
        {
            m_StartPosition.Add(Vector2.zero);
            m_PointerStartPosition.Add(Vector2.zero);
        }
       
        m_Container = visualTree.Q(null, "container");

        visualTree.Query(null, "elem").ForEach(e => { e.RegisterCallback<PointerDownEvent>(OnPointerDown); });
        visualTree.Query(null, "elem").ForEach(e => { e.RegisterCallback<PointerMoveEvent>(OnPointerMove); });
        visualTree.Query(null, "elem").ForEach(e => { e.RegisterCallback<PointerUpEvent>(OnPointerUp); });
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.pointerType != PointerType.touch)
        {
            return;
        }

        if (evt.currentTarget == evt.target)
        {
            evt.target.CapturePointer(evt.pointerId);
            
            VisualElement ve = evt.target as VisualElement;
            ve.AddToClassList("active");

            var fingerIndex = evt.pointerId - PointerId.touchPointerIdBase;
            m_StartPosition[fingerIndex] = new Vector2(ve.resolvedStyle.left, ve.resolvedStyle.top);
            m_PointerStartPosition[fingerIndex] = evt.position;
        }    
    }
    
    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (evt.target.HasPointerCapture(evt.pointerId))
        {
            Debug.Assert(evt.currentTarget == evt.target);
            
            VisualElement ve = evt.target as VisualElement;
            
            Vector2 bounds = new Vector2(m_Container.resolvedStyle.width, m_Container.resolvedStyle.height);
            bounds -= new Vector2(ve.resolvedStyle.width, ve.resolvedStyle.height);
            
            var fingerIndex = evt.pointerId - PointerId.touchPointerIdBase;
            Vector2 p = m_StartPosition[fingerIndex] + new Vector2(evt.position.x, evt.position.y) - m_PointerStartPosition[fingerIndex];
            p = Vector2.Max(p, Vector2.zero);
            p = Vector2.Min(p, bounds);
            ve.style.left = p.x;
            ve.style.top = p.y;
        }
    }
    
    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.target.HasPointerCapture(evt.pointerId))
        {
            Debug.Assert(evt.currentTarget == evt.target);

            VisualElement ve = evt.target as VisualElement;
            ve.RemoveFromClassList("active");
            
            evt.target.ReleasePointer(evt.pointerId);
        }    
    }
}
