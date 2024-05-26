using System.Collections.Generic;
using System.Linq;
using NSMB.Utils;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeamController : MonoBehaviour
{
    public List<PlayerController> redTeamMembers, yellowTeamMembers, greenTeamMembers, blueTeamMembers, purpleTeamMembers;
    public bool teamsMatch, shareStars;

    private Dictionary<int, int> teamStars = new();

    // list of all players in the game
    public List<PlayerController> players = new();

    public void Start()
    {
        Utils.GetCustomProperty(Enums.NetRoomProperties.ShareStars, out int sharingStars);
        Utils.GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out teamsMatch);

        shareStars = sharingStars == 1;
        players = GameManager.Instance.players;
        // initialize dictionary with team numbers
        for (int i = 0; i <= 4; i++) {
        
            teamStars[i] = 0;
        }

        // Calculate the total stars for each team
        CalculateTeamStars();
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
    public bool IsPlayerTeammate(PlayerController thisPlayer, PlayerController otherPlayer) {
        if (!teamsMatch) 
            return false;
        return thisPlayer.team.Equals(otherPlayer.team);
    }
    public void CalculateTeamStars() {
        
        for (int i = 0; i <= 4; i++) {
        
            teamStars[i] = 0;
        } 

        // Iterate through each player and add their stars to the respective team
        foreach (PlayerController teamPlayer in players)
        {
            if (teamStars.ContainsKey(teamPlayer.team))
            {
                teamStars[teamPlayer.team] += teamPlayer.stars;
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
}