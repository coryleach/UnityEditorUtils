using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameClient.Editor
{
    
    public static class EditorGUIHelper
    {
    
        public static Rect DrawSprite(SerializedProperty spriteProperty, float maxHeight)
        {
            if (spriteProperty.objectReferenceValue == null)
            {
                return Rect.zero;
            }

            Sprite sprite = spriteProperty.objectReferenceValue as Sprite;
            if (sprite == null)
            {
                return Rect.zero;
            }
        
            var aspect = sprite.textureRect.width / sprite.textureRect.height;
        
            var width = EditorGUIUtility.currentViewWidth;
            var height = width / aspect;
            if (height > maxHeight)
            {
                width = maxHeight * aspect;
                height = maxHeight;
            }
        
            var rect = GUILayoutUtility.GetRect(width, height);
            EditorGUI.DrawRect(rect,new Color(0,0,0,0.1f));
            rect = DrawSpritePreview(rect,sprite);
            return rect;
        }
        
        public static int DrawIndexSelector(int currentIndex, int length)
        {
            EditorGUILayout.BeginHorizontal();
        
            if (GUILayout.Button("<<", GUILayout.Width(50)))
            {
                currentIndex = 0;
            }
        
            if (GUILayout.Button("<", GUILayout.Width(50)))
            {
                currentIndex--;
            }
        
            GUILayout.FlexibleSpace();
        
            currentIndex = EditorGUILayout.IntField(currentIndex, GUILayout.Width(30));
        
            EditorGUILayout.LabelField($"/{length-1}",GUILayout.Width(30));

            GUILayout.FlexibleSpace();
        
            if (GUILayout.Button(">", GUILayout.Width(50)))
            {
                currentIndex++;
            }
        
            if (GUILayout.Button(">>", GUILayout.Width(50)))
            {
                currentIndex = length-1;
            }
        
            EditorGUILayout.EndHorizontal();
        
            currentIndex = Mathf.Clamp(currentIndex,0, length - 1);
        
            return currentIndex;
        }
        
        public static Rect DrawSpritePreview(Rect rect, Sprite sprite)
        {
            Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
            Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);
 
            Rect coords = sprite.textureRect;
            coords.x /= fullSize.x;
            coords.width /= fullSize.x;
            coords.y /= fullSize.y;
            coords.height /= fullSize.y;
 
            Vector2 ratio;
            ratio.x = rect.width / size.x;
            ratio.y = rect.height / size.y;
            float minRatio = Mathf.Min(ratio.x, ratio.y);
 
            Vector2 center = rect.center;
            rect.width = size.x * minRatio;
            rect.height = size.y * minRatio;
            rect.center = center;
 
            GUI.DrawTextureWithTexCoords(rect, sprite.texture, coords);

            return rect;
        }
        
    }

}

