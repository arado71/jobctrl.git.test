namespace JCAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Automation;

    public class AutomationElementTree
    {
        private bool? hasChildren;
        public static Action<string, Exception> LogFunc = delegate (string _, Exception __) {
        };

        public AutomationElementTree()
        {
            this.Children = new List<AutomationElementTree>();
        }

        public static AutomationElementTree CreateElement(AutomationElementInfo element)
	    {
		    return new AutomationElementTree
		    {
			    Value = element,
			    IsSelected = true
		    };
	    }

	    public static AutomationElementTree CreatePathToElement(AutomationElementInfo element)
        {
            AutomationElementTree item = new AutomationElementTree {
                Value = element,
                IsSelected = true
            };
            while (true)
            {
                AutomationElementInfo parent = item.Value.GetParent();
                if (parent == null)
                {
                    return item;
                }
                AutomationElementTree tree2 = new AutomationElementTree {
                    Value = parent
                };
                int[] runtimeId = item.Value.RuntimeId;
                AutomationElementCollection source = parent.Element.FindAll(TreeScope.Children, Condition.TrueCondition);
                bool flag = false;
                foreach (AutomationElement element2 in source.OfType<AutomationElement>())
                {
                    int[] second = element2.GetRuntimeId();
                    if (runtimeId.SequenceEqual<int>(second))
                    {
                        tree2.Children.Add(item);
                        flag = true;
                    }
                    else
                    {
                        AutomationElementTree tree3 = new AutomationElementTree {
                            Value = new AutomationElementInfo(element2)
                        };
                        tree2.Children.Add(tree3);
                    }
                }
                if (!flag)
                {
                    LogFunc("Cannot find ourself in parents", null);
                    return item;
                }
                item = tree2;
            }
        }

        public bool HasChildren()
        {
            try
            {
                bool? hasChildren = null;
                if (this.Children.Count == 0)
                {
                    hasChildren = this.hasChildren;
                }
                return (hasChildren.HasValue ? true : (this.hasChildren = new bool?(this.Value.Element.FindFirst(TreeScope.Children, Condition.TrueCondition) != null)).Value);
            }
            catch (Exception exception)
            {
                LogFunc("Cannot execute HasChildren", exception);
                return false;
            }
        }

        public void UpdateChildren()
        {
            this.Children = (from n in this.Value.Element.FindAll(TreeScope.Children, Condition.TrueCondition).OfType<AutomationElement>() select new AutomationElementTree { Value = new AutomationElementInfo(n) }).ToList<AutomationElementTree>();
        }

        public List<AutomationElementTree> Children { get; private set; }

        public bool IsSelected { get; set; }

        public string Text
        {
	        get
	        {
		        return ((this.IsSelected ? "[S] " : ((this.Children.Count > 0) ? "[X] " : "[ ] ")) +
		                this.Value.ToString(" ").Replace("\n", "").Replace("\r", ""));
	        }
        }

	    public AutomationElementInfo Value { get; private set; }
    }
}

