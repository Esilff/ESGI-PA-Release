using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICup : MonoBehaviour
{
    [SerializeField] private string cupName;
    [SerializeField] private List<string> maps;
    [SerializeField] private CupPreview preview;
    [SerializeField] private GameConfiguration config;
    [SerializeField] private MenuController menu;

    private static bool _hoverLocked;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public void OnClick()
    {
        if (config.mode == GameMode.Cup)
        {
            config.circuits = maps;
        }
        
    }

    public void LockHover()
    {
        _hoverLocked = !_hoverLocked;
    }

    public void OnHover()
    {
        if (_hoverLocked) return;
        preview.CupName = cupName;
        preview.CupText = maps;
    }
}
