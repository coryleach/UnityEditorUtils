using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameClient.Editor
{
    public class SlideGroup
    {
        private Dictionary<int, Rect> animIDs;

        public SlideGroup()
        {
            animIDs = new Dictionary<int, Rect>();
        }

        public Rect GetRect(int id, Rect r, float easing)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return r;
            }

            if (!animIDs.ContainsKey(id))
            {
                animIDs.Add(id, r);
                return r;
            }
            else
            {
                Rect rect = animIDs[id];

                if (rect.y != r.y)
                {
                    float delta = r.y - rect.y;
                    float absDelta = Mathf.Abs(delta);

                    //if the distance between current rect and target is too large, then move the element towards the target rect so it reaches the destination faster

                    if (absDelta > (rect.height * 2))
                    {
                        r.y = delta > 0 ? r.y - rect.height : r.y + rect.height;
                    }
                    else if (absDelta > 0.5)
                    {
                        r.y = Mathf.Lerp(rect.y, r.y, easing);
                    }

                    animIDs[id] = r;
                    HandleUtility.Repaint();
                }

                return r;
            }
        }

        public Rect SetRect(int id, Rect rect)
        {
            if (animIDs.ContainsKey(id))
            {
                animIDs[id] = rect;
            }
            else
            {
                animIDs.Add(id, rect);
            }

            return rect;
        }
    }

}