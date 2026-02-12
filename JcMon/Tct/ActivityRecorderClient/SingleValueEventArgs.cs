namespace Tct.ActivityRecorderClient
{
    using System;

    public static class SingleValueEventArgs
    {
	    public static SingleValueEventArgs<T> Create<T>(T value)
	    {
		    return new SingleValueEventArgs<T>(value);
	    }
    }
}

