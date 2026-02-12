namespace JCAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Windows.Forms;

    public class CSharpCodeGenerator
    {
        private const string contentBegin = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Text;\r\nusing System.Windows.Automation;\r\n\r\nnamespace MyPlugin\r\n{\r\n\tpublic class MyPlugin\r\n\t{\r\n\t\tpublic AutomationElement ScriptCapture(IntPtr hWnd, int processId, string processName)\r\n\t\t{\r\n";
        private const string contentEnd = "\r\n\t\t}\r\n\t}\r\n}";
        private const string emptyCode = "\t\t\tif (!string.Equals(processName, \"something.exe\", StringComparison.OrdinalIgnoreCase)) return null;\r\n\r\n\t\t\tvar element = AutomationElement.FromHandle(hWnd);\r\n\t\t\tif (element == null) return null;\r\n\r\n\t\t\t//TODO find real target element\r\n\r\n\t\t\treturn element;";
        private const string indent = "\t\t\t";

        public string Generate(AutomationElementTree root)
        {
            if ((root != null) && (root.Value.ClassName != "#32769"))
            {
                MessageBox.Show("Cannot find root csrss process");
                return "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Text;\r\nusing System.Windows.Automation;\r\n\r\nnamespace MyPlugin\r\n{\r\n\tpublic class MyPlugin\r\n\t{\r\n\t\tpublic AutomationElement ScriptCapture(IntPtr hWnd, int processId, string processName)\r\n\t\t{\r\n\t\t\tif (!string.Equals(processName, \"something.exe\", StringComparison.OrdinalIgnoreCase)) return null;\r\n\r\n\t\t\tvar element = AutomationElement.FromHandle(hWnd);\r\n\t\t\tif (element == null) return null;\r\n\r\n\t\t\t//TODO find real target element\r\n\r\n\t\t\treturn element;\r\n\t\t}\r\n\t}\r\n}";
            }
            List<AutomationElementTree> path = GetPath(root);
            if ((path == null) || (path.Count == 0))
            {
                return "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Text;\r\nusing System.Windows.Automation;\r\n\r\nnamespace MyPlugin\r\n{\r\n\tpublic class MyPlugin\r\n\t{\r\n\t\tpublic AutomationElement ScriptCapture(IntPtr hWnd, int processId, string processName)\r\n\t\t{\r\n\t\t\tif (!string.Equals(processName, \"something.exe\", StringComparison.OrdinalIgnoreCase)) return null;\r\n\r\n\t\t\tvar element = AutomationElement.FromHandle(hWnd);\r\n\t\t\tif (element == null) return null;\r\n\r\n\t\t\t//TODO find real target element\r\n\r\n\t\t\treturn element;\r\n\t\t}\r\n\t}\r\n}";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Text;\r\nusing System.Windows.Automation;\r\n\r\nnamespace MyPlugin\r\n{\r\n\tpublic class MyPlugin\r\n\t{\r\n\t\tpublic AutomationElement ScriptCapture(IntPtr hWnd, int processId, string processName)\r\n\t\t{\r\n");
            builder.Append("\t\t\t").Append("if (!string.Equals(processName, \"").Append(path[0].Value.ProcessName).Append("\", StringComparison.OrdinalIgnoreCase)) return null;").AppendLine().AppendLine();
            builder.Append("\t\t\t").Append("var element = AutomationElement.FromHandle(hWnd);").AppendLine().Append("\t\t\t").Append("if (element == null) return null;").AppendLine().AppendLine();
            for (int i = 0; i < (path.Count - 1); i++)
            {
                int index = path[i].Children.IndexOf(path[i + 1]);
                if (index == 0)
                {
                    builder.Append("\t\t\t").Append("element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);").AppendLine().Append("\t\t\t").Append("if (element == null) return null;").AppendLine().AppendLine();
                }
                else
                {
                    builder.Append("\t\t\t").Append("var children").Append(i).Append(" = element.FindAll(TreeScope.Children, Condition.TrueCondition);").AppendLine().Append("\t\t\t").Append("element = children").Append(i).Append(".Count < ").Append((int) (index + 1)).Append(" ? null : ").Append("children").Append(i).Append("[").Append(index).Append("];").AppendLine().Append("\t\t\t").Append("if (element == null) return null;").AppendLine().AppendLine();
                }
            }
            builder.Append("\t\t\t").Append("return element;");
            builder.AppendLine("\r\n\t\t}\r\n\t}\r\n}");
            string str = TreeDumper.Dump(root);
            builder.AppendLine("/*");
            builder.Append(str.Replace("*/", "x/"));
            builder.Append("*/");
            return builder.ToString();
        }

        private static List<AutomationElementTree> GetPath(AutomationElementTree root)
        {
            if (root != null)
            {
                Queue<AutomationElementTreeWithParent> queue = new Queue<AutomationElementTreeWithParent>();
                AutomationElementTreeWithParent item = new AutomationElementTreeWithParent {
                    Value = root,
                    Parent = null
                };
                queue.Enqueue(item);
                while (queue.Count > 0)
                {
                    AutomationElementTreeWithParent parent = queue.Dequeue();
                    if (parent.Value.IsSelected)
                    {
                        List<AutomationElementTree> list = new List<AutomationElementTree> {
                            parent.Value
                        };
                        while ((parent.Parent != null) && (parent.Parent.Parent != null))
                        {
                            parent = parent.Parent;
                            list.Insert(0, parent.Value);
                        }
                        return list;
                    }
                    foreach (AutomationElementTree tree in parent.Value.Children)
                    {
                        AutomationElementTreeWithParent parent2 = new AutomationElementTreeWithParent {
                            Value = tree,
                            Parent = parent
                        };
                        queue.Enqueue(parent2);
                    }
                }
                MessageBox.Show("Cannot find the selected element");
            }
            return null;
        }

        private class AutomationElementTreeWithParent
        {
            public CSharpCodeGenerator.AutomationElementTreeWithParent Parent { get; set; }

            public AutomationElementTree Value { get; set; }
        }
    }
}

