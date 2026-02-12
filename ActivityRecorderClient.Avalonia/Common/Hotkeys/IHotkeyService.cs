using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Forms;

namespace Tct.ActivityRecorderClient.Hotkeys
{
	public interface IHotkeyService : IDisposable
	{
		bool CanRegister(Hotkey hotkey);
		bool IsRegistered(Hotkey hotkey);
		void Register(Hotkey hotkey);
		void Unregister(Hotkey hotkey);
		event EventHandler<SingleValueEventArgs<Hotkey>> HotkeyPressed;
	}

	public class Hotkey
	{
		public Keys KeyCode { get; set; }
		public bool Shift { get; set; }
		public bool Control { get; set; }
		public bool Alt { get; set; }
		public bool Windows { get; set; }

		public override bool Equals(object obj)
		{
			var other = obj as Hotkey;
			return other != null && Equals(other);
		}

		public bool Equals(Hotkey other)
		{
			if (other == null) return false;
			return KeyCode == other.KeyCode
				&& Shift == other.Shift
				&& Control == other.Control
				&& Alt == other.Alt
				&& Windows == other.Windows;
		}

		public override int GetHashCode()
		{
			var result = 17;
			result = 31 * result + KeyCode.GetHashCode();
			result = 31 * result + Shift.GetHashCode();
			result = 31 * result + Control.GetHashCode();
			result = 31 * result + Alt.GetHashCode();
			result = 31 * result + Windows.GetHashCode();
			return result;
		}

		public override string ToString()
		{
			string modifiers = "";
			if (Shift) modifiers += "Shift+";
			if (Control) modifiers += "Ctrl+";
			if (Alt) modifiers += "Alt+";
			if (Windows) modifiers += "Win+";
			return modifiers + KeyCode.ToString();
		}
	}
}
