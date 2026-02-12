using Tct.ActivityRecorderClient.Forms;

namespace Tct.ActivityRecorderClient.Hotkeys
{
	public static class MacKeyMapper
	{
		public static Hotkey FromNSEvent(NSEvent e)
		{
			var result = new Hotkey { KeyCode = KeyCodeToKeys(e.KeyCode) };

			// Map modifiers
			var flags = e.ModifierFlags;

			result.Shift = flags.HasFlag(NSEventModifierMask.ShiftKeyMask);
			result.Control = flags.HasFlag(NSEventModifierMask.ControlKeyMask);
			result.Alt = flags.HasFlag(NSEventModifierMask.AlternateKeyMask);
			result.Windows = flags.HasFlag(NSEventModifierMask.CommandKeyMask);

			return result;
		}

		private static Keys KeyCodeToKeys(ushort keyCode)
		{
			return keyCode switch
			{
				// Letters
				0x00 => Keys.A,
				0x0B => Keys.B,
				0x08 => Keys.C,
				0x02 => Keys.D,
				0x0E => Keys.E,
				0x03 => Keys.F,
				0x05 => Keys.G,
				0x04 => Keys.H,
				0x22 => Keys.I,
				0x26 => Keys.J,
				0x28 => Keys.K,
				0x25 => Keys.L,
				0x2E => Keys.M,
				0x2D => Keys.N,
				0x1F => Keys.O,
				0x23 => Keys.P,
				0x0C => Keys.Q,
				0x0F => Keys.R,
				0x01 => Keys.S,
				0x11 => Keys.T,
				0x20 => Keys.U,
				0x09 => Keys.V,
				0x0D => Keys.W,
				0x07 => Keys.X,
				0x10 => Keys.Y,
				0x06 => Keys.Z,

				// Numbers (top row)
				0x12 => Keys.D1,
				0x13 => Keys.D2,
				0x14 => Keys.D3,
				0x15 => Keys.D4,
				0x17 => Keys.D5,
				0x16 => Keys.D6,
				0x1A => Keys.D7,
				0x1C => Keys.D8,
				0x19 => Keys.D9,
				0x1D => Keys.D0,

				// Enter, Escape, Space, etc.
				0x24 => Keys.Enter,
				0x35 => Keys.Escape,
				0x31 => Keys.Space,
				0x33 => Keys.Back,
				0x30 => Keys.Tab,
				0x39 => Keys.CapsLock,

				// Arrows
				0x7B => Keys.Left,
				0x7C => Keys.Right,
				0x7D => Keys.Down,
				0x7E => Keys.Up,

				// Function keys
				0x7A => Keys.F1,
				0x78 => Keys.F2,
				0x63 => Keys.F3,
				0x76 => Keys.F4,
				0x60 => Keys.F5,
				0x61 => Keys.F6,
				0x62 => Keys.F7,
				0x64 => Keys.F8,
				0x65 => Keys.F9,
				0x6D => Keys.F10,
				0x67 => Keys.F11,
				0x6F => Keys.F12,

				_ => Keys.None
			};
		}
	}
}
