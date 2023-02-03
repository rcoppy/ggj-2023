//--------------------------------------------------------------------
//
// Source: https://fmod.com/resources/documentation-unity?version=2.02&page=examples-async-loading.html
//
// This is a Unity behaviour script that demonstrates how to build
// a loading screen that can be used at the start of a game to
// load both FMOD Studio data and Unity data without blocking the
// main thread.
//
// To make this work properly "Load All Event Data at Initialization"
// needs to be turned off in the FMOD Settings.
//
// This document assumes familiarity with Unity scripting. See
// https://unity3d.com/learn/tutorials/topics/scripting for resources
// on learning Unity scripting. 
//
//--------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

class FMODPreloader : MonoBehaviour
{
    // List of Banks to load
    [FMODUnity.BankRef]
    public List<string> Banks = new List<string>();

    // The name of the scene to load and switch to
    public string Scene = null;

    public float MinLoadingTime = 1f;
    private float _loadingStartTime; 

    public void Start()
    {
        StartCoroutine(LoadGameAsync());
    }

    void Update()
    {
        // Update the loading indication
    }

    IEnumerator LoadGameAsync()
    {
        _loadingStartTime = Time.time; 

        // Start an asynchronous operation to load the scene
        AsyncOperation async = SceneManager.LoadSceneAsync(Scene);

        // Don't lead the scene start until all Studio Banks have finished loading
        async.allowSceneActivation = false;

        // Iterate all the Studio Banks and start them loading in the background
        // including the audio sample data
        foreach (var bank in Banks)
        {
            FMODUnity.RuntimeManager.LoadBank(bank, true);
        }

        // Keep yielding the co-routine until all the bank loading is done
        // (for platforms with asynchronous bank loading)
        while (!FMODUnity.RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        // Keep yielding the co-routine until all the sample data loading is done
        while (FMODUnity.RuntimeManager.AnySampleDataLoading())
        {
            yield return null;
        }

        while (Time.time < _loadingStartTime + MinLoadingTime)
        {
            yield return null; 
        }

        // Allow the scene to be activated. This means that any OnActivated() or Start()
        // methods will be guaranteed that all FMOD Studio loading will be completed and
        // there will be no delay in starting events
        async.allowSceneActivation = true;

        // Keep yielding the co-routine until scene loading and activation is done.
        while (!async.isDone)
        {
            yield return null;
        }

    }
}