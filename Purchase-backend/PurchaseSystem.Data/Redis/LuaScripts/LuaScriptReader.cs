using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace PurchaseSystem.Data.Redis.Services
{
    public class LuaScriptReader
    {
        public static string GetLuaScript(string scriptFileName)
        {
            // 1. 获取当前类所在的程序集
            var assembly = Assembly.GetExecutingAssembly();

            // 2. 构造资源的完整名称
            // 格式：[项目默认命名空间].[文件夹路径（用点分隔）].[文件名]
            // 例如：PurchaseSystem.Data.Redis.LuaScripts.YourScript.lua
            string resourceName = $"PurchaseSystem.Data.Redis.LuaScripts.{scriptFileName}";

            // 3. 从程序集清单中读取资源流
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    // 资源未找到时，抛出异常或返回空
                    throw new FileNotFoundException($"找不到嵌入式资源 '{resourceName}'。请检查文件名和资源名称。");
                }

                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    // 4. 将流内容读取为字符串并返回
                    return reader.ReadToEnd();
                }
            }
        }
    }
}