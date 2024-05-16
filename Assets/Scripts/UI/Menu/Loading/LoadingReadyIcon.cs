using NSMB.Utils;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingReadyIcon : MonoBehaviour {
    public TMP_Text readyText;
    public void Start() {
        GetComponent<Image>().sprite = Utils.GetCharacterData().readySprite;
        readyText.colorGradientPreset = Utils.GetCharacterData().readyTextGradient;
    }
}
