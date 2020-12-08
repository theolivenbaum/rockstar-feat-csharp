using Jint;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Rockstar.Generator
{
    public static class Compiler
    {
        private static Assembly _assembly = typeof(Compiler).Assembly;

        internal static string GetResource(string name)
        {
            using var sr1 = new StreamReader(_assembly.GetManifestResourceStream($"Rockstar.Generator.Resources.{name}"));
            return sr1.ReadToEnd();
        }

        public static string ExtractTree(string code)
        {
            var engine = new Engine();
            var sb     = new StringBuilder();
            sb.Append(GetResource("parser.js")).AppendLine();
            sb.Append("let data = decodeURIComponent('").Append(code.ToJavaScriptString()).AppendLine("');");
            sb.AppendLine("let tree = rockstar.parse(data);");
            sb.AppendLine("return JSON.stringify(tree);");

            var result = engine.Execute(sb.ToString());
            var treeJson = result.GetCompletionValue().AsString();
            return treeJson;
        }

        public static string ProcessTree(string className, string treeJson)
        {
            var tree = JObject.Parse(treeJson);
            return new CodeVisitor().Visit(className, tree);
        }
    }
}
