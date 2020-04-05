using Log;
using System;
using System.IO;
using System.Threading;
using System.Web.Hosting;

namespace FilesIO
{
    public class Files
    {
        // Fields
        private static LogContext _logContext = null;

        // Properties
        public static LogContext LogContext
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
                path = AbsolutePath(path);
                aBuffer = File.ReadAllBytes(path);

                if (isDelete) DelFile(path);

            } catch (Exception e) {
                LogContext.LogRepoistory.SysLog(e);
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

            path = AbsolutePath(path);
            if (!File.Exists(path)) return true;

            try {
                File.Delete(path);
            } catch (Exception e) {
                LogContext.LogRepoistory.SysLog(e);
            }

            return !File.Exists(path);
        }

        /// <summary>
        /// 批次刪除
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void DelBulkFiles(string[] paths)
        {
           
            foreach (var p in paths) {
                var path = AbsolutePath(p);
          
                try {
                    File.Delete(path);
                } catch (Exception e) {
                    LogContext.LogRepoistory.SysLog(e);
                }
            }
         
        }

        /// <summary>
        /// 等待檔案沒有被鎖定
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="mode"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <returns></returns>
        public static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for (int numTries = 0; numTries < 10; numTries++) {
                FileStream fs = null;
                try {
                    fs = new FileStream(fullPath, mode, access, share);
                    return fs;
                } catch (IOException) {
                    if (fs != null) {
                        fs.Dispose();
                    }
                    Thread.Sleep(50);
                }
            }

            return null;
        }

        /// <summary>
        /// 檔案更名
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static bool RenameFile(string oldName, string newName)
        {
            oldName = AbsolutePath(oldName);
            newName = AbsolutePath(newName);
            try {
                File.Move(oldName, newName);
            } catch (Exception e) {
                LogContext.LogRepoistory.SysLog(e);
            }

            return File.Exists(newName);
        }

        private static string AbsolutePath(string path)
        {
            return  !Path.IsPathRooted(path) ? HostingEnvironment.MapPath(path) : path;
            // return path.StartsWith("~") || path.StartsWith("/") ? HostingEnvironment.MapPath(path) : path;
        }
    }
}
