using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tct.ActivityRecorderClient.Forms;

namespace Tct.ActivityRecorderClient.Hotkeys
{
	[DataContract]
	public partial class HotkeySetting
	{

		private string website;

		/// <summary>
		/// Contains key code for the hotkey.
		/// </summary>
		[DataMember]
		public Keys KeyCode { get; set; }

		/// <summary>
		/// Get/set if windows modifier key is specified for the hotkey.
		/// </summary>
		[DataMember]
		public bool Shift { get; set; }

		/// <summary>
		/// Get/set if windows modifier key is specified for the hotkey.
		/// </summary>
		[DataMember]
		public bool Control { get; set; }

		/// <summary>
		/// Get/set if windows modifier key is specified for the hotkey.
		/// </summary>
		[DataMember]
		public bool Alt { get; set; }

		/// <summary>
		/// Get/set if windows modifier key is specified for the hotkey.
		/// </summary>
		[DataMember]
		public bool Windows { get; set; }

		[DataMember]
		public HotkeyActionType ActionType { get; set; }

		[DataMember]
		public int? WorkDataId { get; set; }

		[DataMember]
		public string Website
		{
			get
			{
				return website;
			}

			set
			{
				website = value;
			}
		}
	}

	[DataContract]
	public enum HotkeyActionType
	{
		[EnumMember(Value = "ResumeOrStopWork")]
		ResumeOrStopWork,
		[EnumMember(Value = "NewWorkDetectorRule")]
		NewWorkDetectorRule,
		[EnumMember(Value = "StartWork")]
		StartWork,
		[EnumMember(Value = "StartManualMeeting")]
		StartManualMeeting,
		//WorkDetails,
		[EnumMember(Value = "JobCTRL_com")]
		JobCTRL_com,
		[EnumMember(Value = "AddReason")]
		AddReason,
		[EnumMember(Value = "ToggleMenu")]
		ToggleMenu,
		[EnumMember(Value = "CreateWork")]
		CreateWork,
		[EnumMember(Value = "DeleteCurrentWorkDetectorRule")]
		DeleteCurrentWorkDetectorRule,
		[EnumMember(Value = "TodoList")]
		TodoList,
		[EnumMember(Value = "ClearAutoRuleTimer")]
		ClearAutoRuleTimer,
		[EnumMember(Value = "WorkTimeHistory")]
		WorkTimeHistory
	}

	public partial class HotkeySetting
	{
		public HotkeySetting Clone()
		{
			return new HotkeySetting()
			{
				KeyCode = this.KeyCode,
				Shift = this.Shift,
				Control = this.Control,
				Alt = this.Alt,
				Windows = this.Windows,
				ActionType = this.ActionType,
				WorkDataId = this.WorkDataId,
				website = this.website,
			};
		}

		public override string ToString()
		{
			string modifiers = "";
			if (Shift) modifiers += "Shift+";
			if (Control) modifiers += "Ctrl+";
			if (Alt) modifiers += "Alt+";
			if (Windows) modifiers += "Win+";
			var hotkeyStr = modifiers + GetNameForKey(KeyCode);

			return String.Format("HotKey: {0}, ActionType: {1}, WorkDataId: {2}", hotkeyStr, ActionType.ToString(), WorkDataId.HasValue ? WorkDataId.ToString() : "-");
		}

		private static Dictionary<Keys, string> keyAlterNames = new Dictionary<Keys, string>()
		{
			{ Keys.D0, "0" }, { Keys.D1, "1" }, { Keys.D2, "2" }, { Keys.D3, "3" }, { Keys.D4, "4" }, 
			{ Keys.D5, "5" }, { Keys.D6, "6" }, { Keys.D7, "7" }, { Keys.D8, "8" }, { Keys.D9, "9" },
			{ Keys.Add, "+" }, { Keys.Subtract, "-" }, { Keys.Multiply, "*" }, { Keys.Divide, "/" },
			{ Keys.PageDown, "PageDown" }, { Keys.Enter, "Enter" },
		};

		public static string GetNameForKey(Keys key)
		{
			return keyAlterNames.ContainsKey(key) ? keyAlterNames[key] : key.ToString();
		}

		public static List<Keys> ValidKeys = new List<Keys>()
		{
			Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,
			Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, 
			Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z,
			Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
			Keys.Insert, Keys.Delete, Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown, Keys.Space, Keys.Back, Keys.Tab,
			Keys.Add, Keys.Subtract, Keys.Multiply, Keys.Divide,
			Keys.Enter, Keys.Escape, Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Pause,
		};

		public bool IsInRange(Keys key1, Keys key2)
		{
			key1 = key1 & Keys.KeyCode;
			key2 = key2 & Keys.KeyCode;
			Keys rangeStart = key1 <= key2 ? key1 : key2;
			Keys rangeEnd = key1 <= key2 ? key2 : key1;
			return KeyCode >= rangeStart && KeyCode <= rangeEnd;
		}

		public bool HasModifier
		{
			get { return Shift || Control || Alt || Windows; }
		}

		public static bool IsWorkNeededFor(HotkeyActionType value)
		{
			return value == HotkeyActionType.StartWork;
		}

		public static bool IsWebsiteAvailableFor(HotkeyActionType value)
		{
			return value == HotkeyActionType.JobCTRL_com;
		}

		public static bool IsWorkAvailableFor(HotkeyActionType value)
		{
			return value == HotkeyActionType.StartWork
				|| value == HotkeyActionType.StartManualMeeting
				/*|| value == HotkeyActionType.WorkDetails*/;
		}

		private Keys Modifiers
		{
			get
			{
				var modifier = Keys.None;
				if (Shift)
					modifier = Keys.Shift;
				if (Control)
					modifier |= Keys.Control;
				if (Alt)
					modifier |= Keys.Alt;
				if (Windows)
					modifier |= Keys.LWin;
				return modifier;
			}
		}

		public int CompareTo(Keys code, bool isAltPressed, bool isControlPressed, bool isShiftPressed, bool isWinKeyPressed)
		{
			var modifiers = Keys.None;
			if (isShiftPressed)
				modifiers = Keys.Shift;
			if (isControlPressed)
				modifiers |= Keys.Control;
			if (isAltPressed)
				modifiers |= Keys.Alt;
			if (isWinKeyPressed)
				modifiers |= Keys.LWin;
			return KeyCode == code && Modifiers == modifiers ? 0 : 1;
		}
	}

}
