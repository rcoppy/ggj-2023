using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light))]
public class ShadowSway : MonoBehaviour
{
    private UniversalAdditionalLightData _lightData;

    [SerializeField] private float _swayConstant1 = 0.01f; 
    [SerializeField] private float _swayConstant2 = 0.03f;

    [SerializeField] private float _swayMagnitude = 20f; 
    
    // Start is called before the first frame update
    void Start()
    {
        _lightData = GetComponent<UniversalAdditionalLightData>();

        if (_lightData == null)
        {
            enabled = false; 
        }
    }

    // Update is called once per frame
    void Update()
    {
        var x = _swayMagnitude * (Mathf.Sin(_swayConstant1 * Time.time) + 0.5f * Mathf.Sin(_swayConstant2 * Time.time)); 
        var y = _swayMagnitude * (Mathf.Cos(_swayConstant2 * Time.time) + 0.5f * Mathf.Cos(_swayConstant1 * Time.time));

        _lightData.lightCookieOffset = new Vector2(x, y);
    }
}
