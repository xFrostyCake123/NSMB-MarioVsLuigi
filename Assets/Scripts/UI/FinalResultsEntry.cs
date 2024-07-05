using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NSMB.Utils;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class FinalResultsEntry : MonoBehaviour {

    public TMP_Text nameText, starsText, livesText, coinsText, teamsText;
    [SerializeField] private Image background;
    public PlayerController target;
    public GameObject winnerEffect;
    public int playerId, stars, lives, totalCoins;
    private bool rainbowEnabled;
    public void Start() {
        if (!target) {
            enabled = false;
            return;
        }
        playerId = target.playerId;
        nameText.text = target.photonView.Owner.GetUniqueNickname();
        rainbowEnabled = target.photonView.Owner.HasRainbowName();
        background = GetComponent<Image>();
    }

    public void Update() {
        CheckForTextUpdate();
        if (rainbowEnabled) {
            if (nameText.text.Contains("FrostyCake")) {
                nameText.color = Utils.GetRainbowColor();
            } else if (nameText.text.Contains("BluCor")) {
                nameText.color = Utils.GetBlucorColor();
            } else if (nameText.text.Contains("vic")) {
                nameText.color = Utils.GetVicColor();
            } else if (nameText.text.Contains("KingKittyTurnip")) {
                nameText.color = Utils.GetTurnipColor();
            } else if (nameText.text.Contains("Foxyyy")) {
                nameText.color = Utils.GetFoxyyyColor();
            } else if (nameText.text.Contains("zomblebobble")) {
                nameText.color = Utils.GetZombleColor();
            } else if (nameText.text.Contains("Lust")) {
                nameText.color = Utils.GetLustColor();
            } else if (nameText.text.Contains("Windows10V")) {
                nameText.color = Utils.GetWindowsColor();
            } 
        }
    }

    public void CheckForTextUpdate() {
        Utils.GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out bool team);
        if (!target) {
            // our target lost all lives (or dc'd)
            background.color = new(0.4f, 0.4f, 0.4f, 1f);
            return;
        } else if (team && target != null) {
            background.color = target.team switch {
                int n when n == 0 => new(1f, 0.8f, 0.8f, 1f),
                int n when n == 1 => new(1f, 1f, 0.8f, 1f),
                int n when n == 2 => new(0.8f, 1f, 0.8f, 1f),
                int n when n == 3 => new(0.8f, 0.9f, 1f, 1f),
                int n when n == 4 => new(0.85f, 0.8f, 1f, 1f),
                _ => new(1f, 1f, 1f, 1f)
            };
        }

        stars = target.stars;
        lives = target.lives;
        totalCoins = target.totalCoins;
        UpdateText();
        HandleTextDisabling();
    }
    public void UpdateText() {
        if (lives >= 0)
            livesText.text = target.character.uistring + Utils.GetSymbolString("x" + lives);
        starsText.text = Utils.GetSymbolString("Sx" + stars + "/" + GameManager.Instance.starRequirement);
        nameText.text = target.photonView.Owner.GetUniqueNickname();
        coinsText.text = Utils.GetSymbolString("Cx" + totalCoins);
        teamsText.text = Utils.GetSymbolString((target.team + 5).ToString(), Utils.teamCoinSymbols);
    }
    public void HandleTextDisabling() {
        Utils.GetCustomProperty(Enums.NetRoomProperties.DeathmatchGame, out bool dm);
        Utils.GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out bool team);
        if (!team)
            teamsText.gameObject.SetActive(false);
        if (dm)
            starsText.gameObject.SetActive(false);
        if (lives < 0)
            livesText.gameObject.SetActive(false);
        
    }
    public class ResultsEntryComparer : IComparer<FinalResultsEntry> {
        public int Compare(FinalResultsEntry x, FinalResultsEntry y) {
            Utils.GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out bool teamResults);
            Utils.GetCustomProperty(Enums.NetRoomProperties.DeathmatchGame, out bool deathmatchResults);
            GameManager gm = GameManager.Instance;
            TeamController tm = gm.teamController;

            if (x.target == null ^ y.target == null)
                return x.target == null ? 1 : -1;

            if (teamResults) {
                if (Mathf.Max(0, tm.GetTeamStars(x.target.team)) == Mathf.Max(0, tm.GetTeamStars(y.target.team)))
                        return x.playerId - y.playerId;
                else 
                    return tm.GetTeamStars(y.target.team) - tm.GetTeamStars(x.target.team);
            }
            if (deathmatchResults) {
                if (Mathf.Max(0, x.lives) == Mathf.Max(0, y.lives))
                        return x.playerId - y.playerId;
                else
                    return y.lives - x.lives;
            }
            if (!deathmatchResults && !teamResults && (Mathf.Max(0, x.lives) == Mathf.Max(0, y.lives))) {
                if (Mathf.Max(0, x.totalCoins) == Mathf.Max(0, y.totalCoins))
                        return x.playerId - y.playerId;
                else
                    return y.totalCoins - x.totalCoins;
            }
            if (x.stars == y.stars || x.lives == 0 || y.lives == 0) {
                return y.lives - x.lives;
            }

            
            return y.stars - x.stars;
            
        }
    }
}