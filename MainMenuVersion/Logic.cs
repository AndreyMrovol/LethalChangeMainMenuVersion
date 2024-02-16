//
//  https://github.com/Owen3H/IntroTweaks/blob/main/Patches/MenuManagerPatch.cs
//  under GNU General Public License v3.0
//  https://github.com/Owen3H/IntroTweaks/blob/main/LICENSE
//
//
//
//
//
//
//
//


using System;
using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MainMenuVersion
{
  [HarmonyPatch(typeof(MenuManager))]
  public class MenuManagerPatch
  {
    public static TextMeshProUGUI versionText { get; private set; }
    public static RectTransform versionTextRect { get; private set; }

    public static int realVer { get; internal set; }
    public static int gameVer { get; private set; }

    static MenuManager Instance;

    internal static GameObject VersionNum = null;
    internal static Transform MenuContainer = null;
    internal static Transform MenuPanel = null;

    public static Color32 DARK_ORANGE = new(175, 115, 0, 255);

    [HarmonyPatch(typeof(GameNetworkManager))]
    [HarmonyPrefix]
    [HarmonyPatch("Awake")]
    static void SetRealVersion(GameNetworkManager __instance)
    {
      realVer = __instance.gameVersionNum;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    static void Init(MenuManager __instance)
    {
      Instance = __instance;
      Instance.StartCoroutine(PatchMenuDelayed());
    }

    private static IEnumerator PatchMenuDelayed()
    {
      /*
      *  Waits a single frame.
      *
      *  This is slightly hacky but ensures all references are not null
      *  and that the mod makes its changes after all others.
      */
      yield return new WaitUntil(() => !GameNetworkManager.Instance.firstTimeInMenu);

      MenuContainer = GameObject.Find("MenuContainer")?.transform;
      MenuPanel = MenuContainer?.Find("MainButtons");
      VersionNum = MenuContainer?.Find("VersionNum")?.gameObject;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    [HarmonyPriority(Priority.Last)]
    static void UpdatePatch(MenuManager __instance)
    {
      bool onMenu = __instance.menuButtons.activeSelf;

      // find all in a scene and log them
      var textRects = Object.FindObjectsOfType<RectTransform>();
      foreach (var rect in textRects)
      {
        if (rect.name == "VersionNumberText")
        {
          // hide and remove
          rect.gameObject.SetActive(false);
        }
      }

      // Create the new game version text if not made already.
      if (versionText == null)
        TryReplaceVersionText();
      else
      {
        // Make sure the text is correct.
        versionText.text = ConfigManager.VersionEntry.Value.Replace("%VERSION%", $"{gameVer}");

        var textObj = versionText.gameObject;
        if (!textObj.activeSelf && onMenu)
        {
          textObj.SetActive(true);
        }
      }
    }

    internal static void TryReplaceVersionText()
    {
      if (VersionNum == null || MenuPanel == null)
        return;

      GameObject clone = Object.Instantiate(VersionNum, MenuPanel);
      clone.name = "MainMenuVersion";

      versionText = InitTextMesh(clone.GetComponent<TextMeshProUGUI>());
      versionTextRect = versionText.gameObject.GetComponent<RectTransform>();
      versionTextRect.AnchorToBottom();

      VersionNum.SetActive(false);
    }

    static void SetVersion()
    {
      bool alwaysShort = ConfigManager.AlwaysShortVersion.Value;
      int curVer = Math.Abs(GameNetworkManager.Instance.gameVersionNum);
      gameVer = alwaysShort ? realVer : (curVer != realVer ? curVer : realVer);
    }

    static TextMeshProUGUI InitTextMesh(TextMeshProUGUI tmp)
    {
      SetVersion();

      tmp.text = ConfigManager.VersionEntry.Value;
      tmp.fontSize = ConfigManager.Size.ClampedValue(10, 40);
      tmp.alignment = TextAlignmentOptions.Center;

      TweakTextSettings(tmp);

      return tmp;
    }

    static void TweakTextSettings(TextMeshProUGUI tmp, bool overflow = true, bool wordWrap = false)
    {
      if (overflow)
        tmp.overflowMode = TextOverflowModes.Overflow;
      tmp.enableWordWrapping = wordWrap;
      tmp.faceColor = DARK_ORANGE;
    }
  }
}
