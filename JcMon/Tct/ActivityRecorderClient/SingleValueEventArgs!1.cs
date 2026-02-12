namespace Tct.ActivityRecorderClient
{
    using System;
    using System.Runtime.CompilerServices;

    public class SingleValueEventArgs<T> : EventArgs
    {
        public SingleValueEventArgs(T value)
        {
            this.Value = value;
        }

        public T Value { get; private set; }
    }
}

