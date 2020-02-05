using System;
using System.Collections.Generic;
using System.Text;

namespace ToolStasticCodeLines
{
    /// <summary>
    /// 统计结果
    /// </summary>
    public class StasticResult
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 按照文件格式统计的代码行数
        /// </summary>
        public Dictionary<string, int> FileSuffixLines { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }
    }
}
