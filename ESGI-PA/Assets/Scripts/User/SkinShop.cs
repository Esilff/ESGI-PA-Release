using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SkinShop : MonoBehaviour
{
    // Liste de GameObject Canvas
    public List<Canvas> canvasList = new List<Canvas>();
    public List<Button> buttonList = new List<Button>();
    public RectTransform boutiqueCanvas;
    public RectTransform mainCanvas;
    public Text moneyText;

    void Start()
    {
        if (canvasList == null)
        {
            Debug.LogError("canvasList non assigné dans l'inspecteur.");
            return;
        }

        StartCoroutine(GetSkinDataFromAPI());
    }
    void OnButtonClick(int id, int price)
    {
        Debug.Log("Le bouton a été cliqué ! ID : " + id + ", Prix : " + price);
        StartCoroutine(BuySkin(id, price));
        StartCoroutine(UpdateMoneyInDatabase(PlayerInfo.Instance.id, price));
        moneyText.text = PlayerInfo.Instance.username + "    Money : " + PlayerInfo.Instance.money + " $";
        
    }
    IEnumerator BuySkin(int id, int price)
    {
        // Vérifier si le joueur a assez d'argent pour acheter la peau
        if (PlayerInfo.Instance.money < price)
        {
            Debug.LogError("Pas assez d'argent pour acheter cette peau !");
            yield break;
        }
        Debug.Log("Début de la requête API...");
        Debug.Log("ID du joueur : " + PlayerInfo.Instance.id);
        Debug.Log("ID de la peau : " + id);
        Debug.Log("Prix de la peau : " + price);

        // Créer un objet pour les données de formulaire
        SkinPurchaseData formData = new SkinPurchaseData
        {
            id_user = PlayerInfo.Instance.id,
            id_skin = id,
            price = price
        };

        // Sérialiser les données en JSON
        string json = JsonUtility.ToJson(formData);
        Debug.Log("JSON : " + json);

        // Créer une requête POST
        UnityWebRequest www = new UnityWebRequest("http://127.0.0.1:5000/achat", "POST");

        // Convertir le JSON en bytes et l'ajouter comme données de corps de requête
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);

        // Définir l'en-tête Content-Type sur application/json
        www.SetRequestHeader("Content-Type", "application/json");

        // Attendre la fin de la requête
        yield return www.SendWebRequest();

        // Vérifier les erreurs
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erreur lors de la requête API : " + www.error);
        }
        else
        {
            // Réponse réussie, déduire le prix de l'argent du joueur
            PlayerInfo.Instance.money -= price;
        
            // Afficher un message de succès
            Debug.Log("Achat réussi !");
        }
    }
    
    IEnumerator UpdateMoneyInDatabase(int id_user, int amount)
    {
        Debug.Log("Début de la mise à jour de l'argent dans la base de données...");

        Updatemoney formData = new Updatemoney
        {
            id_user = id_user,
            amount = amount
        };

        string json = JsonUtility.ToJson(formData);

        UnityWebRequest www = new UnityWebRequest("http://127.0.0.1:5000/update_money", "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erreur lors de la mise à jour de l'argent dans la base de données : " + www.error);
        }
        else
        {
            Debug.Log("Mise à jour de l'argent dans la base de données réussie !");
        }
    }


    IEnumerator GetSkinDataFromAPI()
    {
        Debug.Log("Début de la requête API...");
        using (UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1:5000/skins"))
        {
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                string jsonResponse = www.downloadHandler.text;

                // Log the raw server response
                Debug.Log("Réponse JSON reçue : " + jsonResponse);

                Debug.Log("Tentative de déserialization du JSON...");
                // Déclarez skinDataList avant l'appel à FromJson
                SkinDataList skinDataContainer = new SkinDataList();

                try
                {
                    // Maintenant, essayez de désérialiser le JSON dans skinDataContainer
                    skinDataContainer = JsonUtility.FromJson<SkinDataList>(jsonResponse);
                    Debug.Log("Désérialisation du JSON réussie.");
                    Debug.Log("Nombre de skins : " + skinDataContainer.skins.Count);
                    // Afficher le contenu de skinDataContainer

                    for (int i = 0; i < skinDataContainer.skins.Count; i++)
                    {
                        SkinData skin = skinDataContainer.skins[i];
                        if (i < canvasList.Count)
                        {
                            Canvas canvas = canvasList[i];
                            // Obtenir la référence à l'Image
                            Image image = canvas.GetComponentInChildren<Image>();
                            // Charger l'image à partir du chemin de l'image
                            StartCoroutine(LoadImageFromURL(skin.imagepath, image));
                            Button button = buttonList[i];
                            button.onClick.AddListener(() => OnButtonClick(skin.id, skin.price));
                            
                            

                            // Obtenir la référence au Text
                            Debug.Log("eh je suis la");
                            Debug.Log("Nombre de composants Text dans le Canvas : " + canvas.GetComponentsInChildren<Text>().Length);
                            Text[] texts = canvas.GetComponentsInChildren<Text>();
                            Debug.Log("Nombre de composants Text dans le Canvas : " + texts.Length);
                            if (texts.Length == 4)
                            {
                                Debug.Log("Nombre de composants Text dans le Canvas : " + texts.Length);
                                // Modifiez les textes ici
                                texts[0].text = skin.name;
                                texts[1].text = skin.description;
                                texts[2].text = "Price: " + skin.price.ToString();
                            }
                            else
                            {
                                Debug.LogWarning("Il y a un nombre incorrect de composants Text dans le Canvas : " + canvas.name);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Erreur lors de la conversion du JSON : " + ex.Message);
                    Debug.Log("JSON non valide ou format inattendu.");
                }

            }
            else
            {
                Debug.LogError("Erreur lors de la requête API : " + www.error);
            }
        }
    }

    IEnumerator LoadImageFromURL(string url, Image image)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erreur lors du chargement de l'image : " + request.error);
        }
        else
        {
            image.sprite = Sprite.Create(((DownloadHandlerTexture)request.downloadHandler).texture, new Rect(0, 0, ((DownloadHandlerTexture)request.downloadHandler).texture.width, ((DownloadHandlerTexture)request.downloadHandler).texture.height), new Vector2(0.5f, 0.5f));
        }
    }
    public void ToMain()
    {
        boutiqueCanvas.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);
    }
}

[System.Serializable]
public class SkinDataList
{
    public List<SkinData> skins;
}

[System.Serializable]
public class SkinData
{
    public int id;
    public string name;
    public int value;
    public string data_added; 
    public string character; 
    public string imagepath;
    public string description;
    public int price;
}
[System.Serializable]
public class SkinPurchaseData
{
    public int id_user;
    public int id_skin;
    public int price;
}

[System.Serializable]
public class Updatemoney
{
    public int id_user;
    public int amount;
}