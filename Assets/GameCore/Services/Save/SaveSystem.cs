using UnityEngine;

public class SaveSystem : ISaveSystem
{
    private const string SaveKey = "GameSave";
    private SaveData _data;

    public void Save()
    {
        var json = JsonUtility.ToJson(_data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            var json = PlayerPrefs.GetString(SaveKey);
            _data = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            _data = new SaveData();
        }
    }

    public SaveData GetData() => _data;
    public void SetData(SaveData data) => _data = data;
}