using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutManager : MonoBehaviour {

    public float fadeTimer, fadeTime, blackTime, totalTime;
    public Animator animator;
    private Image image;
    private Coroutine fadeCoroutine;

    public void Start() {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
    }

    private IEnumerator Fade() {
        while (fadeTimer > -totalTime) {
            fadeTimer -= Time.deltaTime;
            image.color = new(0, 0, 0, 1 - Mathf.Clamp01((Mathf.Abs(fadeTimer) - blackTime) / fadeTime));
            yield return null;
        }
        image.color = new(0, 0, 0, 0);
        fadeCoroutine = null;
    }

    public void FadeOutAndIn(float fadeTime, float blackTime) {
        this.fadeTime = fadeTime;
        this.blackTime = blackTime;
        totalTime = fadeTime + blackTime;
        fadeTimer = totalTime;

        if (fadeCoroutine == null)
            fadeCoroutine = StartCoroutine(Fade());
    }

    public void FrostedFade(Enums.FrostedFades frostedFade) {
        if (frostedFade == Enums.FrostedFades.Normal)
            animator.SetTrigger("Normal");
        else if (frostedFade == Enums.FrostedFades.Sad)
            animator.SetTrigger("Sad");
        else if (frostedFade == Enums.FrostedFades.Sleepy)
            animator.SetTrigger("Sleepy");
        else if (frostedFade == Enums.FrostedFades.ReverseNormal)
            animator.SetTrigger("ReverseNormal");
    }   
}