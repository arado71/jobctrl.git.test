namespace Reporter.Interfaces
{
	public interface IComputerCollectedItem : ICollectedItem
	{
		int ComputerId { get; }
	}
}
