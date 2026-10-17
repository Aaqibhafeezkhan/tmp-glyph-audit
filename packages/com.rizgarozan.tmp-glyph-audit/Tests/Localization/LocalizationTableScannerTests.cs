using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RizgarOzan.TmpGlyphAudit.Editor;
using RizgarOzan.TmpGlyphAudit.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public class LocalizationTableScannerTests
{
    const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    readonly List<Object> _created = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        foreach (var item in _created)
            if (item != null) Object.DestroyImmediate(item);
        _created.Clear();
    }

    [Test]
    public void Locale_tables_use_their_configured_font_and_report_table_path_and_key()
    {
        var shared = ScriptableObject.CreateInstance<SharedTableData>();
        shared.TableCollectionName = "Glyph Audit Test";
        _created.Add(shared);

        var turkish = MakeTable(shared, "tr", "greeting", "Merhaba ğ");
        var japanese = MakeTable(shared, "ja", "greeting", "こんにちは {player}");
        var settings = new GlyphAuditSettings
        {
            localizationFonts = new List<GlyphAuditSettings.LocalizationFont>
            {
                new GlyphAuditSettings.LocalizationFont { localeCode = "tr", fontAsset = FontPath },
                new GlyphAuditSettings.LocalizationFont { localeCode = "ja", fontAsset = FontPath },
            },
        };
        var audit = new GlyphAudit();

        LocalizationTableScanner.ScanTable(audit, settings, turkish, "Assets/Localization/Test_tr.asset");
        LocalizationTableScanner.ScanTable(audit, settings, japanese, "Assets/Localization/Test_ja.asset");

        Assert.IsTrue(audit.Report.Findings.Any(f =>
            f.Source == "Assets/Localization/Test_ja.asset" &&
            f.Location == "greeting" &&
            f.Missing.Count > 0));
        Assert.IsFalse(audit.Report.Findings.Any(f => f.Source == "Assets/Localization/Test_tr.asset"));
        Assert.IsFalse(audit.Report.Findings.Any(f => f.Missing.Any(cp => cp == '{' || cp == 'p')));
    }

    StringTable MakeTable(SharedTableData shared, string locale, string key, string value)
    {
        var table = ScriptableObject.CreateInstance<StringTable>();
        table.SharedData = shared;
        table.LocaleIdentifier = new LocaleIdentifier(locale);
        table.TableName = "Glyph Audit Test";
        table.AddEntry(key, value);
        _created.Add(table);
        return table;
    }
}
