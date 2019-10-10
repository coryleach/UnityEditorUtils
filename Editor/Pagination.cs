using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient.Editor
{
    public struct Pagination
    {
        public bool enabled;
        public int fixedPageSize;
        public int customPageSize;
        public int page;

        public bool UsePagination => enabled && PageSize > 0;

        public int PageSize => fixedPageSize > 0 ? fixedPageSize : customPageSize;

        public int GetVisibleLength(int total)
        {
            if (GetVisibleRange(total, out var start, out var end))
            {
                return end - start;
            }
            return total;
        }

        public int GetPageForIndex(int index)
        {
            return UsePagination ? Mathf.FloorToInt(index / (float) PageSize) : 0;
        }

        public int GetPageCount(int total)
        {
            return UsePagination ? Mathf.CeilToInt(total / (float) PageSize) : 1;
        }

        public bool GetVisibleRange(int total, out int start, out int end)
        {
            if (UsePagination)
            {
                int size = PageSize;

                start = Mathf.Clamp(page * size, 0, total - 1);
                end = Mathf.Min(start + size, total);
                return true;
            }

            start = 0;
            end = total;
            return false;
        }
    }
}