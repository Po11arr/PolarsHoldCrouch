using System;
using System.Linq.Expressions;
using System.Reflection;
using Sandbox.Game;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using VRage.Input;

namespace PolarsHoldCrouch;

// Vanilla toggles crouch when the Crouch control is first pressed.
// This adds the missing half: when the control is released, toggle back to standing.
// No Harmony patches: vanilla input handling is left untouched.
internal sealed class HoldCrouch
{
    // Frames between stand-up attempts while standing is blocked (low ceiling, jetpack on).
    private const int RetryInterval = 10;

    // Reads the character's crouch intent (the flag vanilla's Crouch() toggles),
    // falling back to the visible crouch state if the flag can't be found.
    private readonly Func<MyCharacter, bool> wantsCrouch;

    private MyCharacter character;
    private bool heldCrouch;
    private int retryCountdown;

    public HoldCrouch()
    {
        wantsCrouch = BuildWantsCrouchReader();
    }

    public void Update()
    {
        var session = MySession.Static;
        var ch = session?.LocalCharacter;
        if (ch == null || ch.IsDead || ch.MarkedForClose)
        {
            Reset(null);
            return;
        }

        if (!ReferenceEquals(ch, character))
            Reset(ch);

        // Seated or remote controlling: keep the state, so releasing the key while seated
        // still stands the character up once it is controlled again.
        var controlled = session.ControlledEntity;
        if (!ReferenceEquals(controlled, ch))
            return;

        // Match vanilla: crouch input is only read by the gameplay screen, not in menus, chat or spectator.
        if (!GameplayHasFocus() || session.IsCameraUserControlledSpectator() || ch.IsOnLadder)
            return;

        var held = MyControllerHelper.IsControl(controlled.ControlContext, MyControlsSpace.CROUCH, MyControlStateType.PRESSED);
        if (!held && !heldCrouch)
            return;

        var wants = wantsCrouch(ch);

        if (held)
        {
            // Pressing while already crouched makes vanilla stand up, so only a press that crouched counts.
            // Falling clears the flag temporarily (vanilla restores it on landing), so keep the state then.
            if (wants)
                heldCrouch = true;
            else if (!ch.IsFalling)
                heldCrouch = false;

            retryCountdown = 0;
            return;
        }

        if (!wants)
        {
            if (!ch.IsFalling)
                heldCrouch = false;
            return;
        }

        if (retryCountdown > 0)
        {
            retryCountdown--;
            return;
        }

        // Same call vanilla makes on a crouch press. Does nothing if there is no headroom,
        // the jetpack is on or the character is falling, so it is retried until it succeeds.
        ch.Crouch();
        retryCountdown = RetryInterval;
    }

    private void Reset(MyCharacter newCharacter)
    {
        character = newCharacter;
        heldCrouch = false;
        retryCountdown = 0;
    }

    private static bool GameplayHasFocus()
    {
        if (!(MyScreenManager.GetScreenWithFocus() is MyGuiScreenGamePlay))
            return false;

        var gui = MyAPIGateway.Gui;
        return gui == null || !gui.ChatEntryVisible;
    }

    private static Func<MyCharacter, bool> BuildWantsCrouchReader()
    {
        try
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var type = typeof(MyCharacter);
            var param = Expression.Parameter(type, "c");
            Expression body = null;
            string source = null;

            var wantsProperty = type.GetProperty("WantsCrouch", flags);
            if (wantsProperty != null && wantsProperty.PropertyType == typeof(bool) && wantsProperty.GetGetMethod(true) != null)
            {
                body = Expression.Property(param, wantsProperty);
                source = "WantsCrouch";
            }
            else
            {
                Expression movementFlags = null;
                var flagsProperty = type.GetProperty("MovementFlags", flags);
                if (flagsProperty != null && flagsProperty.PropertyType.IsEnum && flagsProperty.GetGetMethod(true) != null)
                {
                    movementFlags = Expression.Property(param, flagsProperty);
                    source = "MovementFlags";
                }
                else
                {
                    var flagsField = type.GetField("m_movementFlags", flags);
                    if (flagsField != null && flagsField.FieldType.IsEnum)
                    {
                        movementFlags = Expression.Field(param, flagsField);
                        source = "m_movementFlags";
                    }
                }

                if (movementFlags != null && Enum.IsDefined(movementFlags.Type, "Crouch"))
                {
                    var crouchBit = Convert.ToInt64(Enum.Parse(movementFlags.Type, "Crouch"));
                    var asLong = Expression.Convert(Expression.Convert(movementFlags, Enum.GetUnderlyingType(movementFlags.Type)), typeof(long));
                    body = Expression.NotEqual(Expression.And(asLong, Expression.Constant(crouchBit)), Expression.Constant(0L));
                }
            }

            if (body != null)
            {
                var reader = Expression.Lambda<Func<MyCharacter, bool>>(body, param).Compile();
                Plugin.Log($"Reading crouch intent from MyCharacter.{source}");
                return reader;
            }
        }
        catch (Exception e)
        {
            Plugin.Log($"Could not read the crouch intent flag: {e.Message}");
        }

        Plugin.Log("Crouch intent flag not found, falling back to MyCharacter.IsCrouching");
        return c => c.IsCrouching;
    }
}
