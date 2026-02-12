namespace Tct.ActivityRecorderClient;

public static class Platform
{
    public static IPlatformFactory Factory { get; private set; }

    public static void RegisterFactory(IPlatformFactory factory) {  Factory = factory; }
}

