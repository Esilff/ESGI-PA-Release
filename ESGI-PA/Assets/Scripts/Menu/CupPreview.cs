using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CupPreview : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cupNameUI;
    [SerializeField] private List<GameObject> previews;
    [SerializeField] private GameConfiguration config;
    
    private string cupName;
    
    private List<string> cupText;

    private bool previewLocked = false;

    public bool PreviewLocked
    {
        get => previewLocked;
        set => previewLocked = value;
    }

    private void Start()
    {
        cupText = new();
    }

    // Start is called before the first frame update
    public string CupName
    {
        get => cupName;
        set
        {
            cupName = value;
            cupNameUI.text = cupName;
        }
    }

    public List<string> CupText
    {
        get => cupText;
        set
        {
            cupText = value;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        Debug.Log("Updating preview : " + cupText);
        for (var i = 0; i < previews.Count; i++)
        {
            previews[i].name = cupText[i];
        }
    }

    public void SetMap(GameObject preview)
    {
        if (config.mode is not (GameMode.Free or GameMode.Chrono)) return;
        config.circuits.Clear();
        config.circuits.Add(preview.name);
    }
}
