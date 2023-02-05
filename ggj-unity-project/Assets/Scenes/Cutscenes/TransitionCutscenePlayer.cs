using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class TransitionCutscenePlayer : MonoBehaviour
{
   public VideoPlayer activeVideo;
   
   public void DoChangeScene(string sceneName)
   {
       SceneManager.LoadScene(sceneName);
   } 
   
   private void Start()
   {
     activeVideo.loopPointReached += ActiveVideoOnloopPointReached;
   }

   public UnityEvent VideoComplete;
   private void ActiveVideoOnloopPointReached(VideoPlayer source)
   {
      VideoComplete?.Invoke(); 
   }


   public void Update()
   {
       if (Input.GetKey(KeyCode.F))
       {
           activeVideo.playbackSpeed = 20f;
       }
   }
   
   
}
