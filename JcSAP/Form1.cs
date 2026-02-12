using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using sapfewse;
using saprotwr.net;

namespace JcSAP
{
	public partial class Form1 : Form
	{
		private long currHash = 0L;
		private int fieldCounter = 1;
		private NodeObj _actNodeObj;

		private TreeNode actNode
		{
			set
			{
				_actNodeObj = value.Tag as NodeObj;
				if (_actNodeObj != null)
				{
					lblValue.Text = _actNodeObj.Id;
					toolTip1.SetToolTip(btnAdd, _actNodeObj.ToString());
				}
			}
		}

		public Form1()
		{
			InitializeComponent();
			Icon = Properties.Resources.JobCtrl;
		}

		private static GuiSession CreateNewSession()
		{
			CSapROTWrapper sapROTWrapper = new CSapROTWrapper();
			object SapGuilRot = sapROTWrapper.GetROTEntry("SAPGUI");
			object engine = SapGuilRot.GetType().InvokeMember("GetScriptingEngine", System.Reflection.BindingFlags.InvokeMethod,
				null, SapGuilRot, null);
			var sapGuiApp = engine as GuiApplication;
			GuiConnection connection = sapGuiApp.Children.ElementAt(0) as GuiConnection;
			GuiSession session = connection.Children.ElementAt(0) as GuiSession;
			return session;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			GenAll();
		}

		private void GenAll()
		{
			try
			{
				var session = CreateNewSession();
				var collection = session.ActiveWindow.Children;
				GenAllInt(collection);
			}
			catch (NullReferenceException)
			{
				tvControls.Nodes.Clear();
				tvControls.Nodes.Add("SAP GUI not started yet.");
			}
			catch (Exception ex)
			{
				tvControls.Nodes.Clear();
				tvControls.Nodes.Add(ex.Message);
			}
		}

		private void GenAllInt(GuiComponentCollection collection)
		{
			Invalidate();
			tvControls.Nodes.Clear();
			foreach (var child in collection)
			{
				var comp = (GuiComponent)child;
				tvControls.Nodes.Add(GenNode(comp));
			}
			TreeNode usrNode = null;
			foreach (TreeNode tn in tvControls.Nodes)
				if (tn.Text.StartsWith("usr"))
				{
					tn.ExpandAll();
					usrNode = tn;
				}
			if (usrNode != null) tvControls.SelectedNode = usrNode;
			tvControls.Select();
			currHash = GenHash(collection);
		}

		private long GenHash(GuiComponentCollection coll)
		{
			var hash = 0L;
			foreach (var one in coll)
			{
				var comp = one as GuiComponent;
				var isap = one as ISapScreenTarget;
				if (isap != null)
				{
					hash += GenHash(isap.Children);
				}
				else if (comp != null)
				{
					hash += (comp.Name + comp.Type).GetHashCode();
					var texttarget = one as ISapTextFieldTarget;
					if (texttarget != null)
						hash += texttarget.Text.GetHashCode();
					hash <<= 1;
				}
			}
			return hash;
		}

		TreeNode GenNode(GuiComponent target)
		{
			var format = string.Format("{0} [{1}]", target.Name, target.Type);
			string text = string.Empty;
			if (target is ISapStatusPaneTarget)
				text = (target as ISapStatusPaneTarget).Text;
			if (target is ISapTextFieldTarget)
				text = (target as ISapTextFieldTarget).Text;
			if (!string.IsNullOrEmpty(text.Trim()))
				format += " = \"" + text + "\"";
			var node = new TreeNode
			{
				Text = format,
				ToolTipText = "Try testing data capture",
				ImageIndex = 0,
				Tag = new NodeObj(target.Id)
			};
			dynamic isapst = null;
			if (target is ISapScreenTarget)
				isapst = target as ISapScreenTarget;
			if (target is IGuiMenubarTarget)
				isapst = target as IGuiMenubarTarget;
			if (target is ISapStatusbarTarget)
				isapst = target as ISapStatusbarTarget;
			if (target is ISapToolbarTarget)
				isapst = target as ISapToolbarTarget;
			if (target is ISapTitleBarTarget)
				isapst = target as ISapTitleBarTarget;
			if (target is IGuiMenuTarget)
				isapst = target as IGuiMenuTarget;
            if (target is ISapSimpleContainerTarget)
                isapst = target as ISapSimpleContainerTarget;
            if (target is ISapCustomControlTarget)
                isapst = target as ISapCustomControlTarget;
            if (target is ISapTabbedPane)
                isapst = target as ISapTabbedPane;
            if (target is ISapTabTarget)
                isapst = target as ISapTabTarget;
            if (target is ISapScrollContainerTarget)
                isapst = target as ISapScrollContainerTarget;
            if (isapst == null) return node;
			foreach (var child in isapst.Children)
			{
				node.Nodes.Add(GenNode((GuiComponent)child));
			}
			return node;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			try
			{
				var session = CreateNewSession();
				var collection = session.ActiveWindow.Children;
				if (GenHash(collection) == currHash) return;
				GenAllInt(collection);
			}
			catch (NullReferenceException)
			{
				tvControls.Nodes.Clear();
				tvControls.Nodes.Add("SAP GUI not started yet.");
			}
			catch (Exception ex)
			{
				tvControls.Nodes.Clear();
				tvControls.Nodes.Add(ex.Message);
			}
		}
		private void tvControls_AfterSelect(object sender, TreeViewEventArgs e)
		{
			actNode = e.Node;
		}
		private void btnCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(tbAccuValue.Text);
		}
		private void btnAdd_Click(object sender, EventArgs e)
		{
			if (lblValue.Text == string.Empty) return;
			tbAccuValue.Text += string.Format("{0}{1}:{2}",
				tbAccuValue.Text == string.Empty ? "" : ";",
				string.IsNullOrEmpty(tbName.Text) ? "field" + fieldCounter++ : tbName.Text,
				lblValue.Text);
		}
		private void btnTest_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(lblValue.Text)) return;
			if (_actNodeObj == null) return;
			List<Type> types = new List<Type> {
                typeof(GuiTextField),
                typeof(GuiTextedit),
                typeof(GuiCTextField),
                typeof(GuiLabel),
                typeof(GuiStatusbar)
                };
			try
			{
				Stopwatch sw = Stopwatch.StartNew();
				GuiSession s = CreateNewSession();
				string result = null;
				foreach (Type t in types)
				{
					try
					{
						dynamic d = s.FindById(lblValue.Text);
						result = d.Text as string;
						break;
					}
					catch (Exception ex) { }
				}
				long elapsed = sw.ElapsedMilliseconds;
				_actNodeObj.Add(elapsed);

				lblResultTime.Text = elapsed.ToString();
				lblResultValue.Text = result;
			}
			catch (Exception ex)
			{
				lblResultTime.Text = "";
				lblResultValue.Text = ex.Message;
			}
		}
	}

	internal class NodeObj : Dictionary<DateTime, long>
	{
		public string Id { get; private set; }
		public NodeObj(string id)
		{
			Id = id;
		}
		public void Add(long elapsed)
		{
			this.Add(DateTime.UtcNow, elapsed);
		}
		public override string ToString()
		{
			StringBuilder sb = this.Any()
				? new StringBuilder("History values:").AppendLine()
				: new StringBuilder(this.Count);
			foreach (var item in this.OrderByDescending(e => e.Key))
				sb.Append(item.Key).Append(" => ").Append(item.Value).AppendLine();
			return sb.ToString();
		}
	}

	static class Ext
	{
		public static object GetValue(this MemberInfo memberInfo, object forObject)
		{
			switch (memberInfo.MemberType)
			{
				case MemberTypes.Field:
					return ((FieldInfo)memberInfo).GetValue(forObject);
				case MemberTypes.Property:
					return ((PropertyInfo)memberInfo).GetValue(forObject);
				default:
					throw new NotImplementedException();
			}
		}
		public static bool HasProperty(this Type type, string name)
		{
			return type
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Any(p => p.Name == name);
		}
	}
}
