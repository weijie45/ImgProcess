using Service.Function.Extensions;
using System.Collections.Generic;
using Westwind.Web.Mvc;

namespace Service.Function.Common
{
    public class Pager
    {
        public string TargetID { get; set; }
        public string Func { get; set; }
        public int TotalPage { get; set; } //所有頁數
        public int TotalCnt { get; set; } //全部筆數
        public int CurrPage { get; set; } //目前頁數
        public int PageSize { get; set; } //每頁幾筆     
        public int PagerNo { get; set; } //分頁數

        public void GetPager(Dictionary<string, string> tags, int totalCnt)
        {
            Func = tags.GetValue("Func");
            TargetID = tags.GetValue("TagId");
            PageSize = tags.GetValue("PageSize").ToInt(); // 每頁筆數                 
            CurrPage = tags.GetValue("CurrPage").ToInt(); // 目前頁數     
            if (CurrPage <= 0) { CurrPage = 1; }
            if (PageSize <= 0) { PageSize = 10; }
            PagerNo = ((CurrPage - 1) * PageSize) + 1;

            TotalCnt = totalCnt;

            if ((TotalCnt % PageSize) > 0) {
                TotalPage = TotalCnt / PageSize + 1;
            } else {
                TotalPage = TotalCnt / PageSize;
            }
            if (TotalPage == 0) {
                TotalPage = 1;
            }
            if (CurrPage > TotalPage) {
                CurrPage = TotalPage;
            }

        }

        /// <summary>
        /// 取得換頁版面
        /// </summary>
        /// <param name="pagerWidth">分頁條寬度</param>
        /// <param name="remindInfo">提示文字</param>
        /// <returns></returns>
        public string RenderPage(string pageView)
        {
            return ViewRenderer.RenderPartialView(pageView, this);
        }


    }
}
