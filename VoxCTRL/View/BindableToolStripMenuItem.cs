using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VoxCTRL.View
{
	//http://www.daniweb.com/software-development/csharp/threads/255512/binding-an-application-setting-to-a-control-property
	public class BindableToolStripMenuItem : ToolStripMenuItem, IBindableComponent
	{
		#region IBindableComponent Members
		private BindingContext bindingContext;
		private ControlBindingsCollection dataBindings;

		[Browsable(false)]
		public BindingContext BindingContext
		{
			get
			{
				if (bindingContext == null)
				{
					bindingContext = new BindingContext();
				}
				return bindingContext;

			}
			set
			{
				bindingContext = value;
			}
		}
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlBindingsCollection DataBindings
		{
			get
			{
				if (dataBindings == null)
				{
					dataBindings = new ControlBindingsCollection(this);
				}
				return dataBindings;
			}
		}
		#endregion
	}
}
