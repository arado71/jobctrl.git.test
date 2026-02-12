namespace JCAutomation
{
    using ICSharpCode.TextEditor;
    using ICSharpCode.TextEditor.Document;
    using Microsoft.CSharp;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Automation;
    using System.Windows.Forms;

    public class CodeEditorForm : Form
    {
        private Button btnBuild;
        private Button btnRun;
        private IContainer components;
        private const string DefaultFilter = "All files (*.*)|*.*";
        private string fileName;
        private CustomPlugin generatedPlugin;
        private string lastSavedText;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem miFile;
        private ToolStripMenuItem miNew;
        private ToolStripMenuItem miOpen;
        private ToolStripMenuItem miSave;
        private ToolStripMenuItem miSaveAs;
        private ToolStripMenuItem miShowSnippets;
        private Panel panel1;
        private TableLayoutPanel tableLayoutPanel1;
        private TextEditorControl textEditorControl;
        private TextBox txtOutput;

        public CodeEditorForm() : this(null)
        {
        }

        public CodeEditorForm(AutomationElementTree root)
        {
            this.InitializeComponent();
            this.LoadContent(null, new CSharpCodeGenerator().Generate(root));
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            this.GeneratePluginFromCode();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            this.GeneratePluginFromCode();
            if (this.generatedPlugin != null)
            {
                new PluginRunnerForm(this.generatedPlugin).Show(this);
            }
        }

        private void CodeEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.IsDirty && !this.ConfirmDiscard())
            {
                e.Cancel = true;
            }
        }

        private bool ConfirmDiscard()
	    {
		    return (MessageBox.Show("Discard changes ?", "Plugin is not saved", MessageBoxButtons.OKCancel) == DialogResult.OK);
	    }

	    protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void GeneratePluginFromCode()
        {
            Dictionary<string, string> providerOptions = new Dictionary<string, string> {
                { 
                    "CompilerVersion",
                    "v3.5"
                }
            };
            using (CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions))
            {
                CompilerParameters options = new CompilerParameters(new string[] { "System.Core.dll", "UIAutomationClient.dll", "UIAutomationTypes.dll" }) {
                    GenerateInMemory = true
                };
                CompilerResults results = provider.CompileAssemblyFromSource(options, new string[] { this.textEditorControl.Text });
                this.generatedPlugin = null;
                this.txtOutput.Clear();
                this.textEditorControl.Document.MarkerStrategy.RemoveAll(n => true);
                this.textEditorControl.Document.RequestUpdate(new TextAreaUpdate(0));
                this.textEditorControl.Document.CommitUpdate();
                if (results.Errors.HasErrors || results.Errors.HasWarnings)
                {
                    foreach (CompilerError error in results.Errors)
                    {
                        int num = (this.textEditorControl.Document.GetLineSegment(error.Line - 1).Offset + error.Column) - 1;
						this.textEditorControl.Document.MarkerStrategy.AddMarker(new TextMarker(num - 1, 2, (ICSharpCode.TextEditor.Document.TextMarkerType)3));
						this.textEditorControl.Document.RequestUpdate(new TextAreaUpdate((ICSharpCode.TextEditor.TextAreaUpdateType)1, error.Line - 1));
                        this.txtOutput.AppendText(error.ToString());
                        this.txtOutput.AppendText(Environment.NewLine);
                    }
                    this.textEditorControl.Document.CommitUpdate();
                }
                else
                {
                    var typeInfo = (from n in results.CompiledAssembly.GetTypes()
                        select new { 
                            Type = n,
                            Method = n.GetMethod("Capture", new Type[] { 
                                typeof(IntPtr),
                                typeof(int),
                                typeof(string)
                            }),
                            Ctor = n.GetConstructor(Type.EmptyTypes)
                        } into n
                        where ((n.Method != null) && (n.Method.ReturnType == typeof(AutomationElement))) && (n.Ctor != null)
                        select n).FirstOrDefault();
                    if (typeInfo == null)
                    {
                        this.txtOutput.AppendText("Cannot find type with AutomationElement Capture(IntPtr hWnd, int processId, string processName) method and default ctor.");
                        this.txtOutput.AppendText(Environment.NewLine);
                    }
                    else
                    {
                        try
                        {
                            object obj = Activator.CreateInstance(typeInfo.Type);
                            this.generatedPlugin = new CustomPlugin((hWnd, processId, processName) => (AutomationElement) typeInfo.Method.Invoke(obj, new object[] { hWnd, processId, processName }));
                            this.txtOutput.AppendText("Successfully built plugin");
                            this.txtOutput.AppendText(Environment.NewLine);
                        }
                        catch (Exception exception)
                        {
                            this.txtOutput.AppendText("Unable to instantiate type" + Environment.NewLine + exception);
                            this.txtOutput.AppendText(Environment.NewLine);
                        }
                    }
                }
            }
        }

        public CustomPlugin GetPlugin()
	    {
		    return this.generatedPlugin;
	    }

	    private void InitializeComponent()
        {
			this.textEditorControl = new ICSharpCode.TextEditor.TextEditorControl();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnBuild = new System.Windows.Forms.Button();
			this.btnRun = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.miFile = new System.Windows.Forms.ToolStripMenuItem();
			this.miNew = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.miSave = new System.Windows.Forms.ToolStripMenuItem();
			this.miSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.miShowSnippets = new System.Windows.Forms.ToolStripMenuItem();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textEditorControl
			// 
			this.textEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textEditorControl.IsReadOnly = false;
			this.textEditorControl.Location = new System.Drawing.Point(3, 33);
			this.textEditorControl.Name = "textEditorControl";
			this.textEditorControl.Size = new System.Drawing.Size(1170, 584);
			this.textEditorControl.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.txtOutput, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.textEditorControl, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1176, 670);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// txtOutput
			// 
			this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOutput.Location = new System.Drawing.Point(3, 623);
			this.txtOutput.Multiline = true;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ReadOnly = true;
			this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtOutput.Size = new System.Drawing.Size(1170, 44);
			this.txtOutput.TabIndex = 1;
			this.txtOutput.WordWrap = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.btnBuild);
			this.panel1.Controls.Add(this.btnRun);
			this.panel1.Controls.Add(this.menuStrip1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1170, 24);
			this.panel1.TabIndex = 1;
			// 
			// btnBuild
			// 
			this.btnBuild.Location = new System.Drawing.Point(134, 1);
			this.btnBuild.Name = "btnBuild";
			this.btnBuild.Size = new System.Drawing.Size(75, 23);
			this.btnBuild.TabIndex = 1;
			this.btnBuild.Text = "Build";
			this.btnBuild.UseVisualStyleBackColor = true;
			this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
			// 
			// btnRun
			// 
			this.btnRun.Location = new System.Drawing.Point(53, 1);
			this.btnRun.Name = "btnRun";
			this.btnRun.Size = new System.Drawing.Size(75, 23);
			this.btnRun.TabIndex = 0;
			this.btnRun.Text = "Run";
			this.btnRun.UseVisualStyleBackColor = true;
			this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1170, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// miFile
			// 
			this.miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miNew,
            this.miOpen,
            this.miSave,
            this.miSaveAs,
            this.miShowSnippets});
			this.miFile.Name = "miFile";
			this.miFile.Size = new System.Drawing.Size(37, 20);
			this.miFile.Text = "File";
			// 
			// miNew
			// 
			this.miNew.Name = "miNew";
			this.miNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.miNew.Size = new System.Drawing.Size(160, 22);
			this.miNew.Text = "&New";
			this.miNew.Click += new System.EventHandler(this.miNew_Click);
			// 
			// miOpen
			// 
			this.miOpen.Name = "miOpen";
			this.miOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.miOpen.Size = new System.Drawing.Size(160, 22);
			this.miOpen.Text = "&Open...";
			this.miOpen.Click += new System.EventHandler(this.miOpen_Click);
			// 
			// miSave
			// 
			this.miSave.Name = "miSave";
			this.miSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.miSave.Size = new System.Drawing.Size(160, 22);
			this.miSave.Text = "&Save";
			this.miSave.Click += new System.EventHandler(this.miSave_Click);
			// 
			// miSaveAs
			// 
			this.miSaveAs.Name = "miSaveAs";
			this.miSaveAs.Size = new System.Drawing.Size(160, 22);
			this.miSaveAs.Text = "Save As...";
			this.miSaveAs.Click += new System.EventHandler(this.miSaveAs_Click);
			// 
			// miShowSnippets
			// 
			this.miShowSnippets.Name = "miShowSnippets";
			this.miShowSnippets.Size = new System.Drawing.Size(160, 22);
			this.miShowSnippets.Text = "Show Snippets...";
			this.miShowSnippets.Click += new System.EventHandler(this.miShowSnippets_Click);
			// 
			// CodeEditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1176, 670);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "CodeEditorForm";
			this.Text = "JC Automation Code Editor";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CodeEditorForm_FormClosing);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);

        }

        public void LoadContent(string name, string content)
        {
            this.fileName = name;
            this.textEditorControl.LoadFile(name ?? "new.cs", new MemoryStream(Encoding.UTF8.GetBytes(content)), true, true);
            this.lastSavedText = this.textEditorControl.Text;
        }

        private void miNew_Click(object sender, EventArgs e)
        {
            if (!this.IsDirty || this.ConfirmDiscard())
            {
                this.LoadContent(null, new CSharpCodeGenerator().Generate(null));
            }
        }

        private void miOpen_Click(object sender, EventArgs e)
        {
            try
            {
                this.Open();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Unable to open file. " + exception);
            }
        }

        private void miSave_Click(object sender, EventArgs e)
        {
            try
            {
                this.Save();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Unable to save file. " + exception);
            }
        }

        private void miSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                this.SaveAs();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Unable to save file. " + exception);
            }
        }

        private void miShowSnippets_Click(object sender, EventArgs e)
        {
            Form form = new Form {
                Size = new Size(0x400, 700),
                Text = "JC Automation Snippets"
            };
            TextEditorControl control = new TextEditorControl {
                Dock = DockStyle.Fill
            };
            form.Controls.Add(control);
            control.LoadFile("snippets.cs", Assembly.GetExecutingAssembly().GetManifestResourceStream("JCAutomation.CSharpSnippets.cs"), true, true);
            form.Show();
        }

        private void Open()
        {
            if (!this.IsDirty || this.ConfirmDiscard())
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "C# Files (*.cs)|*.cs|All files (*.*)|*.*";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        this.LoadContent(dialog.FileName, File.ReadAllText(dialog.FileName));
                    }
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                    this.btnRun.PerformClick();
                    break;

                case Keys.F6:
                    this.btnBuild.PerformClick();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Save()
        {
            if (this.fileName == null)
            {
                this.SaveAs();
            }
            else
            {
                this.SaveContent();
            }
        }

        private void SaveAs()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.DefaultExt = "cs";
                dialog.Filter = "C# Files (*.cs)|*.cs|All files (*.*)|*.*";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                this.fileName = dialog.FileName;
            }
            this.SaveContent();
        }

        private void SaveContent()
        {
            File.WriteAllText(this.fileName, this.textEditorControl.Text);
            this.lastSavedText = this.textEditorControl.Text;
        }

        private bool IsDirty
        {
	        get { return (this.textEditorControl.Text != this.lastSavedText); }
        }
    }
}

