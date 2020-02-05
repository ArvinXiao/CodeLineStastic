using Microsoft.Extensions.Configuration;
using System;

namespace ToolStasticCodeLines
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var configuration = builder.Build();
                var stasticLineManage = new StasticLineManage(configuration);
                var result = stasticLineManage.Stastic();

                //汇总输出
                stasticLineManage.FormatOutput(result);

                Console.WriteLine("执行完成");
            }catch(Exception e)
            {
                Console.WriteLine($"程序执行时异常：{e.ToString()}");
            }

            Console.WriteLine("按任意键退出");
            Console.ReadKey();
        }
    }
}
