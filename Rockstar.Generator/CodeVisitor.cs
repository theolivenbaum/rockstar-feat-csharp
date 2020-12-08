using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rockstar.Generator
{
    public class CodeVisitor
    {
        private Stack<HashSet<string>> _scopes = new Stack<HashSet<string>>();

        public string Visit(string className, JToken tree)
        {
            _scopes.Push(new HashSet<string>());

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("namespace Rockstar");
            sb.AppendLine("{");
            sb.AppendLine("    public static class ").AppendLine(className);
            sb.AppendLine("    {");

            sb.AppendLine("        public static void LetsRock()");
            sb.AppendLine("        {");

            VisitToken(tree, sb);

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
        private void VisitToken(JToken tree, StringBuilder code)
        {
            if (tree is null) return;

            foreach (JProperty expression in tree.Children())
            {
                var type = expression.Name;
                var expr = expression.Value;

                switch (type)
                {
                    //case "action": return (tree);
                    case "list":
                        foreach (JObject expr2 in expr)
                        {
                            VisitToken(expr2, code);
                        }
                        return;
                    case "conditional":
                        code.Append("if (");
                        
                        VisitToken(expr["condition"], code);

                        code.Append(")")
                            .AppendLine("{");

                        VisitToken(expr["consequent"],code);

                        code.AppendLine("}");

                        if(expr["alternate"] is object)
                        {
                            code.AppendLine("else")
                                .AppendLine("{");

                            VisitToken(expr["alternate"], code);

                            code.AppendLine("}");
                        }
                        return;
                    case "break":
                        code.AppendLine("break;");
                        return;
                    case "continue":
                        code.AppendLine("continue;");
                        return;
                    case "return":
                        code.Append("return ");
                        VisitToken(expr["expression"].First(), code);
                        code.AppendLine(";");
                        return;
                    case "string":
                        code.Append('"').Append(expr.ToString()).Append('"');
                        return;
                    case "number":
                        code.Append(expr.ToString());
                        return;
                    case "constant":
                        var val = expr.ToString();
                        if (!string.IsNullOrEmpty(val))
                        {
                            code.Append(val);
                        }
                        else
                        {
                            code.Append("Variable.Null");
                        }
                        return;
                    case "output":
                        code.Append("System.Console.WriteLine(");
                        VisitToken(expr[0], code);
                        code.Append(");");
                        return;
                    case "listen":
                        code.Append("System.Console.ReadLine()");
                        return;
                    case "binary":
                        Binary(expr, code);
                        return;
                    case "lookup":
                        Lookup(expr, code);
                        return;
                    case "assign":
                        Assign(expr, code);
                        return;
                    //case "pronoun":
                    //    return env.lookup(env.pronoun_alias);
                    case "blank":
                        return;
                    case "rounding":
                        Rounding(expr, code);
                        return;
                    case "mutation":
                        Mutation(expr, code);
                        return;
                    case "increment":
                        Increment(expr, code);
                        return;
                    case "decrement":
                        Decrement(expr, code);
                        return;
                    case "while_loop":
                        WhileLoop(expr, code);
                        return;
                    case "until_loop":
                        UntilLoop(expr, code);
                        return;
                    case "comparison":
                        Comparison(expr, code);
                        return;
                    case "not":
                        code.Append("!(");
                        VisitToken(expr["expression"], code);
                        code.Append(")");
                        return;
                    case "function":
                        MakeLambda((string)expr["name"], (JObject)expr, code);
                        return;
                    case "call":
                        CallLambda((string)expr["name"], (JObject)expr, code);
                        return;

                    //case "enlist":
                    //    Enlist(expr, code);
                    //    return;
                    //case "delist":
                    //    Delist(expr, code);
                    //    return;

                    default:
                        throw new Exception("Sorry - I don't know how to evaluate this: " + type);
                        //if (Array.isArray(tree) && tree.length == 1) return (evaluate(tree[0], env));
                        //throw new Error("Sorry - I don't know how to evaluate this: " + JSON.stringify(tree))
                }
            }
        }

        private void Binary(JToken tree, StringBuilder code)
        {
            var op = (string)tree["op"];
            var lhs = tree["lhs"];
            var rhs = tree["rhs"];

            if (lhs is JArray arrayLhs) lhs = arrayLhs[0];
            if (rhs is JArray arrayRhs) rhs = arrayRhs[0];

            code.Append("( (");

            VisitToken(lhs, code);
            code.Append(")");

            switch (op)
            {
                case "and":
                    code.Append(" && "); break;
                case "or":
                    code.Append(" || "); break;
                case "-":
                    code.Append(" - "); break;
                case "+":
                    code.Append(" + "); break;
                case "*":
                    code.Append(" * "); break;
                case "/":
                    code.Append(" / "); break;
                default:
                    throw new Exception($"Unknown binary operator { op }");
            }

            code.Append("(");

            VisitToken(rhs, code);
            code.Append(") )");
        }

        private void Assign(JToken tree, StringBuilder code)
        {
            var target   = tree["target"];
            var variable = (string)target["variable"];
            var index    = (int?)target["index"];
            var expression = tree["expression"];

            bool needsInit = _scopes.Peek().Add(variable);

            if (index.HasValue)
            {
                throw new Exception("Missing handlign array assignments");

                if (needsInit)
                {
                    code.Append("var ");
                }
                code.Append(variable);
                code.Append("[").Append(index.Value).Append("]");
                code.Append(" = ");
                VisitToken(expression, code);
                code.AppendLine(";");
            }
            else
            {
                if (needsInit)
                {
                    code.Append("Variable ");
                }
                code.Append(variable);
                code.Append(" = ");
                VisitToken(expression, code);
                code.AppendLine(";");
            }
        }

        private string Dealias(JToken tree)
        {
            var variable = tree["variable"];
            if(variable is JObject jo && jo.ContainsKey("pronoun"))
            {
                return (string)jo["pronoun"]["pronoun_alias"];
            }
            return (string)variable;
        }

        private void Rounding(JToken tree, StringBuilder code)
        {
            var direction = (string)tree["direction"];
            var variable_name = Dealias(tree);

            switch (direction)
            {
                case "up":
                    code.Append(variable_name).Append(" = ").Append("System.Math.Ceil(").Append(variable_name).Append(");").AppendLine(); return;
                case "down":
                    code.Append(variable_name).Append(" = ").Append("System.Math.Floor(").Append(variable_name).Append(");").AppendLine(); return;
                default:
                    code.Append(variable_name).Append(" = ").Append("System.Math.Round(").Append(variable_name).Append(");").AppendLine(); return;
            }
        }

        private void Lookup(JToken tree, StringBuilder code)
        {
            code.Append(Dealias(tree));
        }

        private void Mutation(JToken tree, StringBuilder code)
        {
            throw new NotImplementedException(nameof(Mutation));

            //let source = evaluate(expr.source, env);
            //let modifier = evaluate(expr.modifier, env);
            //switch (expr.type)
            //{
            //    case "split":
            //        return source.toString().split(modifier || "");
            //    case "cast":
            //        if (typeof(source) == 'string') return parseInt(source, modifier);
            //        if (typeof(source) == 'number') return String.fromCharCode(source);
            //        throw new Error(`I don't know how to cast ${source}`);
            //    case "join":
            //        // This is a nasty hack but it avoids having to extend the entire
            //        // parser with a special additional parameter.
            //        env.FORCE_ARRAY_FLAG = true;
            //        source = evaluate(expr.source, env);
            //        if (Array.isArray(source))
            //        {
            //            let joiner = (typeof(modifier) == 'undefined' || modifier == null) ? '' : modifier;
            //            return source.join(joiner);
            //        }
            //        throw new Error("I don't know how to join that.");
            //}
        }

        private void Increment(JToken tree, StringBuilder code)
        {
            var increment_name = Dealias(tree);
            var multiple = (int)tree["multiple"];
            code.Append(increment_name).Append(" = ").Append(increment_name).Append(" + ").Append(multiple).AppendLine(";");
        }

        private void Decrement(JToken tree, StringBuilder code)
        {
            var increment_name = Dealias(tree);
            var multiple = (int)tree["multiple"];
            code.Append(increment_name).Append(" = ").Append(increment_name).Append(" - ").Append(multiple).AppendLine(";");
        }

        private void WhileLoop(JToken tree, StringBuilder code)
        {
            var condition = tree["condition"];
            var action    = tree["consequent"];

            code.Append("while (");
            VisitToken(condition, code);
            code.AppendLine(")");
            code.AppendLine("{");
            VisitToken(action, code);
            code.AppendLine("}");
        }

        private void UntilLoop(JToken tree, StringBuilder code)
        {
            var condition = tree["condition"];
            var action    = tree["consequent"];

            code.Append("while ( !(");
            VisitToken(condition, code);
            code.AppendLine(") )");
            code.AppendLine("{");
            VisitToken(action, code);
            code.AppendLine("}");
        }

        private void Comparison(JToken tree, StringBuilder code)
        {
            var lhs = tree["lhs"][0];
            var rhs = tree["rhs"][0];
            var comparator = (string)tree["comparator"];
            
            code.Append("( (");

            VisitToken(lhs, code);
            code.Append(")");

            switch (comparator)
            {
                case "eq":
                    code.Append(" == "); break;
                case "ne":
                    code.Append(" != "); break;
                case "lt":
                    code.Append(" < "); break;
                case "le":
                    code.Append(" <= "); break;
                case "ge":
                    code.Append(" >= "); break;
                case "gt":
                    code.Append(" > "); break;
                default:
                    throw new Exception($"Unknown comparison operator { comparator }");
            }

            code.Append("(");

            VisitToken(rhs, code);
            code.Append(") )");
        }

        private void MakeLambda(string name, JObject tree, StringBuilder code)
        {
            var hs = new HashSet<string>();

            var args = tree.ContainsKey("args") ? tree["args"] : null;
            var body = tree["body"];
            
            // Hacky way to determine if the current method has a return value - this is not correct for nested methods
            //  Need to process the body["list"] children all the way up to any MakeLambda and check if there is a return value then
            //  Or change it so that all methods return a value

            bool hasReturn = body.ToString().Contains("\"return\"");

            if (hasReturn)
            {
                code.Append("static Variable ");
            }
            else
            {
                code.Append("static void ");
            }

            code.Append(name).Append("(").Append(string.Join(", ", args.Select(a => $"Variable {a}"))).AppendLine(")");
            code.AppendLine("{");

            foreach(var arg in args)
            {
                hs.Add((string)arg);
            }

            _scopes.Push(hs);
            VisitToken(body, code);
            code.AppendLine("}");

            _scopes.Pop();
        }

        private void CallLambda(string name, JObject tree, StringBuilder code)
        {
            //TODO: Missing handling arrays as arguments:

            //    let func = env.lookup(expr.name);
            //    let func_result = func.apply(null, expr.args.map(arg => {
            //        env.FORCE_ARRAY_FLAG = true;
            //        let value = evaluate(arg, env);
            //        // If the arg is an array, we shallow-copy it when passing it to a function call
            //        return (value && value.map ? value.map(e => e) : value);
            //    }));
            //    return (func_result ? func_result.value : undefined);



            var args = tree.ContainsKey("args") ? tree["args"] : null;

            code.Append(name).Append("(");

            if (args is object)
            {

                foreach (var arg in args)
                {
                    VisitToken(arg, code);
                    code.Append(",");
                }
                code.Length--;
            }
            code.Append(")");
        }
    }

}
