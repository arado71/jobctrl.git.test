namespace Tct.ActivityRecorderClient.Hotkeys
{
    using System;

    public interface IHotkeyService : IDisposable
    {
        event EventHandler<SingleValueEventArgs<Hotkey>> HotkeyPressed;

        bool CanRegister(Hotkey hotkey);
        bool IsRegistered(Hotkey hotkey);
        void Register(Hotkey hotkey);
        void Unregister(Hotkey hotkey);
    }
}

