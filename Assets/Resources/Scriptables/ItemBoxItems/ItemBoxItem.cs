using UnityEngine;

[CreateAssetMenu(fileName = "ItemBoxItem", menuName = "ScriptableObjects/ItemBoxItem", order = 5)]
public class ItemBoxItem : ScriptableObject {

    public Enums.ItemBoxItem item;
    public string itemPrefab;
    public Enums.Sounds soundEffect = Enums.Sounds.Player_Sound_PowerupCollect;
    public float chance = 0.1f, losingChanceBoost = 0f;
    public bool firstToThird, fourthToSeventh, eighth;
    public Sprite rouletteSprite;

    public float GetModifiedChance(float starsToWin, float leaderStars, float ourStars) {
        float starDifference = leaderStars - ourStars;
        float bonus = losingChanceBoost * Mathf.Log(starDifference + 1) * (1f - ((starsToWin - leaderStars) / starsToWin));
        return Mathf.Max(0, chance + bonus);
    }
}