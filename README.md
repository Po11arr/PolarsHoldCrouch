# Polar's Hold Crouch

Space Engineers client plugin for [Pulsar](https://github.com/SpaceGT/Pulsar). Crouch while the Crouch key is held and stand up when it is released. Settings add a Hybrid mode (tap to toggle, hold to hold) and the vanilla toggle.

## Usage

- Hold your Crouch key (C by default) to crouch. Release it to stand up.
- With the default Hybrid mode, a quick tap toggles crouch like vanilla.
- Uses the game's own Crouch binding, keyboard or controller. Rebind it in the game's controls menu.
- Pressing Crouch while already crouched still stands you up, as in vanilla.
- Under a low ceiling the character stays crouched after release and stands up as soon as there is headroom. Same if you release while falling or with the jetpack on.

## Settings

Open them with the Settings button for Polar's Hold Crouch in Pulsar's plugin list. The list is in the main menu and in the in-game Esc menu.

| Setting | Values | Default |
| --- | --- | --- |
| Crouch mode | Hold: crouch while the key is held. Toggle: vanilla, each press switches between crouching and standing. Hybrid: a tap shorter than 0.25 s toggles like vanilla, a longer press crouches only while held. | Hybrid |

Changes apply immediately. They are saved in `Storage\PolarsHoldCrouch.cfg` in the game's user data folder (normally `%AppData%\SpaceEngineers`). A missing file is created with the defaults. A file that isn't valid is replaced with them.

## Not affected

- Cockpits, seats, ladders, spectator camera
- Menus, terminal, chat
- Jetpack down movement (still on the Crouch key)

## How it works

Vanilla toggles crouch when the Crouch control is first pressed. In Hold mode the plugin watches the same control and, when it is released after a press that crouched, calls the same `MyCharacter.Crouch()` the game uses to stand back up. In Hybrid mode it does the same unless the press was a tap shorter than 0.25 s, which keeps the vanilla toggle. In Toggle mode it does nothing. No Harmony patches, no game methods changed.

It reads the character's crouch intent flag (the one `Crouch()` toggles) so sprinting or a jump in the middle of a hold doesn't confuse it. If a game update hides that flag, it falls back to the visible crouch state and writes a line to the game log.

## Compatibility

- Client-side only, no server install needed.
- Tested in single player and as a client on a dedicated server.
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
