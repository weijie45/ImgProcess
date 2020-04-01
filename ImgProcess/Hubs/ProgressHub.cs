

using Microsoft.AspNet.SignalR;

namespace ImgProcess.Hubs
{
    public class ProgressHub : Hub
    {
        public int count = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <param name="msg"></param>
        /// <param name="connId"></param>
        public static void SendMessage(string id, double count, string msg, string connId, int counter, string src)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            count = count > 100 ? 100 : count;

            if (!string.IsNullOrEmpty(connId)) {
                hubContext.Clients.Client(connId).sendMessage(id, msg, string.Format("{0}", count.ToString("0.00")), counter,src);
            }
        }
    }

}