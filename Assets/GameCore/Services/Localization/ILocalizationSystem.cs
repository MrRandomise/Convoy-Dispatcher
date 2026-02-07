public interface ILocalizationSystem
{
    string Get(string key);
    void SetLanguage(string languageCode);
    string CurrentLanguage { get; }
}