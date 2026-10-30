using System;
using System.Reflection;
using VRage.Plugins;
using VRage.Utils;

#if !LOCAL_BUILD
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyFileVersion("1.0.1.0")]
#endif

namespace PolarsHoldCrouch;

// ReSharper disable once UnusedType.Global
public class Plugin : IPlugin
{
    public const string Name = "PolarsHoldCrouch";

    private HoldCrouch holdCrouch;
    private bool failed;

    public void Init(object gameInstance)
    {
        try
        {
            holdCrouch = new HoldCrouch();
            Log("Loaded");
        }
        catch (Exception e)
        {
            Fail("Init", e);
        }
    }

    public void Update()
    {
        if (failed || holdCrouch == null)
            return;

        try
        {
            holdCrouch.Update();
        }
        catch (Exception e)
        {
            Fail("Update", e);
        }
    }

    public void Dispose()
    {
        holdCrouch = null;
    }

    private void Fail(string where, Exception e)
    {
        failed = true;
        holdCrouch = null;
        Log($"{where} failed, hold to crouch is disabled until the game restarts: {e}");
    }

    internal static void Log(string message)
    {
        MyLog.Default?.WriteLine($"[{Name}] {message}");
    }
}
