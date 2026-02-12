namespace JCAutomation
{
    using System;
    using System.Windows.Forms;

    internal class WaitCursor : IDisposable
    {
        private static int _count;

        internal WaitCursor()
        {
            if (_count == 0)
            {
                Cursor.Current = Cursors.WaitCursor;
            }
            _count++;
        }

        public void Dispose()
        {
            _count--;
            if (_count == 0)
            {
                Cursor.Current = Cursors.Default;
            }
            GC.SuppressFinalize(this);
        }
    }
}

