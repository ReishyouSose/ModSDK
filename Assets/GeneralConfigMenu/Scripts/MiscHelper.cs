using CoreLib.Data.Configuration;
using HarmonyLib;
using I2.Loc;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public static class MiscHelper
    {
        public static bool IsNeedReload(this ConfigEntryBase config)
            => config.Description.Tags.Any(x => x.ToString().ToLower().Contains("needreload"));
        public static void WirteNeedReloadText(this StringBuilder builder)
            => builder.Append("[!]")
                    .Append(LocalizationManager.GetTranslation("GeneralConfigMenu/NeedReload"))
                    .AppendLine();
        public static string GetLocalKey(string filePath, params string[] fix)
        {
            string[] split = filePath.Split('/');
            string name = split[^1];
            int index = name.LastIndexOf('.');
            name = index > 0 ? name[..index] : name;
            split[^1] = name;
            if (fix != null)
            {
                split = split.AddRangeToArray(fix);
            }
            int length = split.Length;
            StringBuilder builder = new();
            for (int i = 0; i < length; i++)
            {
                builder.Append(split[i]);
                builder.Append(i < length - 2 ? '_' : '/');
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        public static bool TryGetLocalizedText(out LocalizedText text, string key)
        {
            text = TextDataBlock.GetLocalized(key);
            var result = text.title != text.description;
            Debug.Log((key, result));
            Debug.Log($"title: {text.title} desc: {text.description}");
            return result;
        }

        public static bool TryGetLocalizedText(out LocalizedText text, string filePath, params string[] fix) => TryGetLocalizedText(out text, GetLocalKey(filePath, fix));

        public static void SetText(this PugText text, string key, string origin)
        {
            //if (TryGetLocalizedText(out _, key))
            if (LocalizationManager.TryGetTranslation(key, out _))
            {
                text.localize = true;
                text.Render(key, false, true);
            }
            else
            {
                text.localize = false;
                text.Render(origin, false, true);
            }
        }
    }
}
