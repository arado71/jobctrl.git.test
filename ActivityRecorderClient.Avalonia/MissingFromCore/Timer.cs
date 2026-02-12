using System;
using System.Threading;

namespace Tct.ActivityRecorderClient.Forms;

public sealed class Timer : IDisposable
{
    private readonly SynchronizationContext _syncContext;
    private readonly object _lock = new();
    private System.Threading.Timer _timer;
    private bool _enabled;
    private int _interval = 100;
    private bool _disposed;

    public event EventHandler Tick;

    public Timer()
        : this(SynchronizationContext.Current ?? new SynchronizationContext()) { }

    public Timer(SynchronizationContext syncContext)
    {
        _syncContext = syncContext ?? throw new ArgumentNullException(nameof(syncContext));
    }

    public bool Enabled
    {
        get
        {
            lock (_lock) return _enabled;
        }
        set
        {
            lock (_lock)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(Timer));
                if (_enabled == value) return;

                _enabled = value;

                if (_enabled)
                {
                    _timer ??= new System.Threading.Timer(OnTimerElapsed, null, _interval, Timeout.Infinite);
                    _timer.Change(_interval, Timeout.Infinite);
                }
                else
                {
                    _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }
    }

    public int Interval
    {
        get
        {
            lock (_lock) return _interval;
        }
        set
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            lock (_lock)
            {
                _interval = value;
                if (_enabled && !_disposed)
                    _timer?.Change(_interval, Timeout.Infinite);
            }
        }
    }

    private void OnTimerElapsed(object state)
    {
        // Marshal callback to captured SynchronizationContext
        _syncContext.Post(_ =>
        {
            try
            {
                Tick?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                lock (_lock)
                {
                    if (_enabled && !_disposed)
                    {
                        _timer?.Change(_interval, Timeout.Infinite);
                    }
                }
            }
        }, null);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;
            _enabled = false;
            _timer?.Dispose();
            _timer = null;
        }
    }
}


