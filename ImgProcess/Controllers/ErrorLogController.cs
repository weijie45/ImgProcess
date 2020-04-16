using ImgProcess.Infrastructure;
using Log;
using Service.Function.Base;
using Service.Function.Common;
using Service.Function.Extensions;
using System;
using System.Web.Mvc;

namespace ImgProcess.Controllers
{
    public class ErrorLogController : BaseController
    {
        // Fields
        private LogContext _logContext = null;

        // Properties
        public LogContext LogContext
        {
            get
            {
                // Creat
                if (_logContext == null) {
                    // Resolve
                    _logContext = new Log.LogContextModule().Create("Test");
                    if (_logContext == null) throw new InvalidOperationException("_logContextt=null");
                }

                // Return
                return _logContext;
            }
        }


        // GET: ErrorLog
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FindLog(TagsInfo tagsInfo)
        {
            var p = new Page();
            if (tagsInfo.Error) { return p.TagsError(tagsInfo.ErrorMsg); };

            string[] rtn = new string[2] { "", "Failed" };
            var tags = tagsInfo.Tags;
            var fmDate = tags.GetValue("FmDate");
            var toDate = tags.GetValue("ToDate");

            var pageSize = tags.GetValue("PageSize").ToInt(); // 每頁筆數       
            var currPage = tags.GetValue("CurrPage").ToInt(); // 目前頁數     
            var startIndex = ((currPage - 1) * pageSize);

            var data = this.LogContext.LogRepoistory.FindAllByLogDate(fmDate, toDate, startIndex, pageSize);

            // Paginatio
            var pager = new Pager();
            //pager.GetPager(tags, this.LogContext.LogRepoistory.CountAll(fmDate, toDate));

            //p.View(tags.GetValue("TargetId"), "LogView", model: new { TagId = tags.GetValue("TagId"), Pager = pager, Data = data });

            p.Input("#result1", "t", "123");
            p.Input("#result2", "h", "456");
            p.Input("#result3", "r", "789");
            p.Input("#result4", "d", "000");
            p.Input(".result5", "", "555");
            p.Area("#form1");

            rtn[1] = p.Build();


            //p.Area(tags.TryGetValue("AreaId"));

            //p.Toggle("test", true);
            //p.Run("$('#test').css('background-color','#ccc')");
            //p.Select("#test-sel", "Select Distinct Top 5 CostNo [Key], CostType [Value] From Cost", "全部", defVal: "20");

            //p.CheckBox("check-test", "d", "2,3");
            //p.Radio("radio-test", "d", "2");
            //p.CheckBox("check-test", "", "1");
            //p.Radio("radio-test", "", "1");

            //p.Input("test1", "r", "test1");
            //p.Input("test2", "d", "test2");
            //p.Input("test3", "t", "test3");
            //p.Input("test4", "h", "test4");
            //p.Input("test5", "", "test5");

            //p.View(tags.TryGetValue("TargetId"), model: new { TagId = tags.TryGetValue("TagId") });

            //rtn[1] = p.Build();


            return this.ToJsonNet(rtn);
        }


        public ActionResult InitPage(TagsInfo tagsInfo)
        {
            string[] rtn = new string[2] { "", "Failed" };
            if (tagsInfo.Error) { return Json(new string[] { tagsInfo.ErrorMsg, "" }); };

            var tags = tagsInfo.Tags;

            if (tags.Count > 0) {
                rtn[1] = tags.ToJson();
            }

            return this.ToJsonNet(rtn);
        }
    }
}