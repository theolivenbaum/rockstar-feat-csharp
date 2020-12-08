using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rockstar.Generator
{
    [Generator]
    public class RockstarGenerator : ISourceGenerator
    {
        private static Regex RE_RockstarMarkers = new Regex(@"\/\*[\s*].*?let's rock with (([^!\s]|[\s])*)[*!]*((.|[\s])*?)\**?\/", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxTrees = context.Compilation.SyntaxTrees;

            var dumpFolder = Path.Combine(Path.GetTempPath(), "rockstar-compiler");
            
            Directory.CreateDirectory(dumpFolder);

            var count = 0;

            try
            {
                foreach (var tree in syntaxTrees)
                {
                    string fileName = Path.GetFileNameWithoutExtension(tree.FilePath);

                    var found = new Dictionary<string, string>();
                    foreach (var trivia in tree.GetRoot().DescendantTrivia())
                    {
                        if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                        {
                            var text = trivia.ToString();
                            var match = RE_RockstarMarkers.Match(text);
                            if (match.Success)
                            {
                                found[string.Join("", match.Groups[1].Value.Split(new[] { ' ', '.', '!', ',' }, StringSplitOptions.RemoveEmptyEntries))] = match.Groups[3].Value + "\n";
                            }
                        }
                    }
                    foreach (var kv in found)
                    {
                        if (string.IsNullOrWhiteSpace(kv.Value)) continue;

                        File.WriteAllText(Path.Combine(dumpFolder, fileName + kv.Key + ".rocks"), kv.Value);

                        var treeJson = Compiler.ExtractTree(kv.Value);

                        File.WriteAllText(Path.Combine(dumpFolder, fileName + kv.Key + ".json"), treeJson);

                        var compiled = Compiler.ProcessTree(ToClassName(kv.Key), treeJson);

                        File.WriteAllText(Path.Combine(dumpFolder, fileName + kv.Key + ".cs"), compiled);

                        context.AddSource($"rockstarGenerated_{++count}", SourceText.From(compiled, Encoding.UTF8));
                    }
                }

                context.AddSource($"rockstarGenerated_{++count}", SourceText.From(Compiler.GetResource("Variable.cs"), Encoding.UTF8));

            }
            catch (Exception E)
            {
                File.WriteAllText(Path.Combine(dumpFolder, "last-error.txt"), E.ToString());
                throw;
            }
        }

        private string ToClassName(string key)
        {
            if (char.IsDigit(key[0])) return "_" + key;
            return key;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //nothing to do
        }
    }
}