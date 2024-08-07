using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using NSMB.Utils;


public class UIUpdater : MonoBehaviour {

    public static UIUpdater Instance;
    public GameObject playerTrackTemplate, starTrackTemplate;
    public PlayerController player;
    public Sprite storedItemNull;
    public TMP_Text uiStars, uiCoins, uiDebug, uiLives, uiCountdown, uiMiniTimer, redStars, yellowStars, greenStars, blueStars, purpleStars, uiPoints, uiCombo, comboRating;
    public Image itemReserve, itemColor;
    public GameObject cobaltEffect, protectedCobaltEffect;
    public float pingSample = 0;

    private Material timerMaterial;
    private GameObject starsParent, coinsParent, livesParent, timerParent, miniParent, pointsParent, comboParent;
    public GameObject teamsHeader;
    private readonly List<Image> backgrounds = new();
    private bool uiHidden;

    private int coins = -1, stars = -1, lives = -1, timer = -1, miniTimer = -1, combo = -1, points = -1;

    public void Start() {
        Instance = this;
        pingSample = PhotonNetwork.GetPing();

        starsParent = uiStars.transform.parent.gameObject;
        coinsParent = uiCoins.transform.parent.gameObject;
        livesParent = uiLives.transform.parent.gameObject;
        timerParent = uiCountdown.transform.parent.gameObject;
        miniParent = uiMiniTimer.transform.parent.gameObject;
        comboParent = uiCombo.transform.parent.gameObject;
        pointsParent = uiPoints.transform.parent.gameObject;

        backgrounds.Add(starsParent.GetComponentInChildren<Image>());
        backgrounds.Add(coinsParent.GetComponentInChildren<Image>());
        backgrounds.Add(livesParent.GetComponentInChildren<Image>());
        backgrounds.Add(timerParent.GetComponentInChildren<Image>());
        backgrounds.Add(pointsParent.GetComponentInChildren<Image>());

        foreach (Image bg in backgrounds)
            bg.color = GameManager.Instance.levelUIColor;
        itemColor.color = new(GameManager.Instance.levelUIColor.r - 0.2f, GameManager.Instance.levelUIColor.g - 0.2f, GameManager.Instance.levelUIColor.b - 0.2f, GameManager.Instance.levelUIColor.a);
    }

    public void Update() {
        pingSample = Mathf.Lerp(pingSample, PhotonNetwork.GetPing(), Mathf.Clamp01(Time.unscaledDeltaTime * 0.5f));
        if (pingSample == float.NaN)
            pingSample = 0;

        uiDebug.text = "<mark=#000000b0 padding=\"20, 20, 20, 20\"><font=\"defaultFont\">Ping: " + (int) pingSample + "ms</font>";
        
        PlayerController cobalter = player;
        foreach (var player in GameManager.Instance.players) {
            if (player.cobalting > 0 && cobalter.cobalting > 0) {
                protectedCobaltEffect.SetActive(true);
            } else if (player.cobalting > 0  && cobalter.cobalting <= 0) {
                cobaltEffect.SetActive(true);
            } else if (player.cobalting <= 0) {
                cobaltEffect.SetActive(false);
                protectedCobaltEffect.SetActive(false);
            }
        }

        //Player stuff update.
        if (!player && GameManager.Instance.localPlayer)
            player = GameManager.Instance.localPlayer.GetComponent<PlayerController>();

        if (!player) {
            if (!uiHidden)
                ToggleUI(true);

            return;
        }
        Utils.GetCustomProperty(Enums.NetRoomProperties.DeathmatchGame, out bool deathmatch);
        if (deathmatch)
            starsParent.SetActive(false);

        Utils.GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out bool teamsGame);
        Utils.GetCustomProperty(Enums.NetRoomProperties.DeathmatchGame, out bool dm);
        Utils.GetCustomProperty(Enums.NetRoomProperties.ShareStars, out int starSharing);
        if (!teamsGame || starSharing != 1 || dm) 
            teamsHeader.SetActive(false);

        if (GameManager.Instance.teamController.redTeamMembers.Count == 0)
            redStars.transform.parent.gameObject.SetActive(false);
        else if (GameManager.Instance.teamController.redTeamMembers.Count > 0)
            redStars.transform.parent.gameObject.SetActive(true);

        if (GameManager.Instance.teamController.yellowTeamMembers.Count == 0)
            yellowStars.transform.parent.gameObject.SetActive(false);
        else if (GameManager.Instance.teamController.yellowTeamMembers.Count > 0)
            yellowStars.transform.parent.gameObject.SetActive(true);

        if (GameManager.Instance.teamController.greenTeamMembers.Count == 0)
            greenStars.transform.parent.gameObject.SetActive(false);
        else if (GameManager.Instance.teamController.greenTeamMembers.Count > 0)
            greenStars.transform.parent.gameObject.SetActive(true);

        if (GameManager.Instance.teamController.blueTeamMembers.Count == 0)
            blueStars.transform.parent.gameObject.SetActive(false);
        else if (GameManager.Instance.teamController.blueTeamMembers.Count > 0)
            blueStars.transform.parent.gameObject.SetActive(true);

        if (GameManager.Instance.teamController.purpleTeamMembers.Count == 0)
            purpleStars.transform.parent.gameObject.SetActive(false);
        else if (GameManager.Instance.teamController.purpleTeamMembers.Count > 0)
            purpleStars.transform.parent.gameObject.SetActive(true);
            
        if (uiHidden)
            ToggleUI(false);

        UpdateStoredItemUI();
        UpdateTextUI();
        
    }

    private void ToggleUI(bool hidden) {
        uiHidden = hidden;

        starsParent.SetActive(!hidden);
        livesParent.SetActive(!hidden);
        coinsParent.SetActive(!hidden);
        timerParent.SetActive(!hidden);
        pointsParent.SetActive(!hidden);
        comboParent.SetActive(!hidden);
    }

    private void UpdateStoredItemUI() {
        if (!player)
            return;

        itemReserve.sprite = player.storedPowerup != null ? player.storedPowerup.reserveSprite : storedItemNull;
    }

    private void UpdateTextUI() {
        if (!player || GameManager.Instance.gameover)
            return;

        Utils.GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out bool teams);
        Utils.GetCustomProperty(Enums.NetRoomProperties.ShareCoins, out bool shareCoins);
        Utils.GetCustomProperty(Enums.NetRoomProperties.ShareStars, out int shareStars);

        bool coinSharing = teams && shareCoins;
        bool starSharing = teams && shareStars == 1;
        GameManager gm = GameManager.Instance;
        TeamController tm = gm.teamController;
        if (starSharing) {
            stars = tm.GetTeamStars(player.team);
            uiStars.text = Utils.GetSymbolString("" + (player.team + 5), Utils.teamCoinSymbols) + Utils.GetSymbolString("x" + stars + "/" + GameManager.Instance.starRequirement); 
        } else {
            if (player.stars != stars) {
                stars = player.stars;
                uiStars.text = Utils.GetSymbolString("Sx" + stars + "/" + GameManager.Instance.starRequirement);
            }
        }
        if (coinSharing) {
            coins = tm.GetTeamCoins(player.team);
            uiCoins.text = Utils.GetSymbolString("" + player.team, Utils.teamCoinSymbols) + Utils.GetSymbolString("x" + coins + "/" + tm.TeamCoinRequirement(player.team));
        } else {
            if (player.coins != coins) {
                coins = player.coins;
                uiCoins.text = Utils.GetSymbolString("Cx" + coins + "/" + GameManager.Instance.coinRequirement);
            }
        }

        if (player.lives >= 0) {
            if (player.lives != lives) {
                lives = player.lives;
                uiLives.text = Utils.GetCharacterData(player.photonView.Owner).uistring + Utils.GetSymbolString("x" + lives);
            }
        } else {
            livesParent.SetActive(false);
        }

        if (player.combo >= 1 && GameManager.Instance.tideSurfingLevel) {
            if (player.combo != combo) {
                combo = player.combo;
                uiCombo.text = "Combo X" + combo;
            }
            comboParent.SetActive(true);
        } else {
            comboParent.SetActive(false);
        }
        if (GameManager.Instance.tideSurfingLevel) {
            pointsParent.SetActive(true);
            if (player.points != points) {
                points = player.points;
                uiPoints.text = Utils.GetSymbolString(points.ToString());
            }
        } else {
            pointsParent.SetActive(false);
        }
        if (combo >= 1 && GameManager.Instance.tideSurfingLevel) {
            comboRating.text = combo switch {
                int n when n > 15 => "EXCELLENT!",
                int n when n > 11 => "GREAT!",
                int n when n > 7  => "GOOD!",
                int n when n > 3  => "OK!",
                _ => ""
            };
            comboRating.color = combo switch {
                int n when n > 15 => new Color(1f, 0f, 0.1f, 1f),
                int n when n > 11 => new Color(1f, 0.35f, 0f, 1f),
                int n when n > 7  => new Color(0.75f, 1f, 0f, 1f),
                int n when n > 3  => new Color(0.5f, 1f, 0f, 1f),
                _ => new Color(1f, 1f, 1f, 1f) 
            };
        }
        redStars.text = Utils.GetSymbolString("" + GameManager.Instance.redTeamStars);
        yellowStars.text = Utils.GetSymbolString("" + GameManager.Instance.yellowTeamStars);
        greenStars.text = Utils.GetSymbolString("" + GameManager.Instance.greenTeamStars);
        blueStars.text = Utils.GetSymbolString("" + GameManager.Instance.blueTeamStars);
        purpleStars.text = Utils.GetSymbolString("" + GameManager.Instance.purpleTeamStars);

        if (GameManager.Instance.timedGameDuration > 0) {
            int seconds = Mathf.CeilToInt((GameManager.Instance.endServerTime - PhotonNetwork.ServerTimestamp) / 1000f);
            seconds = Mathf.Clamp(seconds, 0, GameManager.Instance.timedGameDuration);
            if (seconds != timer) {
                timer = seconds;
                uiCountdown.text = Utils.GetSymbolString("cx" + (timer / 60) + ":" + (seconds % 60).ToString("00"));
            }
            timerParent.SetActive(true);

            if (GameManager.Instance.endServerTime - PhotonNetwork.ServerTimestamp < 0) {
                if (timerMaterial == null) {
                    CanvasRenderer cr = uiCountdown.transform.GetChild(0).GetComponent<CanvasRenderer>();
                    cr.SetMaterial(timerMaterial = new(cr.GetMaterial()), 0);
                }

                float partialSeconds = (GameManager.Instance.endServerTime - PhotonNetwork.ServerTimestamp) / 1000f % 2f;
                byte gb = (byte) (Mathf.PingPong(partialSeconds, 1f) * 255);
                timerMaterial.SetColor("_Color", new Color32(255, gb, gb, 255));
            }
        } else {
            timerParent.SetActive(false);
        }

        if (player.shrunk > 0) { 
            uiMiniTimer.text = Utils.GetSymbolString("c" + (Mathf.RoundToInt(player.shrunk)));
            miniParent.SetActive(true);
        } else {
            miniParent.SetActive(false);
        }
    }

    public GameObject CreatePlayerIcon(PlayerController player) {
        GameObject trackObject = Instantiate(playerTrackTemplate, playerTrackTemplate.transform.parent);
        TrackIcon icon = trackObject.GetComponent<TrackIcon>();
        icon.target = player.gameObject;

        trackObject.SetActive(true);

        return trackObject;
    }
}
