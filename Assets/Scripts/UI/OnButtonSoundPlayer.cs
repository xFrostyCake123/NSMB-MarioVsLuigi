using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using NSMB.Utils;
public class OnButtonSoundPlayer : MonoBehaviour {
    public void PlaySound(Enums.Sounds sound, AudioSource sfx) {
        sfx.PlayOneShot(sound.GetClip());
    }
}
