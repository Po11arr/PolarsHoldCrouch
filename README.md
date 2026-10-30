# Polar's Hold Crouch

Space Engineers client plugin for [Pulsar](https://github.com/SpaceGT/Pulsar). Makes on-foot crouching hold-to-crouch instead of a toggle.

## Usage

- Hold your Crouch key (C by default) to crouch. Release it to stand up.
- Uses the game's own Crouch binding, keyboard or controller. Rebind it in the game's controls menu.
- Pressing Crouch while already crouched still stands you up, as in vanilla.
- Under a low ceiling the character stays crouched after release and stands up as soon as there is headroom. Same if you release while falling or with the jetpack on.

## Not affected

- Cockpits, seats, ladders, spectator camera
- Menus, terminal, chat
- Jetpack down movement (still on the Crouch key)

## How it works

Vanilla toggles crouch when the Crouch control is first pressed. The plugin watches the same control and, when it is released after a press that crouched, calls the same `MyCharacter.Crouch()` the game uses to stand back up. No Harmony patches, no game methods changed.

It reads the character's crouch intent flag (the one `Crouch()` toggles) so sprinting or a jump in the middle of a hold doesn't confuse it. If a game update hides that flag, it falls back to the visible crouch state and writes a line to the game log.

## Configuration

None.

## Compatibility

- Client-side only, no server install needed.
- Tested in single player. Multiplayer (listen host and dedicated server) is untested so far.
- No patched methods, so no patch conflicts with other plugins.
- .NET Framework 4.8 (Legacy) and .NET 10 (Interim) Pulsar runtimes.

## Log

Lines are prefixed with `[PolarsHoldCrouch]` in `SpaceEngineers.log`. If anything throws, the plugin logs the error and switches itself off until the game restarts.

## Building locally

Open `PolarsHoldCrouch.sln`. `Bin64` is auto-detected from Steam; override it in `Directory.Build.props.user` if needed. Set `Pulsar` there to deploy each build into Pulsar's `Local` plugin folder.

## Support

Bugs and suggestions: [GitHub issues](https://github.com/Po11arr/PolarsHoldCrouch/issues)

## License

MIT
