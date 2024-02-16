using System;
using BepInEx.Configuration;

namespace MainMenuVersion
{
  public class ConfigManager
  {
    public static ConfigManager Instance { get; private set; }

    public static void Init(ConfigFile config)
    {
      Instance = new ConfigManager(config);
    }

    private readonly ConfigFile configFile;

    public static ConfigEntry<string> VersionEntry { get; private set; }
    public static ConfigEntry<Single> Size { get; private set; }
    public static ConfigEntry<Single> YOffset { get; private set; }

    public static ConfigEntry<bool> AlwaysShortVersion { get; private set; }

    private ConfigManager(ConfigFile config)
    {
      // god forgive me for this
      configFile = config;

      VersionEntry = configFile.Bind("General", "Version", "v%VERSION% [MODDED]", "The version string to display on the main menu");

      Size = configFile.Bind("General", "Size", 20f, "The size of the version string");
      YOffset = configFile.Bind("General", "YOffset", 0f, "The Y offset of the version string");

      AlwaysShortVersion = configFile.Bind(
        "General",
        "AlwaysShortVersion",
        true,
        "Always display the short version of the game - this ignores LC_API and MoreCompany changes"
      );
    }
  }
}
