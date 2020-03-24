using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ImgProcess.Hubs.Startup))]


namespace ImgProcess.Hubs
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 如需如何設定應用程式的詳細資訊，請參閱  http://go.microsoft.com/fwlink/?LinkID=316888
            app.MapSignalR();
        }
    }
}