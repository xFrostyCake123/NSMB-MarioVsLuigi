using NSMB.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
public class FinalResultsList : MonoBehaviour {

    public static FinalResultsList instance;
    private static IComparer<FinalResultsEntry> entryComparer;
    public List<PlayerController> players;
    public FinalResultsEntry winnerEntry;
    [SerializeField] GameObject entryTemplate;

    public List<FinalResultsEntry> entries = new();

    public void Awake() {
        instance = this;
        if (entryComparer == null)
            entryComparer = new FinalResultsEntry.ResultsEntryComparer();
    }
    public void Start() {
        players = GameManager.Instance.players;
    }
    public void Update() {
        Reposition();
    }
    public void Populate(IEnumerable<PlayerController> availablePlayers) {
        foreach (PlayerController player in availablePlayers) {
            if (!player)
                continue;

            GameObject entryObj = Instantiate(entryTemplate, transform);
            entryObj.SetActive(true);
            entryObj.name = player.photonView.Owner.NickName;
            FinalResultsEntry entry = entryObj.GetComponent<FinalResultsEntry>();
            entry.target = player;

            entries.Add(entry);
        }

        Reposition();
    }
    public void Reposition() {
        entries.Sort(entryComparer);
        entries.ForEach(se => se.transform.SetAsLastSibling());
    }
    
    public void SelectWinningPlayer(Player winner) {
        Utils.GetCustomProperty(Enums.NetRoomProperties.TeamsMatch, out bool teamResults);
        if (winner != null) {
            foreach (FinalResultsEntry qualifiedEntry in entries) {
                if (qualifiedEntry.target.photonView.Owner == winner) 
                    winnerEntry = qualifiedEntry;

                if (winnerEntry != null) {
                    winnerEntry.winnerEffect.SetActive(true);
                    if (teamResults) {
                        if (winnerEntry.target.team == qualifiedEntry.target.team) {
                            qualifiedEntry.winnerEffect.SetActive(true);
                        }
                    }
                }
            }
            
        }
    }
}