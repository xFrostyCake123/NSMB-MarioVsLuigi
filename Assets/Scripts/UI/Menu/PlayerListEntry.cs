using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using Photon.Pun;
using Photon.Realtime;
using NSMB.Utils;

public class PlayerListEntry : MonoBehaviour {

    public Player player;
    public bool isPartOfGameManager;

    [SerializeField] private TMP_Text nameText, pingText;
    [SerializeField] private Image colorStrip, pingColorStrip;

    [SerializeField] private RectTransform background, options;
    [SerializeField] private GameObject blockerTemplate, firstButton;

    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private LayoutElement layout;

    [SerializeField] private GameObject[] adminOnlyOptions;

    private GameObject blockerInstance;

    private void OnDestroy() {
        if (blockerInstance)
            Destroy(blockerInstance);
    }

    public void Update() {
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
        } 
    }

    public void UpdateText() {
        colorStrip.color = Utils.GetPlayerColor(player, 1f, 1f);
	    pingColorStrip.color = Utils.GetPlayerColor(player, 1f, 1f);
        enabled = player.HasRainbowName();

        string permissionSymbol = "";
        if (player.IsMasterClient)
            permissionSymbol += "<sprite=5>";

        Utils.GetCustomProperty(Enums.NetPlayerProperties.Status, out bool status, player.CustomProperties);
        if (status)
            permissionSymbol += "<sprite=26>";

        string characterSymbol = Utils.GetCharacterData(player).uistring;
        Utils.GetCustomProperty(Enums.NetPlayerProperties.Ping, out int ping, player.CustomProperties);

        string pingColor;
        if (ping < 0) {
            pingColor = "black";
        } else if (ping < 80) {
            pingColor = "#00b900";
        } else if (ping < 120) {
            pingColor = "orange";
        } else {
            pingColor = "red";
        }

        nameText.text = permissionSymbol + characterSymbol + player.GetUniqueNickname();
        pingText.text = $"<color={pingColor}>{ping}";

        Transform parent = transform.parent;
        int childIndex = 0;
        for (int i = 0; i < parent.childCount; i++) {
            if (parent.GetChild(i) != gameObject)
                continue;

            childIndex = i;
            break;
        }

        layout.layoutPriority = transform.parent.childCount - childIndex;
    }

    public void ShowDropdown() {
        if (blockerInstance)
            Destroy(blockerInstance);

        bool admin = PhotonNetwork.IsMasterClient && !player.IsMasterClient;
        foreach (GameObject option in adminOnlyOptions) {
            option.SetActive(admin);
        }

        Canvas.ForceUpdateCanvases();

        blockerInstance = Instantiate(blockerTemplate, rootCanvas.transform);
        RectTransform blockerTransform = blockerInstance.GetComponent<RectTransform>();
        blockerTransform.offsetMax = blockerTransform.offsetMin = Vector2.zero;
        blockerInstance.SetActive(true);

        background.offsetMin = new(background.offsetMin.x, -options.rect.height);
        options.anchoredPosition = new(options.anchoredPosition.x, -options.rect.height);

        EventSystem.current.SetSelectedGameObject(firstButton);
        MainMenuManager.Instance.sfx.PlayOneShot(Enums.Sounds.UI_Cursor.GetClip());
    }

    public void HideDropdown(bool didAction) {
        Destroy(blockerInstance);

        background.offsetMin = new(background.offsetMin.x, 0);
        options.anchoredPosition = new(options.anchoredPosition.x, 0);

        MainMenuManager.Instance.sfx.PlayOneShot((didAction ? Enums.Sounds.UI_Decide : Enums.Sounds.UI_Back).GetClip());
    }

    public void BanPlayer() {
        MainMenuManager.Instance.Ban(player);
        HideDropdown(true);
    }

    public void KickPlayer() {
        MainMenuManager.Instance.Kick(player);
        HideDropdown(true);
    }

    public void MutePlayer() {
        MainMenuManager.Instance.Mute(player);
        HideDropdown(true);
    }

    public void PromotePlayer() {
        if (isPartOfGameManager) {
            GameManager.Instance.AttemptTransferHost(player);
            HideDropdown(true);
        } else {
            MainMenuManager.Instance.Promote(player);
            HideDropdown(true);
        }
    }

    public void CopyPlayerId() {
        TextEditor te = new();
        te.text = player.UserId;
        te.SelectAll();
        te.Copy();
        HideDropdown(true);
    }
}