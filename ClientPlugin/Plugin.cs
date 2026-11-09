using System;
using System.Reflection;
using Sandbox.Graphics.GUI;
using VRage.Plugins;
using VRage.Utils;

#if !LOCAL_BUILD
[assembly: AssemblyVersion("1.2.1.0")]
[assembly: AssemblyFileVersion("1.2.1.0")]
#endif

namespace PolarsHoldCrouch;

// ReSharper disable once UnusedType.Global
public class Plugin : IPlugin
{
    public const string Name = "PolarsHoldCrouch";

    private Config config;
    private HoldCrouch holdCrouch;
    private bool failed;

    public void Init(object gameInstance)
    {
        try
        {
            config = Config.Load();
            holdCrouch = new HoldCrouch(config);
            Log($"Loaded, crouch mode: {config.Mode}");
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

    // Called by Pulsar when the player clicks Settings for this plugin.
    // ReSharper disable once UnusedMember.Global
    public void OpenConfigDialog()
    {
        try
        {
            if (config == null)
                config = Config.Load();

            MyGuiSandbox.AddScreen(new ConfigDialog(config));
        }
        catch (Exception e)
        {
            Log($"Could not open the settings: {e}");
        }
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
