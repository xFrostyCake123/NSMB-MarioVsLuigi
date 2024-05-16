using UnityEngine;
using UnityEngine.Audio;

public class OnButtonSoundPlayer : MonoBehaviour {
    public Enums.Sounds soundToPlay;
    public AudioSource sfx;
    public void PlaySound(Enums.Sounds sound) {
        sound = soundToPlay;
        sfx.PlayOneShot(sound.GetClip());
    }
}
