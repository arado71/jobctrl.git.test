using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public class DataGridViewFilterTextBoxColumn : DataGridViewTextBoxColumn, IFilterColumn
	{
		public DataGridViewFilterTextBoxColumn()
		{
			base.DefaultHeaderCellType = typeof(DataGridViewFilterColumnHeaderCell);
		}

		/// <summary>
		/// Returns the AutoFilter header cell type. This property hides the 
		/// non-virtual DefaultHeaderCellType property inherited from the 
		/// DataGridViewBand class. The inherited property is set in the 
		/// DataGridViewAutoFilterTextBoxColumn constructor. 
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Type DefaultHeaderCellType
		{
			get
			{
				return typeof(DataGridViewFilterColumnHeaderCell);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether filtering is enabled for this column. 
		/// </summary>
		[DefaultValue(true)]
		public Boolean FilteringEnabled
		{
			get
			{
				return ((DataGridViewFilterColumnHeaderCell)HeaderCell).FilteringEnabled;
			}
			set
			{
				((DataGridViewFilterColumnHeaderCell)HeaderCell).FilteringEnabled = value;
			}
		}

		public new DataGridViewFilterColumnHeaderCell HeaderCell
		{
			get { return (DataGridViewFilterColumnHeaderCell)base.HeaderCell; }
			set { base.HeaderCell = value; }
		}

		public void ApplyFilters()
		{
			HeaderCell.ApplyFilters();
		}

		public bool ShouldShowRow(int rowIndex)
		{
			return HeaderCell.ShouldShowRow(rowIndex);
		}

		public string FilterString
		{
			get { return HeaderCell.FilterString; }
			set { HeaderCell.FilterString = value; }
		}
	}
}
