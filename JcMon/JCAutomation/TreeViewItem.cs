namespace JCAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class TreeViewItem
    {
        private static readonly object emptyPlaceholder = new object();

        private TreeViewItem(AutomationElementTree value, TreeNode parent)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            this.Value = value;
            TreeNode node = new TreeNode(value.Value.ToString(" ")) {
                Tag = this
            };
            this.Current = node;
            this.Parent = parent;
            this.Parent.Nodes.Add(this.Current);
        }

        public static TreeNode BuildTree(AutomationElementTree value, out TreeNode selected)
        {
            TreeNode parent = new TreeNode("Root");
            Queue<TreeViewItem> queue = new Queue<TreeViewItem>();
            queue.Enqueue(new TreeViewItem(value, parent));
            selected = null;
            while (queue.Count > 0)
            {
                TreeViewItem item = queue.Dequeue();
                foreach (AutomationElementTree tree in item.Value.Children)
                {
                    TreeViewItem item2 = new TreeViewItem(tree, item.Current);
                    queue.Enqueue(item2);
                    if (tree.IsSelected)
                    {
                        selected = item2.Current;
                    }
                }
                if ((item.Value.Children.Count == 0) && item.Value.HasChildren())
                {
                    TreeNode node = new TreeNode("Cannot load children") {
                        Tag = emptyPlaceholder
                    };
                    item.Current.Nodes.Add(node);
                }
            }
            return parent;
        }

        public bool ExpandOnDemand()
        {
            if ((this.Current.Nodes.Count != 1) || (this.Current.Nodes[0].Tag != emptyPlaceholder))
            {
                return true;
            }
            this.Value.UpdateChildren();
            this.Current.Nodes.Clear();
            foreach (AutomationElementTree tree in this.Value.Children)
            {
                TreeViewItem item = new TreeViewItem(tree, this.Current);
                if (item.Value.HasChildren())
                {
                    TreeNode node = new TreeNode("Cannot load children") {
                        Tag = emptyPlaceholder
                    };
                    item.Current.Nodes.Add(node);
                }
            }
            return (this.Value.Children.Count != 0);
        }

        public void Refresh(bool withChildren)
        {
            this.Value.Value.RefreshInfo();
            this.Current.Text = this.Value.Value.ToString(" ");
            if (withChildren && ((this.Current.Nodes.Count != 1) || (this.Current.Nodes[0].Tag != emptyPlaceholder)))
            {
                foreach (TreeNode node in this.Current.Nodes.OfType<TreeNode>())
                {
                    TreeViewItem tag = (TreeViewItem) node.Tag;
                    tag.Parent = null;
                }
                this.Current.Nodes.Clear();
                TreeNode node2 = new TreeNode("Cannot load children") {
                    Tag = emptyPlaceholder
                };
                this.Current.Nodes.Add(node2);
                this.ExpandOnDemand();
            }
        }

        public TreeNode Current { get; set; }

        public TreeNode Parent { get; set; }

        public AutomationElementTree Value { get; set; }
    }
}

