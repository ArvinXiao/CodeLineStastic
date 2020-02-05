using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ToolStasticCodeLines
{
    public class StasticLineManage
    {
        private StasticSettings StasticSetting;

        public StasticLineManage(IConfiguration configuration)
        {
            StasticSetting =  Utils.GetSection<StasticSettings>(configuration, "StasticSettings");
        }

        /// <summary>
        /// 统计启动，输出行数
        /// </summary>
        public List<StasticResult> Stastic()
        {
            if (!Directory.Exists(StasticSetting.FolderPath))
            {
                throw new DirectoryNotFoundException($"文件夹不存在：{StasticSetting.FolderPath}");
            }

            var directories = Directory.GetDirectories(StasticSetting.FolderPath);
            if(directories.Length <= 0)
            {
                throw new DirectoryNotFoundException($"待统计的文件路径下没有内容：{StasticSetting.FolderPath}");
            }

            var result = new List<StasticResult>();
            //根项目文件夹路径
            foreach(var dir in directories)
            {
                var dirResult = new StasticResult();
                dirResult.ProjectName = GetFolderName(dir);
                dirResult.FileSuffixLines = new Dictionary<string, int>();
                var errorBuilder = new StringBuilder();
                StasticDirLines(dir, dirResult.FileSuffixLines, errorBuilder);
                if(errorBuilder.Length < 0)
                {
                    dirResult.ErrorMsg = errorBuilder.ToString();
                }

                result.Add(dirResult);
            }

            return result;
        }

        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="stasticResults"></param>
        public void FormatOutput(List<StasticResult> stasticResults)
        {
            if(stasticResults == null || stasticResults.Count <= 0)
            {
                Console.WriteLine("没有符合条件的统计结果");
                return;
            }

            var allSuffixStastic = new Dictionary<string, int>();
            var allCount = 0;
            foreach(var stasticResult in stasticResults)
            {
                Console.WriteLine($"--------Project:{stasticResult.ProjectName}");
                if(stasticResult.FileSuffixLines.Count > 0)
                {
                    foreach(var suffix in stasticResult.FileSuffixLines.Keys)
                    {
                        Console.WriteLine($"----{suffix} 数量：{stasticResult.FileSuffixLines[suffix]}");
                        AppendFileSuffixLines(allSuffixStastic, suffix, stasticResult.FileSuffixLines[suffix]);
                        allCount += stasticResult.FileSuffixLines[suffix];
                    }
                }
                else
                {
                    Console.WriteLine($"----该项目下没有符合条件的代码记录");
                }

                if (!string.IsNullOrEmpty(stasticResult.ErrorMsg))
                {
                    Console.WriteLine($"----错误信息：{stasticResult.ErrorMsg}");
                }
            }

            Console.WriteLine($"------------------------总数量：{allCount}");
            if (allSuffixStastic.Count > 0)
            {
                Console.WriteLine($"------------------------明细信息如下：");
                foreach (var suffix in allSuffixStastic.Keys)
                {
                    Console.WriteLine($"----{suffix} 数量：{allSuffixStastic[suffix]}");
                }
            }
        }

        /// <summary>
        /// 统计单个文件中的记录行数
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private (int lines, string errorMsg) StasticFileLines(string filePath)
        {
            if (File.Exists(filePath))
            {
                int lines = 0;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (StasticSetting.FilterEmpty 
                        ? !string.IsNullOrEmpty(reader.ReadLine().Trim()) 
                        : reader.ReadLine() != null)
                    {
                        lines++;
                    }
                }

                return (lines, null);
            }
            else
            {
                return (0, $"{filePath} 不存在");
            }
        }

        /// <summary>
        /// 统计文件夹下所有文件,每种文件后缀的记录行数
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        private void StasticDirLines(string dirPath, Dictionary<string, int> lines, StringBuilder errorMsg)
        {
            var dirs = Directory.GetDirectories(dirPath);
            if(dirs.Length <= 0)
            {
                var files = Directory.GetFiles(dirPath);
                foreach(var file in files)
                {
                    var suffix = GetFileSuffix(file);
                    if (!StasticSetting.StasticFileSuffix.Contains(suffix.ToLower()))
                    {
                        continue;
                    }

                    var fileResult = StasticFileLines(file);
                    if (!string.IsNullOrEmpty(fileResult.errorMsg))
                    {
                        errorMsg.AppendLine(fileResult.errorMsg);
                    }

                    AppendFileSuffixLines(lines, suffix, fileResult.lines);
                }

                return;
            }

            if(dirs.Length > 0)
            {
                foreach(var dir in dirs)
                {
                    StasticDirLines(dir, lines, errorMsg);
                }
            }
        }

        /// <summary>
        /// 行数增加记录
        /// </summary>
        /// <param name="baseLines"></param>
        /// <param name="suffix"></param>
        /// <param name="line"></param>
        private void AppendFileSuffixLines(Dictionary<string, int> baseLines, string suffix, int line)
        {
            if (baseLines.ContainsKey(suffix))
            {
                baseLines[suffix] += line;
            }
            else
            {
                baseLines.Add(suffix, line);
            }
        }

        /// <summary>
        /// 获取文件后缀
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetFileSuffix(string filePath)
        {
            var startIndex = filePath.LastIndexOf(".") + 1;
            return filePath.Substring(startIndex, (filePath.Length - startIndex));
        }

        /// <summary>
        /// 获取文件名称
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        private string GetFolderName(string dirPath)
        {
            var startIndex = dirPath.LastIndexOf("\\") + 1;
            return dirPath.Substring(startIndex, (dirPath.Length - startIndex));
        }

        /// <summary>
        /// 配置项
        /// </summary>
        class StasticSettings
        {
            public bool FilterEmpty { get; set; }
            public string FolderPath { get; set; }
            public List<string> StasticFileSuffix { get; set; }
        }
    }
}
