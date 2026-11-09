using System;
using System.IO;
using System.Xml.Serialization;
using VRage.FileSystem;

namespace PolarsHoldCrouch;

public enum CrouchMode
{
    // Crouch while the Crouch control is held, stand up when it is released.
    Hold,

    // Vanilla: each press of the Crouch control switches between crouching and standing.
    Toggle,

    // A short tap toggles like vanilla, a longer press crouches only while held.
    Hybrid,
}

// Settings, saved as XML in <game user data>\Storage\PolarsHoldCrouch.cfg.
// A missing file is created with the defaults. A file that isn't valid is replaced with them.
public class Config
{
    private const string FileName = "PolarsHoldCrouch.cfg";

    private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Config));

    public CrouchMode Mode { get; set; } = CrouchMode.Hybrid;

    internal static Config Load()
    {
        var path = GetPath();
        if (path == null)
        {
            Plugin.Log("Game user data folder is not available, using the default settings");
            return new Config();
        }

        if (File.Exists(path))
        {
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    if (Serializer.Deserialize(stream) is Config loaded)
                        return loaded;
                }

                // A nil root element deserialises to null.
                Plugin.Log($"{path} holds no settings, replacing it with the default settings");
            }
            catch (InvalidOperationException e)
            {
                // Not valid XML, or a value this version doesn't know.
                Plugin.Log($"{path} is not valid, replacing it with the default settings: {e.InnerException?.Message ?? e.Message}");
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                // Can't be read right now: use the defaults for this session and leave the file alone.
                Plugin.Log($"Could not read {path}, using the default settings: {e.Message}");
                return new Config();
            }
        }

        var defaults = new Config();
        defaults.Save(path);
        return defaults;
    }

    internal void Save()
    {
        var path = GetPath();
        if (path != null)
            Save(path);
    }

    private void Save(string path)
    {
        var temp = path + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Write a temporary file first, so an interrupted save never leaves a half-written config.
            using (var stream = File.Create(temp))
                Serializer.Serialize(stream, this);

            if (File.Exists(path))
                File.Replace(temp, path, null);
            else
                File.Move(temp, path);
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            Plugin.Log($"Could not save {path}: {e.Message}");
        }
    }

    private static string GetPath()
    {
        // UserDataPath throws until the game has set up its file system.
        if (!MyFileSystem.IsInitialized)
            return null;

        var userData = MyFileSystem.UserDataPath;
        return string.IsNullOrEmpty(userData) ? null : Path.Combine(userData, "Storage", FileName);
    }
}
