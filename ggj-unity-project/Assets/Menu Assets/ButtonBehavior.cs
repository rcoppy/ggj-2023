using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonBehavior : MonoBehaviour
{
    [SerializeField] GameObject _pointerObject;

    private RectTransform _pointerRect;
    private RectTransform _selfRect;
    
    private void Start()
    {
        _pointerRect = _pointerObject.GetComponent<RectTransform>();
        _selfRect = GetComponent<RectTransform>();
    }

    public void HandleHover()
    {
        Debug.Log("hover on " + gameObject.name);
        var offset = _pointerRect.rect.width + 0.04f * _pointerRect.rect.width;
        _pointerRect.position = new Vector2(_selfRect.rect.xMin - offset, _selfRect.position.y);
        
        // TODO: play sound effect
    }

    public void DoChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    } 
}
