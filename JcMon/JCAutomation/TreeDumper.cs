namespace JCAutomation
{
    using System;
    using System.Text;

    internal class TreeDumper
    {
        private readonly StringBuilder sb = new StringBuilder();

        private TreeDumper()
        {
        }

        public static string Dump(AutomationElementTree root)
        {
            TreeDumper dumper = new TreeDumper();
            dumper.Dump(root, "");
            return dumper.sb.ToString();
        }

        private void Dump(AutomationElementTree node, string indent)
        {
            this.sb.AppendLine(node.Text);
            for (int i = 0; i < node.Children.Count; i++)
            {
                bool flag = i == (node.Children.Count - 1);
                AutomationElementTree tree = node.Children[i];
                this.sb.Append(indent);
                this.sb.Append(flag ? " └" : " ├");
                this.sb.Append('─');
                this.Dump(tree, indent + (flag ? "   " : " │ "));
            }
        }
    }
}

