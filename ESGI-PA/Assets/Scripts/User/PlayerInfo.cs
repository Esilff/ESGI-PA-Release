using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo Instance; // Singleton instance

    // Player's data
    public int id;
    public string username;
    public string email;
    public int money;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject); // The gameObject this script is attached to will not be destroyed when loading a new scene.
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Ensure that there is only one instance of this GameObject.
        }
    }
}
