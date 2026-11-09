namespace PolarsHoldCrouch;

// Decides when to stand the character back up after the Crouch control is released.
// Pure state machine with no game calls, so every case can be tested outside the game.
internal sealed class CrouchRelease
{
    // Frames between stand-up attempts while standing is blocked (low ceiling, jetpack on).
    internal const int RetryInterval = 10;

    private bool wasHeld;
    private bool heldCrouch;
    private long pressStart;
    private int retryCountdown;

    // Nothing pending: the control isn't down and no stand-up is owed.
    public bool Idle => !wasHeld && !heldCrouch;

    public void Reset()
    {
        wasHeld = false;
        heldCrouch = false;
        retryCountdown = 0;
    }

    // held: Crouch control is down. wants: the character's crouch intent flag. falling: the character is falling.
    // now and tapTime share one clock; a Hybrid release sooner than tapTime after the press counts as a tap.
    // Returns true when the caller should call MyCharacter.Crouch() to stand up.
    public bool Step(CrouchMode mode, bool held, bool wants, bool falling, long now, long tapTime)
    {
        if (held)
        {
            if (!wasHeld)
                pressStart = now;
            wasHeld = true;

            // Pressing while already crouched makes vanilla stand up, so only a press that crouched counts.
            // Falling clears the flag temporarily (vanilla restores it on landing), so keep the state then.
            if (wants)
                heldCrouch = true;
            else if (!falling)
                heldCrouch = false;

            retryCountdown = 0;
            return false;
        }

        var justReleased = wasHeld;
        wasHeld = false;

        if (!heldCrouch)
            return false;

        // Hybrid: a short tap keeps the vanilla toggle, so the character stays crouched.
        if (justReleased && mode == CrouchMode.Hybrid && now - pressStart < tapTime)
        {
            heldCrouch = false;
            return false;
        }

        if (!wants)
        {
            if (!falling)
                heldCrouch = false;
            return false;
        }

        if (retryCountdown > 0)
        {
            retryCountdown--;
            return false;
        }

        retryCountdown = RetryInterval;
        return true;
    }
}
