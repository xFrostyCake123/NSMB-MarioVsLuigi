using UnityEngine;
using Photon.Pun;
using NSMB.Utils;

public class ObjectSwitchin : MonoBehaviour
{
    public GameObject[] gameObjects;
    public float switchInterval = 10f; // time interval between switching objects
    public bool loops = false; // if the array should loop back to the the start or not
    private int currentIndex = 0;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            SwitchGameObject();
            timer = 0f;
        }
    }

    private void SwitchGameObject()
    {
        // disable the current object
        if (currentIndex < gameObjects.Length && gameObjects[currentIndex] != null)
        {
            gameObjects[currentIndex].SetActive(false);
        }

        // move to the next object in the array
        currentIndex++;
        if (currentIndex >= gameObjects.Length)
        {
            if (loops)
            {
                currentIndex = 0; // loop back to the start
            }
            else
            {
                currentIndex = gameObjects.Length - 1; // stay at the last object
            }
        }

        // enable the next game object
        if (currentIndex < gameObjects.Length && gameObjects[currentIndex] != null)
        {
            gameObjects[currentIndex].SetActive(true);
        }
    }
}