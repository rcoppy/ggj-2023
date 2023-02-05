using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    [SerializeField] private Animator creditsAnimator;
    [SerializeField] private float fastCreditsSpeed;
    
   
    
    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            creditsAnimator.speed = fastCreditsSpeed;
        }
        
        
    }
    
    public void LoadNewScene()
    {
        SceneManager.LoadScene("TitleScreen");        
    }
    
}
