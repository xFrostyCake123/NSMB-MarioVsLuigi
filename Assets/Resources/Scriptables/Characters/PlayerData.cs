using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 0)]
public class PlayerData : ScriptableObject {
    public string characterName, soundFolder, prefab, uistring;
    public Sprite loadingSmallSprite, loadingBigSprite, readySprite;
    public TMP_ColorGradient readyTextGradient;
    public RuntimeAnimatorController smallOverrides, largeOverrides;
}