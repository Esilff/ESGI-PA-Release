using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SkinItemUI : MonoBehaviour
{
    public Image skinImage;
    public Text skinNameText;
    public Text skinPriceText;

    public void Initialize(SkinData skinData)
    {
        if (skinData == null)
        {
            Debug.LogError("skinData est null lors de l'appel à Initialize().");
            return;
        }

        StartCoroutine(LoadSkinImage(skinData.imagepath));

        if (skinNameText == null)
        {
            Debug.LogError("skinNameText est null lors de l'appel à Initialize().");
        }
        else
        {
            skinNameText.text = skinData.name;
        }

        if (skinPriceText == null)
        {
            Debug.LogError("skinPriceText est null lors de l'appel à Initialize().");
        }
        else
        {
            skinPriceText.text = "Prix : " + skinData.price.ToString();
        }
    }

    IEnumerator LoadSkinImage(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("imagePath est null ou vide lors de l'appel à LoadSkinImage().");
            yield break;
        }

        Debug.Log("Début du chargement de l'image du skin depuis : " + imagePath);

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imagePath))
        {
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                if (texture == null)
                {
                    Debug.LogError("Échec du chargement de la texture depuis " + imagePath);
                    yield break;
                }

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                if (skinImage == null)
                {
                    Debug.LogError("skinImage est null lors de la tentative d'attribution du sprite.");
                }
                else
                {
                    skinImage.sprite = sprite;
                }

                Debug.Log("Chargement de l'image du skin terminé avec succès.");
            }
            else
            {
                Debug.LogError("Erreur lors du chargement de l'image du skin : " + www.error);
            }
        }
    }
}
