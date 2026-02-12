namespace Reporter.Interfaces
{
	public interface IComputerWorkItem : IWorkItem
	{
		int MouseActivity { get; }
		int KeyboardActivity { get; }
		int ComputerId { get; set; }
	}
}
