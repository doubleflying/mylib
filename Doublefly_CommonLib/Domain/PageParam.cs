using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class PageParam
    {
        public PageParam()
        {
            PageIndex = 1;
            PageSize = 15;
            IsDesc = true;
        }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int DataCount { get; set; }

        public int PageCount
        {
            get
            {
                return (DataCount / PageSize) + ((DataCount % PageSize) > 0 ? 1 : 0);
            }
        }

        public int BeginRow
        {
            get { return (PageIndex - 1) * PageSize + 1; }
        }
        public int EndRow
        {
            get { return PageIndex * PageSize; }
        }

        /// <summary>
        /// 是否根据ID降序
        /// </summary>
        public bool IsDesc { get; set; }
    }
}
