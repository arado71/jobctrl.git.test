using System;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Plugins.Ocr;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace OcrSnippetsViewer
{
	public partial class Form1 : Form
	{
		class Record
		{
			public byte[] image { get; set; }
			public string content { get; set; }
			public Guid guid { get; set; }
			public string processname { get; set; }
			public DateTime createdAt { get; set; }
			public DateTime? processedAt { get; set; }
			public int userId { get; set; }
			public int ruleId { get; set; }
			public bool IsBaddata { get; set; }
			public int companyId { get; set; }
			public int quality { get; set; }
		}

		class lbItem
		{
			public string file { get; set; }
			public int index { get; set; }
			public Record record { get; set; }
		}

		private static readonly ILog log = LogManager.GetLogger(typeof(Form1));
		private static readonly FileSystemWatcher watcher = new FileSystemWatcher();
		private static readonly string conStringDev = SimpleEncoder.Decode(Properties.Settings.Default.ConnectionSettingDev);
		private static readonly string conStringLive = SimpleEncoder.Decode(Properties.Settings.Default.ConnectionSettingLive);
		private readonly SynchronizationContext context;
		private int insertedIndex;
		private int? filterRule;
		private int? filterCompany;
		private readonly string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		private string _imagPath;
		private string imagPath
		{
			set { _imagPath = value; }
			get
			{
				if (_imagPath == null) return LoadConfig("scanningPath") ?? defaultPath;
				return _imagPath;
			}
		}

		enum DataSourceEnum : int
		{
			IMAGES = 1,
			CONTENTED = 2,
			DBDEV = 3,
			DBLIVE = 4
		}

		readonly BindingList<lbItem> items = new BindingList<lbItem>();
		private string scanningPath;
		private string ScanningPath
		{
			get { return scanningPath; }
			set
			{
				scanningPath = value;
				if (ScanningType == DataSourceEnum.IMAGES)	SaveConfig("scanningPath", value);
				if (ScanningType == DataSourceEnum.DBDEV || ScanningType == DataSourceEnum.DBLIVE)
				{
					watcher.EnableRaisingEvents = false;
				}
				else
				{
					watcher.Path = scanningPath;
					watcher.EnableRaisingEvents = true;
				}
							
			}
		}
		DataSourceEnum scanningType;

		private DataSourceEnum ScanningType
		{
			set
			{
				scanningType = value;
				switch (scanningType)
				{
					case DataSourceEnum.CONTENTED:
						ScanningPath = Path.Combine(imagPath, "data");
						break;
					case DataSourceEnum.IMAGES:
						ScanningPath = imagPath;
						break;
					case DataSourceEnum.DBDEV:
						ScanningPath = conStringDev;
						break;
					case DataSourceEnum.DBLIVE:
						ScanningPath = conStringLive;
						break;
				}
			}
			get { return scanningType; }
		}

		public Form1()
		{
			context = AsyncOperationManager.SynchronizationContext;
			InitializeComponent();

			ScanningType = DataSourceEnum.IMAGES;
			lbx.DisplayMember = "file";
			watcher.Path = ScanningPath;
			watcher.EnableRaisingEvents = true;
			watcher.IncludeSubdirectories = false;
			watcher.Changed += Watcher_Changed;
			log.Info("OCR Snippet Viewer started");
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			lblPath.Text = PreparePath(ScanningPath);
			Loaditems();
			lbx.DataSource = items;
		}

		private void lbx_SelectedIndexChanged(object sender, EventArgs e)
		{
			ResetEditingArea();
			if (lbx.SelectedItem != null)
				if (scanningType == DataSourceEnum.DBDEV || scanningType == DataSourceEnum.DBLIVE)
					ItemInfo(((lbItem)lbx.SelectedItem).record);
				else
					ItemInfo(((lbItem)lbx.SelectedItem).file);
			else
			{
				label3.Text = "content";
				tbContent.Text = "no data";
				lblUserid.Text = lblCreated.Text = lblQuality.Text = lblRuleId.Text = lblCompanyId.Text = "";
				pib.Image = null;
			}
		}

		private void lbx_Refresh()
		{
			lbx.Refresh();
			lbx.Update();
		}

		private void ItemInfo(object data)
		{
			switch (ScanningType)
			{
				case DataSourceEnum.IMAGES:
					IsolatedStorageHelper.ImageData imageData;
					label3.Text = "guessed content";
					if (isoLoad(Path.Combine(ScanningPath, (string)data), out imageData))
					{
						tbContent.Text = imageData.ContentGuess;
						lblProcessName.Text = imageData.ProcessName;
						lblUserid.Text = "";
						lblRuleId.Text = imageData.RuleId.ToString();
						pib.Image = imageData.Image;
					}
					break;
				case DataSourceEnum.CONTENTED:
					Snippet loaded;
					label3.Text = "content";
					if (isoLoad(Path.Combine(ScanningPath, (string)data), out loaded))
					{
						tbContent.Text = loaded.Content;
						lblCreated.Text = loaded.CreatedAt.ToString("yyyy.MM.dd HH:mm:ss");
						lblUserid.Text = loaded.UserId.ToString();
						lblProcessName.Text = loaded.ProcessName;
						lblRuleId.Text = loaded.RuleId.ToString();
						pib.Image = byteArrayToImage(loaded.ImageData);
					}
					break;
				case DataSourceEnum.DBDEV:
				case DataSourceEnum.DBLIVE:
					label3.Text = "content";
					if (data is Record)
					{
						Record rec = data as Record;
						tbContent.Text = rec.content;
						lblCreated.Text = rec.createdAt.ToString("yyyy.MM.dd HH:mm:ss");
						lblUserid.Text = rec.userId.ToString();
						lblRuleId.Text = rec.ruleId.ToString();
						lblQuality.Text = rec.quality.ToString();
						lblProcessName.Text = rec.processname;
						lblProcessedAt.Text = rec.processedAt.HasValue ? rec.processedAt.Value.ToString("yyyy.MM.dd HH:mm:ss") : "probably new";
						cbBaddata.Checked = rec.IsBaddata;
						pib.Image = byteArrayToImage(rec.image);
						lblCompanyId.Text = rec.companyId.ToString();
						btnEdit.Enabled = true;
						tbFilterRule.Enabled = true;
						tbFilterCompany.Enabled = true;
						btnFilter.Enabled = true;
					}
					break;
			}
		}

		private Image byteArrayToImage(byte[] ba)
		{
			MemoryStream ms = new MemoryStream(ba);
			Image returnImage = Image.FromStream(ms);
			return returnImage;
		}

		private static bool isoLoad<T>(string path, out T value)
		{
			try
			{
				T obj;
				IFormatter formatter = new BinaryFormatter();
				formatter.Binder = LegacyBinder.Instance;
				Stream stream;
				stream = new FileStream(path, FileMode.Open, FileAccess.Read);
				using (stream)
				{
					if (stream.Length == 0)
					{
						try
						{
							stream.Close();
						}
						catch (Exception ex)
						{
						}
						value = default(T);
						return false;
					}
					obj = (T)formatter.Deserialize(stream);
				}
				value = obj;
				return true;
			}
			catch (Exception ex)
			{
				value = default(T);
				return false;
			}
		}

		private class LegacyBinder : SerializationBinder
		{
			public static readonly LegacyBinder Instance = new LegacyBinder();

			private LegacyBinder()
			{
			}

			public override Type BindToType(string assemblyName, string typeName)
			{
				String exeAssembly = Assembly.GetExecutingAssembly().FullName;
				return
					Type.GetType(typeName + ", " +
								 exeAssembly); //no strong name so assemName is ok (Juval Lowy): http://msdn.microsoft.com/en-us/magazine/cc163902.aspx
			}
		}
		void Watcher_Changed(object sender, FileSystemEventArgs e)
		{
			log.InfoFormat("watcher event: {0} file:{1}", e.ChangeType, e.Name);
			context.Post(_ =>
			{
				if (e.ChangeType == WatcherChangeTypes.Deleted)
				{
					var f = items.Single(g => g.file == e.Name);
					items.Remove(f);
				}
				else
				if (e.Name != "Temp")
					items.Insert(0, new lbItem
					{
						file = e.Name,
						index = insertedIndex++
					});
				if (chkKeep.Checked)
					lbx.SelectedIndex = 0;
			}, null);
		}

		private void Loaditems()
		{
			if (ScanningPath == null) throw new ArgumentNullException("ScanningPath not set");
			items.Clear();
			lblProcessedAt.Text = lblProcessName.Text = tbContent.Text = lblCreated.Text = lblUserid.Text = "";
			cbBaddata.Checked = false;
			if (ScanningType == DataSourceEnum.DBDEV || ScanningType == DataSourceEnum.DBLIVE)
				LoadDbitems(ScanningType == DataSourceEnum.DBDEV ? conStringDev : conStringLive);
			else
			{
				DirectoryInfo di = new DirectoryInfo(ScanningPath);
				var files = di.GetFiles("*");
				foreach (var f in files.OrderByDescending(e => e.CreationTime).Take(100))
					items.Add(new lbItem { file = f.Name, index = insertedIndex++ });
			}
		}

		private void LoadDbitems(string conStr)
		{
			try
			{
				using (SqlConnection con = new SqlConnection(conStr))
				{
					con.Open();
					var sqlCommand =
						"SELECT Guid, ImageData, Content, RuleId, ProcessName, CreatedAt, ProcessedAt, UserId, IsBadData, Quality, CompanyID FROM [Snippets] LEFT JOIN [User] ON UserId = Id ORDER BY CreatedAt";
					using (SqlCommand cmd = new SqlCommand(sqlCommand, con))
					{
						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.HasRows && reader.Read())
							{
								Record rec = new Record
								{
									guid = (Guid)reader["Guid"],
									image = (byte[])reader["ImageData"],
									content = (string)reader["Content"],
									createdAt = (DateTime)reader["CreatedAt"]
								};
								if (reader["ProcessedAt"] != DBNull.Value)
									rec.processedAt = (DateTime?)reader["ProcessedAt"];
								rec.ruleId = (int)reader["RuleId"];
								rec.processname = (string)reader["ProcessName"];
								rec.userId = (int)reader["UserId"];
								rec.IsBaddata = (bool)reader["IsBadData"];
								rec.companyId = (int)reader["CompanyID"];
								rec.quality = (int)reader["Quality"];
								if (filterRule != null && rec.ruleId != filterRule) { continue; }
								if (filterCompany != null && rec.companyId != filterCompany) { continue; }
								items.Add(new lbItem { file = rec.guid.ToString().Substring(0, 13), index = insertedIndex++, record = rec });
							}
						}
					}

					con.Close();
				}
			}
			catch (Exception ex)
			{
				log.Info("Cannot connect to database");
			}
		}

		private void SaveToDb(string conStr, Record snippet)
		{
			try
			{
				using (SqlConnection con = new SqlConnection(conStr))
				{
					con.Open();
					using (SqlCommand cmd =
						new SqlCommand(
							"UPDATE [Snippets] SET IsBadData = @IsBadData, Content = @Content WHERE Guid = @Guid",
							con))
					{
						cmd.Parameters.AddWithValue("@IsBadData", snippet.IsBaddata);
						cmd.Parameters.AddWithValue("@Content", snippet.content);
						cmd.Parameters.AddWithValue("@Guid", snippet.guid);
						cmd.ExecuteNonQuery();
					}

					con.Close();
				}
			}
			catch (Exception ex)
			{
				log.Info("Cannot connect to database");
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			foreach (var f in Directory.EnumerateFiles(ScanningPath))
				File.Delete(f);
			items.Clear();
			log.Info("All files were deleted from " + ScanningPath);
		}
		private void rbSrc_Click(object sender, EventArgs e)
		{
			var obj = sender as RadioButton;
			ScanningType = (DataSourceEnum)Enum.Parse(typeof(DataSourceEnum), (string)obj.Tag);
			btnClearFilter_Click(sender, e);
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			lblPath.Text = PreparePath(ScanningPath);
			Loaditems();
			lbx_Refresh();
			lbx_SelectedIndexChanged(this, EventArgs.Empty);
			lbx.Focus();
			log.Info("Path changed to " + ScanningPath);
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			var st = ScanningType;
			using (FolderBrowserDialog dlg = new FolderBrowserDialog())
			{
				dlg.SelectedPath = ScanningPath;
				dlg.ShowNewFolderButton = false;
				var res = dlg.ShowDialog();
				if (res == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
				{
					imagPath = dlg.SelectedPath;
					ScanningType = st;
					btnRefresh_Click(sender, e);
				}
			}
		}
		private string LoadConfig(string key)
		{
			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var entry = config.AppSettings.Settings[key];
			return entry == null ? null : entry.Value;
		}
		private void SaveConfig(string key, string value)
		{
			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var entry = config.AppSettings.Settings[key];
			if (entry == null)
				config.AppSettings.Settings.Add(key, value);
			else
				config.AppSettings.Settings[key].Value = value;
			config.Save(ConfigurationSaveMode.Modified);
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			btnEdit.Enabled = false;
			btnSave.Enabled = true;
			cbBaddata.Enabled = true;
			tbContent.Enabled = true;
			tbFilterRule.Enabled = false;
			tbFilterCompany.Enabled = false;
			btnFilter.Enabled = false;
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			ResetEditingArea();
			tbFilterRule.Enabled = true;
			tbFilterCompany.Enabled = true;
			btnFilter.Enabled = true;
			btnEdit.Enabled = true;
			SaveChanges();
		}

		private void SaveChanges()
		{
			var snippet = ((lbItem) lbx.SelectedItem).record;
			if (snippet != null)
			{
				snippet.IsBaddata = cbBaddata.Checked;
				snippet.content = tbContent.Text;
			}
			SaveToDb(ScanningType == DataSourceEnum.DBDEV ? conStringDev : conStringLive, snippet);
		}

		private void ResetEditingArea()
		{
			btnEdit.Enabled = false;
			btnSave.Enabled = false;
			cbBaddata.Enabled = false;
			tbContent.Enabled = false;
			tbFilterRule.Enabled = false;
			tbFilterCompany.Enabled = false;
			btnFilter.Enabled = false;
		}

		private void btnFilter_Click(object sender, EventArgs e)
		{
			if (tbFilterRule.Text != "")
			{
				filterRule = int.Parse(tbFilterRule.Text);
			}
			else
			{
				filterRule = null;
			}
			if (tbFilterCompany.Text != "")
			{
				filterCompany = int.Parse(tbFilterCompany.Text);
			}
			else
			{
				filterCompany = null;
			}
			btnRefresh_Click(sender, e);
		}

		private void tbFilterRule_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}
		}

		private void tbFilterCompany_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}

		}

		private void btnClearFilter_Click(object sender, EventArgs e)
		{
			filterRule = null;
			filterCompany = null;
			tbFilterRule.Clear();
			tbFilterCompany.Clear();
			btnRefresh_Click(sender, e);
		}

		private void tbContent_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Enter)
			{
				btnSave_Click(sender, e);
				if (lbx.SelectedIndex < lbx.Items.Count - 1)
				{
					lbx.SelectedIndex++;
					btnEdit_Click(sender, e);
					tbContent.Focus();
				}
				e.Handled = true;
			}
		}

		private string PreparePath(string path)
		{
			if (!path.Contains(";")) { return path; }
			return path.Substring(0, path.IndexOf(";", StringComparison.Ordinal));
		}
	}
}

