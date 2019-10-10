using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameClient.Editor
{
        public class ListSelection : IEnumerable<int>
        {
            private List<int> indexes;

            private int? firstSelected;

            public ListSelection()
            {
                indexes = new List<int>();
            }

            public ListSelection(int[] indexes)
            {
                this.indexes = new List<int>(indexes);
            }

            public int First => indexes.Count > 0 ? indexes[0] : -1;

            public int Last => indexes.Count > 0 ? indexes[indexes.Count - 1] : -1;

            public int Length => indexes.Count;

            public int this[int index]
            {
                get => indexes[index];
                set
                {
                    int oldIndex = indexes[index];

                    indexes[index] = value;

                    if (oldIndex == firstSelected)
                    {
                        firstSelected = value;
                    }
                }
            }

            public bool Contains(int index)
            {
                return indexes.Contains(index);
            }

            public void Clear()
            {
                indexes.Clear();
                firstSelected = null;
            }

            public void SelectWhenNoAction(int index, Event evt)
            {
                if (!EditorGUI.actionKey && !evt.shift)
                {
                    Select(index);
                }
            }

            public void Select(int index)
            {
                indexes.Clear();
                indexes.Add(index);

                firstSelected = index;
            }

            public void Remove(int index)
            {
                if (indexes.Contains(index))
                {
                    indexes.Remove(index);
                }
            }

            public void AppendWithAction(int index, Event evt)
            {
                if (EditorGUI.actionKey)
                {
                    if (Contains(index))
                    {
                        Remove(index);
                    }
                    else
                    {
                        Append(index);
                        firstSelected = index;
                    }
                }
                else if (evt.shift && indexes.Count > 0 && firstSelected.HasValue)
                {
                    indexes.Clear();

                    AppendRange(firstSelected.Value, index);
                }
                else if (!Contains(index))
                {
                    Select(index);
                }
            }

            public void Sort()
            {
                if (indexes.Count > 0)
                {
                    indexes.Sort();
                }
            }

            public void Sort(System.Comparison<int> comparison)
            {
                if (indexes.Count > 0)
                {
                    indexes.Sort(comparison);
                }
            }

            public int[] ToArray()
            {
                return indexes.ToArray();
            }

            public ListSelection Clone()
            {
                ListSelection clone = new ListSelection(ToArray());
                clone.firstSelected = firstSelected;

                return clone;
            }

            internal void Trim(int min, int max)
            {
                int i = indexes.Count;

                while (--i > -1)
                {
                    int index = indexes[i];

                    if (index < min || index >= max)
                    {
                        if (index == firstSelected && i > 0)
                        {
                            firstSelected = indexes[i - 1];
                        }

                        indexes.RemoveAt(i);
                    }
                }
            }

            internal bool CanRevert(SerializedProperty list)
            {
                if (list.serializedObject.targetObjects.Length == 1)
                {
                    for (int i = 0; i < Length; i++)
                    {
                        if (list.GetArrayElementAtIndex(this[i]).isInstantiatedPrefab)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            internal void RevertValues(object userData)
            {
                SerializedProperty list = userData as SerializedProperty;

                for (int i = 0; i < Length; i++)
                {
                    SerializedProperty property = list.GetArrayElementAtIndex(this[i]);

                    if (property.isInstantiatedPrefab)
                    {
                        property.prefabOverride = false;
                    }
                }

                list.serializedObject.ApplyModifiedProperties();
                list.serializedObject.Update();

                HandleUtility.Repaint();
            }

            internal void Duplicate(SerializedProperty list)
            {
                int offset = 0;

                for (int i = 0; i < Length; i++)
                {
                    this[i] += offset;

                    list.GetArrayElementAtIndex(this[i]).DuplicateCommand();
                    list.serializedObject.ApplyModifiedProperties();
                    list.serializedObject.Update();

                    offset++;
                }

                HandleUtility.Repaint();
            }

            internal void Delete(SerializedProperty list)
            {
                Sort();

                int i = Length;

                while (--i > -1)
                {
                    list.GetArrayElementAtIndex(this[i]).DeleteCommand();
                }

                Clear();

                list.serializedObject.ApplyModifiedProperties();
                list.serializedObject.Update();

                HandleUtility.Repaint();
            }

            private void Append(int index)
            {
                if (index >= 0 && !indexes.Contains(index))
                {
                    indexes.Add(index);
                }
            }

            private void AppendRange(int from, int to)
            {
                int dir = (int) Mathf.Sign(to - from);

                if (dir != 0)
                {
                    for (int i = from; i != to; i += dir)
                    {
                        Append(i);
                    }
                }

                Append(to);
            }

            public IEnumerator<int> GetEnumerator()
            {
                return ((IEnumerable<int>) indexes).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<int>) indexes).GetEnumerator();
            }
        }

}