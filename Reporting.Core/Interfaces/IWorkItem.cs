namespace Reporter.Interfaces
{
	public interface IWorkItem : IInterval
	{
		int UserId { get; set; }
		int WorkId { get; }
		
	}
}
