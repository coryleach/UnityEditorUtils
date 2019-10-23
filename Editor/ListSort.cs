using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameframe.EditorUtils
{
    public static class ListSort
    {
        private delegate int SortComparision(SerializedProperty p1, SerializedProperty p2);

        internal static void SortOnProperty(SerializedProperty list, int length, bool descending,
            string propertyName)
        {
            BubbleSort(list, length, (p1, p2) =>
            {
                SerializedProperty a = p1.FindPropertyRelative(propertyName);
                SerializedProperty b = p2.FindPropertyRelative(propertyName);

                if (a != null && b != null && a.propertyType == b.propertyType)
                {
                    int comparison = Compare(a, b, descending, a.propertyType);

                    return descending ? -comparison : comparison;
                }

                return 0;
            });
        }

        internal static void SortOnType(SerializedProperty list, int length, bool descending,
            SerializedPropertyType type)
        {
            BubbleSort(list, length, (p1, p2) =>
            {
                int comparision = Compare(p1, p2, descending, type);

                return descending ? -comparision : comparision;
            });
        }

        private static void BubbleSort(SerializedProperty list, int length, SortComparision comparision)
        {
            for (int i = 0; i < length; i++)
            {
                SerializedProperty p1 = list.GetArrayElementAtIndex(i);

                for (int j = i + 1; j < length; j++)
                {
                    SerializedProperty p2 = list.GetArrayElementAtIndex(j);

                    if (comparision(p1, p2) > 0)
                    {
                        list.MoveArrayElement(j, i);
                    }
                }
            }
        }

        private static int Compare(SerializedProperty p1, SerializedProperty p2, bool descending,
            SerializedPropertyType type)
        {
            if (p1 == null || p2 == null)
            {
                return 0;
            }

            switch (type)
            {
                case SerializedPropertyType.Boolean:

                    return p1.boolValue.CompareTo(p2.boolValue);

                case SerializedPropertyType.Character:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:

                    return p1.longValue.CompareTo(p2.longValue);

                case SerializedPropertyType.Color:

                    return p1.colorValue.grayscale.CompareTo(p2.colorValue.grayscale);

                case SerializedPropertyType.ExposedReference:

                    return CompareObjects(p1.exposedReferenceValue, p2.exposedReferenceValue, descending);

                case SerializedPropertyType.Float:

                    return p1.doubleValue.CompareTo(p2.doubleValue);

                case SerializedPropertyType.ObjectReference:

                    return CompareObjects(p1.objectReferenceValue, p2.objectReferenceValue, descending);

                case SerializedPropertyType.String:

                    return p1.stringValue.CompareTo(p2.stringValue);

                default:

                    return 0;
            }
        }

        private static int CompareObjects(Object obj1, Object obj2, bool descending)
        {
            if (obj1 && obj2)
            {
                return obj1.name.CompareTo(obj2.name);
            }
            else if (obj1)
            {
                return descending ? 1 : -1;
            }

            return descending ? -1 : 1;
        }
    }
}