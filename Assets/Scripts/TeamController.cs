using System.Collections.Generic;
using System.Linq;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeamController : MonoBehaviourPun
{
    public List<PlayerController> redTeamMembers, yellowTeamMembers, greenTeamMembers, blueTeamMembers, purpleTeamMembers;
    public bool teamsMatch, shareStars, shareCoins;

    private Dictionary<int, int> teamStars = new();
    private Dictionary<int, int> teamCoins = new();
    private Dictionary<int, int> teamCoinReq = new();

    // list of all players in the game
    public List<PlayerController> players = new();

    public void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.ShareStars, out int sharingStars);
        Utils.GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out teamsMatch);
        Utils.GetCustomProperty(Enums.NetRoomProperties.ShareCoins, out shareCoins);

        shareStars = sharingStars == 1;
        players = GameManager.Instance.players;
        // initialize stars dictionary with team numbers
        for (int i = 0; i <= 4; i++) {
        
            teamStars[i] = 0;
        }
        // initialize coins dictionary with team numbers
        for (int i = 0; i <= 4; i++) {
        
            teamCoins[i] = 0;
        }
        // Calculate the total stars and coins for each team
        CalculateTeamStars();
        CalculateTeamStars();
    }
    public void Update() {
        CalculateTeamStars();
        CalculateTeamCoins();
    }
    public int LeaderTeamStars() {
        GameManager gm = GameManager.Instance;
        int red = gm.redTeamStars;
        int yellow = gm.yellowTeamStars;
        int green = gm.greenTeamStars;
        int blue = gm.blueTeamStars;
        int purple = gm.purpleTeamStars;

        int leadingTeam = -1;
        int maxStars = -1;

        // check red team
        if (red > maxStars) {
            maxStars = red;
            leadingTeam = 0;
        }
        // check yellow team
        if (yellow > maxStars) {
            maxStars = yellow;
            leadingTeam = 1;
        }
        // check green team
        if (green > maxStars) {
            maxStars = green;
            leadingTeam = 2;
        }
        // check blue team
        if (blue > maxStars) {
            maxStars = blue;
            leadingTeam = 3;
        }
        // check purple team
        if (purple > maxStars) {
            maxStars = purple;
            leadingTeam = 4;
        }

        return maxStars;
    }
    
    public void AddPlayerToTeam(PlayerController player) {
        if (player.team == 0) {
            redTeamMembers.Add(player);
        } else if (player.team == 1) {
            yellowTeamMembers.Add(player);
        } else if (player.team == 2) {
            greenTeamMembers.Add(player);
        } else if (player.team == 3) {
            blueTeamMembers.Add(player);
        } else if (player.team == 4) {
            purpleTeamMembers.Add(player);
        }
    }
    public void GrantTeamPowerup(int team) {
        if (!teamsMatch)
            return;
            
        if (team == 0) {
            foreach (PlayerController red in redTeamMembers) {
                red.SpawnTeamItem();
                red.coins = 0;
            }
        }
        if (team == 1) {
            foreach (PlayerController yellow in yellowTeamMembers) {
                yellow.SpawnTeamItem();
                yellow.coins = 0;
            }
        }
        if (team == 2) {
            foreach (PlayerController green in greenTeamMembers) {
                green.SpawnTeamItem();
                green.coins = 0;
            }
        }
        if (team == 3) {
            foreach (PlayerController blue in blueTeamMembers) {
                blue.SpawnTeamItem();
                blue.coins = 0;
            }
        }
        if (team == 4) {
            foreach (PlayerController purple in purpleTeamMembers) {
                purple.SpawnTeamItem();
                purple.coins = 0;
            }
        }
    }
    public bool IsPlayerTeammate(PlayerController thisPlayer, PlayerController otherPlayer) {
        if (!teamsMatch) 
            return false;
        return thisPlayer.team.Equals(otherPlayer.team);
    }
    public void CalculateTeamStars() {
        if (!teamsMatch)
            return;
        for (int i = 0; i <= 4; i++) {
        
            teamStars[i] = 0;
        } 

        // Iterate through each player and add their stars to the respective team
        foreach (PlayerController teamPlayer in players)
        {
            if (teamPlayer == null)
                return;

            if (teamStars.ContainsKey(teamPlayer.team))
            {
                teamStars[teamPlayer.team] += teamPlayer.stars;
            }
        }

    }

    public void CalculateTeamCoins() {
        if (!teamsMatch)
            return;
        for (int i = 0; i <= 4; i++) {
        
            teamCoins[i] = 0;
        } 

        // Iterate through each player and add their stars to the respective team
        foreach (PlayerController teamPlayer in players)
        {
            if (teamPlayer == null)
                return;
            if (teamCoins.ContainsKey(teamPlayer.team))
            {
                teamCoins[teamPlayer.team] += teamPlayer.coins;
            }
        }

    }
    public void CalculateTeamCoinRequirement() {
        if (!teamsMatch || !shareCoins)
            return;
        for (int i = 0; i <= 4; i++) {
        
            teamCoinReq[i] = 0;
        } 

        // Iterate through each player and add their stars to the respective team
        foreach (PlayerController teamPlayer in players)
        {
            if (teamPlayer == null)
                return;

            if (teamCoinReq.ContainsKey(teamPlayer.team))
            {
                teamCoinReq[teamPlayer.team] += GameManager.Instance.coinRequirement;
            }
        }    
    }
    public int GetTeamStars(int team)
    {
        if (teamStars.ContainsKey(team))
        {
            return teamStars[team];
        }
        return 0;
    }
    public int GetTeamCoins(int team)
    {
        if (teamCoins.ContainsKey(team))
        {
            return teamCoins[team];
        }
        return 0;
    }
    public int TeamCoinRequirement(int team)
    {
        if (teamCoinReq.ContainsKey(team))
        {
            return teamCoinReq[team];
        }
        return 0;
    }
}