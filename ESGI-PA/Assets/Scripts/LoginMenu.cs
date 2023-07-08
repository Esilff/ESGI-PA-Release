using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class LoginMenu : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Button submitButton;
    public Text messageText; // Texte pour afficher le message de connexion
    public Text MoneyText;
    public RectTransform loginCanvas;
    public RectTransform registerCanvas;
    public RectTransform mainCanvas;

    private bool isLogin = false;

    private string apiUrl = "http://127.0.0.1:5000/users"; // URL de l'API

    void Start()
    {
        submitButton.onClick.AddListener(GetUser);
        isLogin = false;
    }

    public void GetUser()
    {
        if (!loginCanvas.gameObject.activeSelf)
        {
            loginCanvas.gameObject.SetActive(true);
        }
        
        StartCoroutine(GetUserCoroutine());
    }

    public void ToRegister()
    {
        loginCanvas.gameObject.SetActive(false);
        registerCanvas.gameObject.SetActive(true);
    }

    public void ToMain()
    {
        if (isLogin)
        {
            loginCanvas.gameObject.SetActive(false);
            mainCanvas.gameObject.SetActive(true);
        }
    }

    IEnumerator GetUserCoroutine()
    {
        // Construire l'URL de requête en ajoutant les paramètres de l'utilisateur
        string requestUrl = apiUrl;

        // Envoyer la requête GET à l'API
        using (UnityWebRequest www = UnityWebRequest.Get(requestUrl))
        {
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                // Désérialiser la réponse JSON
                string jsonResponse = www.downloadHandler.text;
                User[] users = JsonUtility.FromJson<UserArrayWrapper>("{\"users\":" + jsonResponse + "}").users;

                // Vérifier les utilisateurs dans la réponse
                foreach (User user in users)
                {
                    if (user.username == usernameInput.text && user.password == passwordInput.text)
                    {
                        Debug.Log("Connexion réussie");
                        PlayerInfo.Instance.id = user.id;
                        PlayerInfo.Instance.username = user.username;
                        PlayerInfo.Instance.email = user.email;
                        PlayerInfo.Instance.money = user.money;
                        MoneyText.text = user.username + "    Money : " + user.money+"$";
                        isLogin = true;
                        break; // Sortir de la boucle si la connexion est réussie
                    }
                }

                if (isLogin)
                {
                    messageText.text = "Connexion réussie";
                    ToMain();
                }
                else
                {
                    messageText.text = "Nom d'utilisateur ou mot de passe incorrect";
                }
            }
            else
            {
                Debug.LogError("Erreur lors de la requête API : " + www.error);
            }
        }
    }
}

[System.Serializable]
public class UserArrayWrapper
{
    public User[] users;
}

[System.Serializable]
public class User
{
    public int id;
    public string username;
    public string email;
    public string password;
    public int money;
}
