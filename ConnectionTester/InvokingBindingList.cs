using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConnectionTester
{
    public class InvokingBindingList<T> : BindingList<T>
    {
        public InvokingBindingList(IList<T> list, Control control = null)
            : base(list)
        {
            this.Control = control;
        }

        public InvokingBindingList(Control control = null)
        {
            this.Control = control;
        }

        public Control Control { get; set; }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
        if (Control != null && Control.InvokeRequired)
            Control.Invoke(new Action(() => base.OnListChanged(e)));
        else
            base.OnListChanged(e);
        }
    }
}
