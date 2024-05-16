using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using NSMB.Utils;

public class Switch : MonoBehaviourPun {

    public GameObject targetObject;
    public Vector2 movePosition = new Vector2(0, 7);
    public bool pressed;
    public Enums.Sounds pressSound = Enums.Sounds.World_Switch_Pressed;
    public Animator animator;
    public BoxCollider2D switchCollision;
    public float secondsTillDestroy = 10.5f;
    public float eventDuration = 10f;
    public List<Enums.SwitchEvent> possibleEvents;

    public void Start() {
        animator = GetComponent<Animator>();
        switchCollision = GetComponent<BoxCollider2D>();
    }
    public void OnCollisionEnter2D(Collision2D collision) {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        Enums.SwitchEvent switchEvent = possibleEvents[Random.Range(0, possibleEvents.Count)];
        bool successfulPress = pressed;
        if (collision.gameObject != player.gameObject || pressed)
            return;
        
        if (collision.contacts[0].normal.y < -0.5f) {
            pressed = true;
            GameManager.Instance.inSwitchEvent = pressed;
            ActivateSwitch(switchEvent);
            animator.SetBool("pressed", true);
            PlaySoundEverywhere(pressSound);
            if (successfulPress)
                HandleLayerState();

        }
    }

    public void OnCollisionStay2D(Collision2D collision) {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        Enums.SwitchEvent switchEvent = possibleEvents[Random.Range(0, possibleEvents.Count)];
        bool successfulPress = pressed;
        if (collision.gameObject != player.gameObject || pressed)
            return;
        
        if (collision.contacts[0].normal.y < -0.5f) {
            pressed = true;
            GameManager.Instance.inSwitchEvent = pressed;
            ActivateSwitch(switchEvent);
            animator.SetBool("pressed", true);
            PlaySoundEverywhere(pressSound);
            if (successfulPress)
                HandleLayerState();

        }
    }
    public void ActivateSwitch(Enums.SwitchEvent selectedEvent) {
        if (selectedEvent == Enums.SwitchEvent.Move)
            StartCoroutine(MovingEvent());
        if (selectedEvent == Enums.SwitchEvent.AllStar)
            photonView.RPC(nameof(StarEvent), RpcTarget.All);
    }
    void HandleLayerState() {
        bool shouldntCollide = pressed;
        int layer = Layers.LayerDefault;
        if (shouldntCollide) {
            layer = Layers.LayerPassthrough;
        }

        gameObject.layer = layer;
    }
    public IEnumerator MovingEvent() {
        Vector2 originalPosition = targetObject.transform.position;
        Vector2 newPosition = originalPosition + movePosition;
        float currentTime = 0f;

        while (currentTime < eventDuration) {
            float time = currentTime / eventDuration;
            targetObject.transform.position = Vector3.Lerp(originalPosition, newPosition, time);
            currentTime += Time.deltaTime;
            yield return null;
        }
        
        targetObject.transform.position = newPosition;

    }
    
    [PunRPC]
    public void StarEvent() {
        foreach (PlayerController player in GameManager.Instance.players) {
          player.invincible = 10f;  
        }
    }
    public void PlaySoundEverywhere(Enums.Sounds sound) {
        GameManager.Instance.sfx.PlayOneShot(sound.GetClip());
    }
    public void Update() {
        if (pressed) {
            secondsTillDestroy -= Time.fixedDeltaTime;
            eventDuration -= Time.fixedDeltaTime;
        }
        if (eventDuration <= 0)
            pressed = false;
            Destroy(gameObject);
        
    }




}
