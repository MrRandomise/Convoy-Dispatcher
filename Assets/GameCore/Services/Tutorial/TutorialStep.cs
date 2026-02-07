using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Game/Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    public string LocalizationKey;
    public Vector2 HighlightPosition;
    public Vector2 HighlightSize;
    public bool RequiresAction;
    public string RequiredActionId;
}