using Log;
using System;
using System.IO;
using System.Web.Hosting;

namespace FilesIO
{
    public class Files
    {
        // Fields
        private static LogContext _logContext = null;

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

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="path">檔案路徑</param>
        /// <param name="isDelete">刪除檔案</param>
        /// <returns></returns>
        public static byte[] ReadBinaryFile(string path, bool isDelete = false)
        {
            byte[] aBuffer = new byte[] { 0 };
            if (path.Equals(string.Empty)) return aBuffer;

            try {
                //path = HostingEnvironment.MapPath(path);
                aBuffer = File.ReadAllBytes(path);

                if (isDelete) DelFile(path);

            } catch (Exception e) {
                _logContext.LogRepoistory.SysLog(e);
            }

            return aBuffer;
        }

        /// <summary>
        /// 刪除檔案
        /// </summary>
        /// <param name="path">檔案路徑</param>
        /// <returns></returns>
        public static bool DelFile(string path)
        {
            if (path.Equals(string.Empty)) return false;
            if (!File.Exists(path)) return true;

            try {
                File.Delete(path);
            } catch (Exception e) {
                _logContext.LogRepoistory.SysLog(e);
            }

            return !File.Exists(path);
        }

    }
}
