namespace Tct.ActivityRecorderClient.View.Controls
{
	public interface ISelectionProvider<out T>
	{
		T Selection { get; }
	}
}