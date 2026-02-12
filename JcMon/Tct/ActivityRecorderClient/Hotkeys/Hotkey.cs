namespace Tct.ActivityRecorderClient.Hotkeys
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    public class Hotkey
    {
        public override bool Equals(object obj)
        {
            Hotkey other = obj as Hotkey;
            return ((other != null) && this.Equals(other));
        }

        public bool Equals(Hotkey other)
        {
            if (other == null)
            {
                return false;
            }
            return ((((this.KeyCode == other.KeyCode) && (this.Shift == other.Shift)) && ((this.Control == other.Control) && (this.Alt == other.Alt))) && (this.Windows == other.Windows));
        }

        public override int GetHashCode()
        {
            int num = 0x11;
            num = (0x1f * num) + this.KeyCode.GetHashCode();
            num = (0x1f * num) + this.Shift.GetHashCode();
            num = (0x1f * num) + this.Control.GetHashCode();
            num = (0x1f * num) + this.Alt.GetHashCode();
            return ((0x1f * num) + this.Windows.GetHashCode());
        }

        public override string ToString()
        {
            string str = "";
            if (this.Shift)
            {
                str = str + "Shift+";
            }
            if (this.Control)
            {
                str = str + "Ctrl+";
            }
            if (this.Alt)
            {
                str = str + "Alt+";
            }
            if (this.Windows)
            {
                str = str + "Win+";
            }
            return (str + this.KeyCode.ToString());
        }

        public bool Alt { get; set; }

        public bool Control { get; set; }

        public Keys KeyCode { get; set; }

        public bool Shift { get; set; }

        public bool Windows { get; set; }
    }
}

