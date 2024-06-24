using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using NSMB.Utils;
using TMPro;
public class PointsBalloon : MonoBehaviour {

    public int points = 10;
    public TMP_Text countText;
    public bool randomCycle;
    public List<int> randomInts;
    public float randomInterval = 0.5f;
    public float restartRandomInterval = 0.25f;
    public AudioSource sfx;
    public NumberParticle numColor;
    public int nextNumber = 0;

    public void Start() {
        countText = GetComponentInChildren<TMP_Text>();
        numColor = GetComponentInChildren<NumberParticle>();
        sfx = GetComponent<AudioSource>();
    }

    public void Update() {
        countText.text = Utils.GetSymbolString((points > 0 ? "+" : ""), Utils.numberSymbols) + Utils.GetSymbolString(points.ToString(), Utils.numberSymbols);
        numColor.ApplyColor(points < 0 ? new Color(1f, 0f, 0f, 1f) : points == 0 ? new Color(0.6f, 0.6f, 0.6f, 1f) : points > 0 ? new Color(0f, 1f, 0f, 1f) : new Color(1f, 1f, 1f, 1f));
        if (randomCycle && ((randomInterval -= Time.fixedDeltaTime) <= 0))
            CycleThroughPoints();
    }

    public void CycleThroughPoints() {
        randomInterval = restartRandomInterval;
        points = randomInts[nextNumber];
        nextNumber++;
        sfx.PlayOneShot(Enums.Sounds.UI_Cursor.GetClip());
        if (nextNumber >= randomInts.Count)
            nextNumber = 0;

    }
}
