using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
using RizgarOzan.TmpGlyphAudit.Editor;

namespace RizgarOzan.TmpGlyphAudit.Localization
{
    /// <summary>Scans Unity Localization string tables without making Localization a core dependency.</summary>
    public static class LocalizationTableScanner
    {
        public static void Scan(GlyphAudit audit, GlyphAuditSettings settings)
        {
            if (settings.localizationFonts == null || settings.localizationFonts.Count == 0)
                return;

            foreach (string guid in AssetDatabase.FindAssets("t:StringTable"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var table = AssetDatabase.LoadAssetAtPath<StringTable>(path);
                if (table != null)
                    ScanTable(audit, settings, table, path);
            }
        }

        public static void ScanTable(GlyphAudit audit, GlyphAuditSettings settings, StringTable table, string source)
        {
            if (audit == null) throw new ArgumentNullException(nameof(audit));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (table == null) throw new ArgumentNullException(nameof(table));

            string localeCode = table.LocaleIdentifier.Code;
            var configured = settings.localizationFonts?.FirstOrDefault(
                item => item != null && string.Equals(item.localeCode?.Trim(), localeCode, StringComparison.OrdinalIgnoreCase));
            if (configured == null)
                return;

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(configured.fontAsset);
            if (font == null)
            {
                Debug.LogWarning($"TMP Glyph Audit: no TMP font asset at '{configured.fontAsset}' for locale '{localeCode}' (String Table '{source}').");
                return;
            }

            foreach (var entry in table.Values)
            {
                if (entry == null || string.IsNullOrEmpty(entry.LocalizedValue))
                    continue;

                string drawableText = TextScan.StripSmartStringPlaceholders(entry.LocalizedValue);
                audit.Check(drawableText, font, true, true, source, entry.Key);
            }
        }
    }
}
