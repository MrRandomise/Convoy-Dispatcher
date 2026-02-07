using UnityEditor.Overlays;

public interface ISaveSystem
{
    void Save();
    void Load();
    SaveData GetData();
    void SetData(SaveData data);
}