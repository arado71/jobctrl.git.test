using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public sealed partial class WorkGrid : UserControl, INavigator, ISelectionProvider<WorkDataWithParentNames>, IDropdownContainer, ILocalizableControl
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int LoadSize = 8;
		private const int HistorySize = 10;
		private readonly List<IDropdown> dropdowns = new List<IDropdown>();

		private readonly FixedStack<LocationKey> history = new FixedStack<LocationKey>(HistorySize);
		private int? activeSplitter = null;
		private List<WorkData> allWorks = null;
		private ClientMenuLookup clientMenu = null;
		private bool currentNavigableLeavesTrail = true;
		private NavigationBase currentNavigation = null;
		private LocationKey currentLocation = null;
		private NavigationBase.RenderHint? currentRender = null;
		private ISelectable<NavigationBase> currentSelection = null;
		private Dragger dragger = null;
		private LocationKey[] currentKeys = null;
		private Stack<LocationKey> path = new Stack<LocationKey>();
		public event EventHandler<WorkDataEventArgs> WorkClick;
		private int loadedKeysCount;
		private int currentKeysIdx;
		private int currentSizeLimit;
		private bool isControlShown;

		public event EventHandler DropdownClosed;
		public event EventHandler CreateWorkClicked;

		public NavigationFactory NavigationFactory { get; set; }

		public bool Reorderable
		{
			get { return currentNavigation.Reorderable; }
		}

		public LocationKey CurrentLocation
		{
			get { return currentLocation; }

			private set
			{
				Debug.Assert(value != null);
				if (value == null || value.Equals(currentLocation)) return;
				log.DebugFormat("CurrentLocation is set to {0}", value);
				if (currentNavigation != null)
				{
					currentNavigation.PropertyChanged -= HandleCurrentNavigationChanged;
				}

				var oldNavigation = currentNavigation;
				currentNavigation = NavigationFactory.Get(value);
				if (currentNavigation == null)
				{
					if (path.Count > 0)
					{
						path.Pop();
						Goto();
					}
					else
					{
						GoHome();
					}

					NavigationFactory.Release(oldNavigation);
					return;
				}

				currentNavigation.PropertyChanged += HandleCurrentNavigationChanged;
				currentLocation = value;
				currentNavigation.SimulateChange(oldNavigation, HandleCurrentNavigationChanged);
				NavigationFactory.Release(oldNavigation);
				pBack.Enabled = pHome.Enabled = path.Count > 1;
				pBack.BackgroundImage = pBack.Enabled ? Resources.parent : Resources.parent_inactive;
				pHome.BackgroundImage = pHome.Enabled ? Resources.home : Resources.home_inactive;
				RaiseNavigate();
			}
		}

		private static string NavigationFile
		{
			get { return "Navigation-" + ConfigManager.UserId; }
		}

		public WorkGrid()
		{
			currentNavigation = null;
			InitializeComponent();
			SetColorScheme();
			lblPath.Font = StyleUtils.GetFont(FontStyle.Bold, 9f);
			lblPath.ForeColor = StyleUtils.ForegroundLight;
			DoubleBuffered = true;
			pVisibleArea.BackColor = StyleUtils.Background;
			pVisibleArea.VerticalScroll.Enabled = true;
			pVisibleArea.VerticalScroll.Visible = false;
			scrollBar1.CloseToEnd += HandleScrollBarCloseToEnd;
			MenuQuery.Instance.ClientMenuLookup.Changed += (_, __) => SetClientMenuLookupIfNecessary();
			RefreshScroll();
			clientMenu = MenuQuery.Instance.ClientMenuLookup.Value;
			Localize();
		}

		public void Localize()
		{
			toolTip1.SetToolTip(pBack, Labels.MenuBack);
			toolTip1.SetToolTip(pHome, Labels.MenuMenu);
			toolTip1.SetToolTip(lblPath, Labels.MenuView);
			toolTip1.SetToolTip(pCreateWork, Labels.NewWork);
			currentNavigation?.Localize();
		}

		public bool DropdownShown
		{
			get { return dropdowns.Any(x => x.IsShown); }
		}

		public void RegisterDropdown(IDropdown dropdown)
		{
			dropdowns.Add(dropdown);
		}

		public event EventHandler<SingleValueEventArgs<NavigationBase>> OnNavigate;

		public void Goto(LocationKey location, bool leaveTrail = true)
		{
			if (location == null) return;

			switch (location.Type)
			{
				case LocationKey.LocationKeyType.MenuItem:
				case LocationKey.LocationKeyType.Project:
					if (currentNavigableLeavesTrail)
					{
						if (CurrentLocation != null) history.Push(CurrentLocation);
					}

					path.Push(location);
					currentNavigableLeavesTrail = leaveTrail;
					CurrentLocation = location;
					break;
				case LocationKey.LocationKeyType.Work:
					if (clientMenu.WorkDataById.ContainsKey(location.Id))
					{
						Debug.Assert(clientMenu.WorkDataById[location.Id].WorkData != null);
						currentSelection = null;
						if (TelemetryHelper.IsEnabled(TelemetryHelper.KeyWorkGridFolder) && path.Count >= 2)
						{
							TelemetryHelper.Measure(TelemetryHelper.KeyWorkGridFolder, path.ToArray()[path.Count - 2].ToString());
						}

						SwitchWork(clientMenu.WorkDataById[location.Id].WorkData, true);
						return;
					}
					else if (allWorks != null && (ConfigManager.LocalSettingsForUser.SearchInClosed || !ConfigManager.LocalSettingsForUser.SearchOwnTasks))
					{
						var cwork = SearchWork(location.Id, allWorks);
						if (cwork != null)
						{
							currentSelection = null;
							SwitchWork(cwork, false);
							return;
						}
					}

					log.DebugAndFail("Unable to navigate to work " + location.Id);
					break;
				case LocationKey.LocationKeyType.ClosedWork:
					var recents = RecentClosedHelper.GetRecents();
					var work = recents.FirstOrDefault(x => x.WorkData != null && x.WorkData.Id != null && x.WorkData.Id.Value == location.Id);
					if (work != null)
					{
						TelemetryHelper.RecordFeature("WorkGrid", "ClosedWork");
						SwitchWork(work.WorkData, false);
						return;
					}

					log.DebugAndFail("Unable to navigate to closed work " + location.Id);
					break;
			}
		}

		public void Back()
		{
			log.Debug("Navigating back");
			if (history.Count == 0) return;
			CurrentLocation = history.Pop();
		}

		private void SwitchWork(WorkData work, bool ownWork)
		{
			if (work == null) return;
			RaiseWorkClick(work, ownWork);
		}

		public WorkDataWithParentNames Selection
		{
			get
			{
				if (currentSelection == null) return null;
				var nav = currentSelection.Value as INavigationWithWork;
				if (nav == null) return null;
				return nav.Work;
			}
		}

		public void ClearSelection()
		{
			if (currentSelection == null) return;
			currentSelection.Selected = false;
			currentSelection = null;
		}

		public void GoHome()
		{
			path.Clear();
			path.Push(LocationKey.Root);
			Goto();
		}

		public void SavePath()
		{
			IsolatedStorageSerializationHelper.Save(NavigationFile, path);
		}

		public void ScrollDelta(int verticalDelta)
		{
			scrollBar1.ScrollDelta(verticalDelta);
		}

		protected override void OnParentVisibleChanged(EventArgs e)
		{
			base.OnParentVisibleChanged(e);
			if (isControlShown) return;
			isControlShown = true;
		}

		private void SetClientMenuLookupIfNecessary()
		{
			if (!isControlShown) return;
			SetClientMenuLookup(MenuQuery.Instance.ClientMenuLookup.Value);
		}

		public void SetClientMenuLookup(ClientMenuLookup menu)
		{
			clientMenu = menu;
			if (menu != null && CurrentLocation == null)
			{
				var target = LocationKey.Root;
				if (IsolatedStorageSerializationHelper.Exists(NavigationFile))
				{
					Stack<LocationKey> serialized;
					if (IsolatedStorageSerializationHelper.Load(NavigationFile, out serialized))
					{
						if (serialized == null || serialized.Count == 0)
						{
							log.Debug("Invalid menu position loaded");
							Goto(target);
						}
						else
						{
							log.DebugFormat("Menu position loaded: {0}", string.Join(" > ", serialized.Select(x => x.ToString()).ToArray()));
							path = serialized;
							Goto();
						}

						return;
					}
				}

				Goto(target);
			}
		}

		public void StartDrag(Control row, Point? startPosition = null)
		{
			log.Debug("UI - Trying to drag element");
			if (!currentNavigation.Reorderable) return;
			log.Debug("UI - Dragging element");
			TelemetryHelper.RecordFeature("WorkGrid", "Drag");
			dragger = new Dragger(row, startPosition);
			row.Visible = false;
			DragDropEffects res = DoDragDrop(new DragContainer<NavigationBase> { ParentName = Name, Content = ((ISelectable<NavigationBase>) row).Value }, DragDropEffects.Move | DragDropEffects.Copy);
			if (res == DragDropEffects.None || res == DragDropEffects.Copy)
			{
				row.Visible = true;
			}

			dragger.Dispose();
			dragger = null;
			log.Debug("UI - Element dragged");
		}

		public void Up()
		{
			if (path.Count < 2)
			{
				GoHome();
			}
			else
			{
				path.Pop();
				Goto(path.Pop());
			}
		}

		public void UpdateAllWorks(List<WorkData> allWorkData)
		{
			this.allWorks = allWorkData;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			RefreshScroll();
		}

		private void HandleCurrentNavigationChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Name":
					CurrentNameChanged();
					break;
				case "Children":
					if (currentNavigation.Children.Length == 0)
					{
						Up();
					}

					UpdateWorks(currentNavigation.Children);
					break;
			}
		}

		private void HandleDragDropped(object sender, DragEventArgs e)
		{
			log.Debug("Drag dropped");
			if (activeSplitter.HasValue && workTable.Controls.Count > activeSplitter.Value)
				((SmallSplitter) workTable.Controls[activeSplitter.Value]).Accent = false;
			activeSplitter = null;
			var dragData = e.Data.GetData(typeof(DragContainer<NavigationBase>)) as DragContainer<NavigationBase>;
			if (dragData == null) return;
			if (dragData.ParentName != Name) return;
			int dragTargetIndex = workTable.Controls.Cast<Control>().Where(x => x as ISelectable<NavigationBase> != null).TakeWhile(ctrl => e.Y >= ctrl.PointToScreen(Point.Empty).Y + ctrl.Height / 2).Count();
			int srcIndex = workTable.Controls.IndexOf(workTable.Controls.Cast<Control>().First(x => x is ISelectable<NavigationBase> && ((ISelectable<NavigationBase>) x).Value.Id == dragData.Content.Id)) / 2;
			if (dragTargetIndex > srcIndex) dragTargetIndex--;
			workTable.Controls[2 * srcIndex].Visible = true;
			Cursor = DefaultCursor;
			if (dragTargetIndex != srcIndex)
			{
				currentNavigation.Reorder(dragData.Content, dragTargetIndex);
				MoveWork(dragData.Content.Id, dragTargetIndex);
			}

			TelemetryHelper.RecordFeature("WorkGrid", "Drop");
		}

		private void HandleDragEntered(object sender, DragEventArgs e)
		{
			var dragData = e.Data.GetData(typeof(DragContainer<NavigationBase>)) as DragContainer<NavigationBase>;
			if (dragData != null && dragData.ParentName == Name)
			{
				e.Effect = DragDropEffects.Move;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void HandleDragMoved(object sender, DragEventArgs e)
		{
			int dragTargetIndex = workTable.Controls.Cast<Control>().Where(x => x as ISelectable<NavigationBase> != null).TakeWhile(ctrl => e.Y >= ctrl.PointToScreen(Point.Empty).Y + ctrl.Height / 2).Count();
			if (activeSplitter.HasValue && workTable.Controls.Count > activeSplitter.Value)
				((SmallSplitter) workTable.Controls[activeSplitter.Value]).Accent = false;
			if (dragTargetIndex == 0 || dragTargetIndex == workTable.Controls.Count)
			{
				activeSplitter = null;
				return;
			}

			if (workTable.Controls.Count > 2 * dragTargetIndex - 1)
				((SmallSplitter) workTable.Controls[2 * dragTargetIndex - 1]).Accent = true;
			activeSplitter = 2 * dragTargetIndex - 1;
		}

		private void HandleDragging(object sender, GiveFeedbackEventArgs e)
		{
			Debug.Assert(dragger != null);
			dragger.UpdateCursor(sender, e);
		}

		private void HandleHomeClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				log.Debug("UI - Home clicked");
				GoHome();
			}
			else
			{
				log.DebugFormat("UI - Home wrongly clicked with {0}", e.Button);
			}
		}

		private void HandleScrollBarCloseToEnd(object sender, EventArgs e)
		{
			LoadMoreWork();
		}

		private void HandleScrolled(object sender, EventArgs e)
		{
			ScrollTo(scrollBar1.Value);
		}

		private void HandleSelectionChanged(object sender, EventArgs e)
		{
			var sel = (ISelectable<NavigationBase>) sender;
			if (sel.Selected)
			{
				if (sel == currentSelection) return;
				if (currentSelection != null) currentSelection.Selected = false;
				currentSelection = sel;
			}
			else
			{
				if (sel != currentSelection) return;
				currentSelection = null;
			}
		}

		private void HandleUpClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				log.Debug("UI - Back clicked");
				Up();
			}
			else
			{
				log.DebugFormat("UI - Back wrongly clicked with {0}", e.Button);
			}
		}

		private void AddRow(WorkRowBase c, bool sh = true)
		{
			c.Margin = new Padding(0);
			workTable.AddControl(c, sh);
		}

		private void ChangeWorks(LocationKey[] works)
		{
			currentKeys = works;
			LoadInitialWorks();
			scrollBar1.Value = 0;
			ScrollTo(0);
		}

		private WorkRowBase CreateControl(NavigationBase nav, NavigationBase.RenderHint hint)
		{
			switch (hint)
			{
				case NavigationBase.RenderHint.Progress:
				case NavigationBase.RenderHint.Remaining:
				case NavigationBase.RenderHint.Priority:
					var ctrl = new WorkRowDetailed(this, nav, hint);
					ctrl.SelectionChanged += HandleSelectionChanged;
					return ctrl;
				case NavigationBase.RenderHint.Long:
					var ctrl2 = new WorkRowLong(this, nav);
					ctrl2.SelectionChanged += HandleSelectionChanged;
					return ctrl2;
				case NavigationBase.RenderHint.Short:
					var ctrl3 = new WorkRowShort(this, nav);
					ctrl3.SelectionChanged += HandleSelectionChanged;
					return ctrl3;
			}

			return null;
		}

		private void CurrentNameChanged()
		{
			lblPath.Text = currentNavigation.Name;
		}

		private void Goto()
		{
			if (path.Count == 0) return;
			CurrentLocation = path.Peek();
		}

		private void HandleHidden(object sender, EventArgs e)
		{
			if (!DropdownShown)
			{
				EventHandler evt = DropdownClosed;
				if (evt != null) evt(this, EventArgs.Empty);
			}
		}

		private void LoadInitialWorks()
		{
			SuspendLayout();
			pVisibleArea.SuspendLayout();
			workTable.SuspendLayout();
			var oldNavigationBase = workTable.GetControls().Select(x => x.Navigation).ToArray();
			try
			{
				if (currentRender.HasValue && currentNavigation.Render != currentRender.Value)
				{
					workTable.ClearControls();
				}

				var controls = workTable.GetControls().ToArray();

				loadedKeysCount = 0;
				currentKeysIdx = 0;
				currentSizeLimit = LoadSize;
				while (loadedKeysCount < currentSizeLimit && currentKeysIdx < currentKeys.Length)
				{
					var newNavigation = NavigationFactory.Get(currentKeys[currentKeysIdx++]);
					if (newNavigation == null) continue;
					if (loadedKeysCount < controls.Length)
					{
						controls[loadedKeysCount].Navigation = newNavigation;
					}
					else
					{
						var taskRow = CreateControl(newNavigation, currentNavigation.Render);
						AddRow(taskRow, false);
					}

					loadedKeysCount++;
				}

				if (workTable.Count > loadedKeysCount)
				{
					workTable.Trim(loadedKeysCount);
				}

				workTable.ResumeLayout(true);
				pVisibleArea.ResumeLayout(true);
				ResumeLayout(true);

				currentRender = currentNavigation.Render;
				RefreshScroll();
				log.Debug("Works loaded");
			}
			finally
			{
				Array.ForEach(oldNavigationBase, x => NavigationFactory.Release(x));
			}
		}

		private void LoadMoreWork()
		{
			if (currentKeys == null || currentKeys.Count() <= loadedKeysCount)
			{
				return;
			}

			SuspendLayout();
			pVisibleArea.SuspendLayout();
			workTable.SuspendLayout();

			currentSizeLimit += LoadSize;
			while (loadedKeysCount < currentSizeLimit && currentKeysIdx < currentKeys.Length)
			{
				var newNavigation = NavigationFactory.Get(currentKeys[currentKeysIdx++]);
				if (newNavigation == null) continue;
				var taskRow = CreateControl(newNavigation, currentNavigation.Render);
				AddRow(taskRow, false);
				loadedKeysCount++;
			}

			RefreshScroll();
			workTable.ResumeLayout(true);
			pVisibleArea.ResumeLayout(true);
			ResumeLayout(true);
			log.Debug("More work loaded");
		}

		private void MoveWork(int workId, int targetIndex)
		{
			SelectableControl<NavigationBase>[] ctrls = workTable.Controls.Cast<Control>().Select(x => x as SelectableControl<NavigationBase>).Where(x => x != null).ToArray();
			SelectableControl<NavigationBase> ctrl = ctrls.First(x => x.Value.Id == workId);
			int srcIndex = workTable.Controls.IndexOf(ctrl) / 2;
			NavigationBase tmp = ctrl.Value;
			if (targetIndex < srcIndex)
			{
				for (int i = srcIndex; i > targetIndex; i--)
				{
					ctrls[i].Value = ctrls[i - 1].Value;
				}

				ctrls[targetIndex].Value = tmp;
			}
			else
			{
				for (int i = srcIndex; i < targetIndex; i++)
				{
					ctrls[i].Value = ctrls[i + 1].Value;
				}

				ctrls[targetIndex].Value = tmp;
			}
		}

		private void RaiseNavigate()
		{
			EventHandler<SingleValueEventArgs<NavigationBase>> evt = OnNavigate;
			if (evt != null) evt(this, new SingleValueEventArgs<NavigationBase>(currentNavigation));
		}

		private void RaiseWorkClick(WorkData work, bool ownTask)
		{
			if (work.Id == null) return;
			EventHandler<WorkDataEventArgs> evt = WorkClick;
			if (evt != null) evt(this, new WorkDataEventArgs(work, ownTask));
		}

		private void RefreshScroll()
		{
			workTable.Height = workTable.PreferredSize.Height;
			scrollBar1.ScrollTotalSize = workTable.Height;
			scrollBar1.ScrollVisibleSize = pVisibleArea.Height;
			mainGrid.ColumnStyles[4].Width = 0;
			mainGrid.ColumnStyles[4].SizeType = pVisibleArea.Height >= workTable.Height ? SizeType.Absolute : SizeType.AutoSize;
			ScrollTo(scrollBar1.Value);
		}

		private void ScrollTo(int verticalPosition)
		{
			workTable.Location = new Point(0, -verticalPosition);
		}

		private WorkData SearchWork(int id, List<WorkData> works)
		{
			foreach (var work in works)
			{
				if (work.Id == id)
					return work;
				if (work.Children != null)
				{
					var res = SearchWork(id, work.Children);
					if (res != null)
						return res;
				}
			}

			return null;
		}

		private void UpdateWorks(LocationKey[] works)
		{
			if (currentKeys != null && (works == null || works.SequenceEqual(currentKeys)))
			{
				return;
			}

			ChangeWorks(works);
		}

		private void HandleCreateWorkClicked(object sender, MouseEventArgs e)
		{
			log.DebugFormat("UI - Create work clicked with {0}", e.Button);
			var evt = CreateWorkClicked;
			if (evt != null) evt(this, EventArgs.Empty);
		}

		public void SetColorScheme()
		{
			if (SystemInformation.HighContrast)
			{
				this.workRow1.BackColor = SystemColors.Window;
				this.workRow2.BackColor = SystemColors.Window;
				this.workRow3.BackColor = SystemColors.Window;
				this.workRow4.BackColor = SystemColors.Window;
				pHome.BackColor = Color.White;
				pBack.BackColor = Color.White;
				pCreateWork.BackColor = Color.White;
				pVisibleArea.BackColor = SystemColors.Window;
				this.BackColor = SystemColors.Window;
			}
			else
			{
				this.workRow1.BackColor = Color.White;
				this.workRow2.BackColor = Color.White;
				this.workRow3.BackColor = Color.White;
				this.workRow4.BackColor = Color.White;
				pHome.BackColor = Color.White;
				pBack.BackColor = Color.White;
				pCreateWork.BackColor = Color.White;
				pVisibleArea.BackColor = Color.White;
				this.BackColor = Color.White;
			}

			foreach (var workRowBase in workTable.Controls.OfType<WorkRowBase>())
			{
				workRowBase.SetColorScheme();
			}
		}

		private void PanelPaint(object sender, PaintEventArgs e)
		{
			if (!(sender is Panel panel)) return;
			// Draw background image even if in high contrast mode
			// See: https://stackoverflow.com/a/11110297/2295648
			e.Graphics.DrawImage(panel.BackgroundImage, (panel.Width - panel.BackgroundImage.Width) / 2, (panel.Height - panel.BackgroundImage.Height) / 2);
		}
	}
}