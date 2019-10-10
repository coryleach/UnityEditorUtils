using UnityEditor;
using UnityEngine;

namespace GameClient.Editor
{
    public class DraggableRect
    {
        public Rect dragArea;
        public Rect currentRect;
        public Color color = Color.red;

        private bool dragging = false;
        private Rect dragRect;
        private Vector2 previousMousePt;

        public void DoLayout()
        {
            if (Event.current.type == EventType.Layout)
            {
                return;
            }

            var drawRect = currentRect;
            drawRect.x += dragArea.x;
            drawRect.y += dragArea.y;

            if (Event.current.type == EventType.MouseDown && drawRect.Contains(Event.current.mousePosition))
            {
                dragging = true;
                dragRect = currentRect;
                previousMousePt = Event.current.mousePosition;
                Event.current.Use();
            }
            else if (dragging && Event.current.type == EventType.MouseUp)
            {
                dragging = false;
                currentRect.x = Mathf.Clamp(dragRect.x, 0, dragArea.width - currentRect.width);
                currentRect.y = Mathf.Clamp(dragRect.y, 0, dragArea.height - currentRect.height);
                Event.current.Use();
            }
            else if (dragging && Event.current.type == EventType.MouseDrag)
            {
                var mouseDelta = Event.current.mousePosition - previousMousePt;
                dragRect.center += mouseDelta;
                previousMousePt = Event.current.mousePosition;
                Event.current.Use();
            }

            if (dragging)
            {
                currentRect.x = Mathf.Clamp(dragRect.x, 0, dragArea.width - currentRect.width);
                currentRect.y = Mathf.Clamp(dragRect.y, 0, dragArea.height - currentRect.height);
            }

            drawRect = currentRect;
            drawRect.x += dragArea.x;
            drawRect.y += dragArea.y;

            EditorGUI.DrawRect(drawRect, color);
        }
    }
}