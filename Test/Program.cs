using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("123");

            string path= HostingEnvironment.MapPath("~/Tmp/xxx.txt");
            Console.WriteLine(path);

            path = HostingEnvironment.MapPath("/Tmp/xxx.txt");
            Console.WriteLine(path);

            path = HostingEnvironment.MapPath("..");
            Console.WriteLine(path); 
            
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
            Console.ReadLine();
        }
    }
}
