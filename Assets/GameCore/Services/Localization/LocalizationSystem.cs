using System.Collections.Generic;
using UnityEngine;

public class LocalizationSystem : ILocalizationSystem
{
    private Dictionary<string, string> _translations = new();
    public string CurrentLanguage { get; private set; }

    public void SetLanguage(string languageCode)
    {
        CurrentLanguage = languageCode;
        var asset = Resources.Load<TextAsset>($"Localization/{languageCode}");
        if (asset != null)
        {
            _translations = ParseLocalization(asset.text);
        }
    }

    public string Get(string key)
    {
        return _translations.TryGetValue(key, out var value) ? value : $"[{key}]";
    }

    private Dictionary<string, string> ParseLocalization(string text)
    {
        var result = new Dictionary<string, string>();
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            var parts = line.Split('=');
            if (parts.Length == 2)
            {
                result[parts[0].Trim()] = parts[1].Trim();
            }
        }
        return result;
    }
}