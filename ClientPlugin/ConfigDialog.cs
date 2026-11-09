using System;
using Sandbox;
using Sandbox.Graphics.GUI;
using VRage.Utils;
using VRageMath;

namespace PolarsHoldCrouch;

// Opened by the Settings button in Pulsar's plugin list, in the main menu or in game.
internal sealed class ConfigDialog : MyGuiScreenBase
{
    private readonly Config config;

    public ConfigDialog(Config config)
        : base(
            new Vector2(0.5f),
            MyGuiConstants.SCREEN_BACKGROUND_COLOR,
            new Vector2(0.42f, 0.2f),
            backgroundTransition: MySandboxGame.Config.UIBkOpacity,
            guiTransition: MySandboxGame.Config.UIOpacity)
    {
        this.config = config;
        EnabledBackgroundFade = true;
        CloseButtonEnabled = true;
        m_closeOnEsc = true;
    }

    public override string GetFriendlyName() => "PolarsHoldCrouchConfig";

    public override void LoadContent()
    {
        base.LoadContent();
        RecreateControls(true);
    }

    public override void RecreateControls(bool constructor)
    {
        try
        {
            base.RecreateControls(constructor);

            AddCaption("Polar's Hold Crouch");

            var label = new MyGuiControlLabel(
                position: new Vector2(-0.17f, 0.02f),
                text: "Crouch mode",
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);

            var mode = new MyGuiControlCombobox(
                position: new Vector2(0.17f, 0.02f),
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            mode.Size = new Vector2(0.18f, mode.Size.Y);
            mode.AddItem((long)CrouchMode.Hold, "Hold", toolTip: "Crouch while the Crouch key is held, stand up when it is released.");
            mode.AddItem((long)CrouchMode.Toggle, "Toggle", toolTip: "Vanilla: each press of Crouch switches between crouching and standing.");
            mode.AddItem((long)CrouchMode.Hybrid, "Hybrid", toolTip: "Tap Crouch to toggle like vanilla, hold it longer to crouch only while held.");
            mode.SelectItemByKey((long)config.Mode, sendEvent: false);
            mode.ItemSelected += () => OnModeSelected(mode);

            Controls.Add(label);
            Controls.Add(mode);
        }
        catch (Exception e)
        {
            Plugin.Log($"Could not build the settings screen: {e}");
        }
    }

    private void OnModeSelected(MyGuiControlCombobox mode)
    {
        try
        {
            config.Mode = (CrouchMode)mode.GetSelectedKey();
            config.Save();
            Plugin.Log($"Crouch mode set to {config.Mode}");
        }
        catch (Exception e)
        {
            Plugin.Log($"Could not change the crouch mode: {e}");
        }
    }
}
