using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NSMB.Utils;

public class LobbySettingsList : MonoBehaviour
{
    public TMP_Text mapName, starReq, coinReq, lives, timer;
    public GameObject[] powerupSprites;
    public GameObject[] togglesSprites;

    void Update()
    {
        
        // Game settings texts
        Utils.GetCustomProperty(Enums.NetRoomProperties.Lives, out int life);
        Utils.GetCustomProperty(Enums.NetRoomProperties.Time, out int time);
        mapName.text = "Map: " + GameManager.Instance.levelName;
        starReq.text = "Star Requirement: "  + GameManager.Instance.starRequirement.ToString();
        coinReq.text = "Coins for Item: " + GameManager.Instance.coinRequirement.ToString();
        lives.text = life == -1 ? "Lives: Disabled" : "Amount of Lives: " + MainMenuManager.Instance.livesField.text;
        timer.text = time == -1 ? " Timer: Disabled" : "Time Limit: " + MainMenuManager.Instance.timeField.text;

        // Item sprites 
        foreach (var sprite in powerupSprites) {
            Utils.GetCustomProperty(Enums.NetRoomProperties.NsmbPowerups, out bool mush);
            Utils.GetCustomProperty(Enums.NetRoomProperties.FireFlowerPowerup, out bool fire);
            Utils.GetCustomProperty(Enums.NetRoomProperties.BlueShellPowerup, out bool blue);
            Utils.GetCustomProperty(Enums.NetRoomProperties.MiniMushroomPowerup, out bool mini);
            Utils.GetCustomProperty(Enums.NetRoomProperties.OneUpMush, out bool oneup);
            Utils.GetCustomProperty(Enums.NetRoomProperties.NewPowerups, out bool ice);
            Utils.GetCustomProperty(Enums.NetRoomProperties.PropellerMush, out bool feller);
            Utils.GetCustomProperty(Enums.NetRoomProperties.FrostyPowerups, out bool stellar);
            Utils.GetCustomProperty(Enums.NetRoomProperties.TideFlowerPowerup, out bool tide);
            Utils.GetCustomProperty(Enums.NetRoomProperties.SuperAcornPowerup, out bool acorn);
            Utils.GetCustomProperty(Enums.NetRoomProperties.TenPlayersPowerups, out bool water);
            Utils.GetCustomProperty(Enums.NetRoomProperties.MagmaFlowerPowerup, out bool magma);
            Utils.GetCustomProperty(Enums.NetRoomProperties.TemporaryPowerups, out bool mega);
            Utils.GetCustomProperty(Enums.NetRoomProperties.StarmanPowerup, out bool star);
            Utils.GetCustomProperty(Enums.NetRoomProperties.CobaltStarPowerup, out bool cobalt);
            
            if (sprite.name.Contains("Mushroom") && !mush)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("FireFlower") && !fire)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("BlueShell") && !blue)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("MiniMush") && !mini)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("1upMush") && !oneup)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("IceFlower") && !ice)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("PropellerMush") && !feller)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("StellarFlower") && !stellar)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("TideFlower") && !tide)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("SuperAcorn") && !acorn)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("WaterFlower") && !water)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("MagmaFlower") && !magma)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("MegaMush") && !mega)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("Starman") && !star)
                sprite.gameObject.SetActive(false);

            if (sprite.name.Contains("CobaltStar") && !cobalt)
                sprite.gameObject.SetActive(false);
        }

        foreach (var toggleSprite in togglesSprites) {
            Utils.GetCustomProperty(Enums.NetRoomProperties.DropReserve, out bool reserve);
            Utils.GetCustomProperty(Enums.NetRoomProperties.FireballDamage, out bool fireball);
            Utils.GetCustomProperty(Enums.NetRoomProperties.ProgressiveToRoulette, out bool ptr);
            Utils.GetCustomProperty(Enums.NetRoomProperties.NoMapCoins, out bool nocoin);
            Utils.GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out bool team);
            Utils.GetCustomProperty(Enums.NetRoomProperties.FriendlyFire, out bool friendly);
            Utils.GetCustomProperty(Enums.NetRoomProperties.DeathmatchGame, out bool deathmatch);

            if (toggleSprite.name.Contains("DropReserve") && !reserve)
                toggleSprite.gameObject.SetActive(false);

            if (toggleSprite.name.Contains("FireballDmg") && !fireball)
                toggleSprite.gameObject.SetActive(false);

            if (toggleSprite.name.Contains("ProgressiveToRoulette") && !ptr)
                toggleSprite.gameObject.SetActive(false);

            if (toggleSprite.name.Contains("NoCoins") && !nocoin)
                toggleSprite.gameObject.SetActive(false);

            if (toggleSprite.name.Contains("Teams") && !team)
                toggleSprite.gameObject.SetActive(false);

            if (toggleSprite.name.Contains("FriendlyFire") && !friendly)
                toggleSprite.gameObject.SetActive(false);

            if (toggleSprite.name.Contains("Deathmatch") && !deathmatch)
                toggleSprite.gameObject.SetActive(false);
        
        }

    }
}
