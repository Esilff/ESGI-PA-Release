using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class RegisterUser : MonoBehaviour
{
    public InputField usernameInput;
    public InputField emailInput;
    public InputField passwordInput;
    public Button submitButton;
    public Text errorText;
    public RectTransform loginCanvas;
    public RectTransform registerCanvas;
    public RectTransform mainCanvas;
    private string apiUrl = "http://127.0.0.1:5000/users"; // URL de l'API

    void Start()
    {
        submitButton.onClick.AddListener(Register);
    }

    public void Register()
    {
        StartCoroutine(RegisterUserCoroutine());
    }

    IEnumerator RegisterUserCoroutine()
    {
        // Créer un objet JSON contenant les données de l'utilisateur
        RUser newUser = new RUser
        {
            username = usernameInput.text,
            email = emailInput.text,
            password = passwordInput.text
        };

        // Convertir l'objet JSON en chaîne JSON
        string jsonData = JsonUtility.ToJson(newUser);

        // Envoyer la requête POST à l'API
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(apiUrl, jsonData))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                Debug.Log("Utilisateur créé avec succès");
                errorText.text = "Utilisateur créé avec succès";
                // Effacer les champs d'entrée
                usernameInput.text = "";
                emailInput.text = "";
                passwordInput.text = "";
                ToRegister();
            }
            else
            {
                Debug.LogError("Erreur lors de la création de l'utilisateur : " + www.error);
                errorText.text = "Erreur lors de la création de l'utilisateur";
            }
        }
    }
    public void ToRegister()
    {
        loginCanvas.gameObject.SetActive(true);
        registerCanvas.gameObject.SetActive(false);
    }
    public void ToMain()
    {
        registerCanvas.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);
    }
    public void ToLogin()
    {
        registerCanvas.gameObject.SetActive(false);
        loginCanvas.gameObject.SetActive(true);
    }
}

[System.Serializable]
public class RUser
{
    public string username;
    public string email;
    public string password;
}
