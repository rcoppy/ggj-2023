using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity; 

namespace GGJ2022.Audio
{
    public class SFXAudioEventDriver : MonoBehaviour
    {
        // singleton pattern
        public static SFXAudioEventDriver Instance { get; set; }

        private void Awake()
        {
            // if (Instance != null && Instance != this)
            // {
            //     Destroy(gameObject);
            //     return;
            // }
            // else
            // {
            
            // need to inherit settings from most recent scene
                Instance = this;
            //}

            
            
            _sfxMapFile.RefreshMap();
            _sfxDict = _sfxMapFile.Map;
        }

        [SerializeField]
        GameObject _listener;

        [SerializeField]
        SFXFMODMap _sfxMapFile;

        [SerializeField]
        Dictionary<string, FMOD.GUID> _sfxDict; 

        [SerializeField]
        RelativeCharacterController _playerController;

        [SerializeField]
        CharacterAnimationManager _playerAnimationManager;
        
        // List of Banks to load
        [FMODUnity.BankRef]
        public List<string> Banks = new List<string>();

        bool CheckAreBanksLoaded()
        {
            if (FMODUnity.RuntimeManager.HaveAllBanksLoaded) return true;
            
            // Iterate all the Studio Banks and start them loading in the background
            // including the audio sample data
            foreach (var bank in Banks)
            {
                FMODUnity.RuntimeManager.LoadBank(bank, true);
            }

            return false; 
        }

        public static void StaticFireSFXEvent(string action)
        {
            Instance.FireSFXEvent(action);
        }

        public void FireSFXEvent(string action)
        {
            if (_sfxDict == null)
            {
                _sfxMapFile.RefreshMap();
                _sfxDict = _sfxMapFile.Map; 
            }

            FMOD.GUID guid = _sfxDict[action];

            FMODUnity.RuntimeManager.PlayOneShotAttached(guid, _listener);
        }

        private void OnEnable()
        {
            CheckAreBanksLoaded(); // should be handled by preloader
            
            try
            {
                _playerController.OnJumpStarted.AddListener(() => FireSFXEvent("Jump"));
                _playerController.OnJumpEnded.AddListener(() => FireSFXEvent("EndJump"));
                _playerAnimationManager.OnActionStarted += FireSFXEvent;
            }
            catch
            {
                Debug.LogError("couldn't find player controllers");
            }
        }

        private void OnDisable()
        {
            try
            {
                _playerController.OnJumpStarted.RemoveAllListeners();
                _playerController.OnJumpEnded.RemoveAllListeners();

                _playerAnimationManager.OnActionStarted -= FireSFXEvent;
            }
            catch
            {
                Debug.LogError("couldn't find player controllers");
            }
        }
    }
}
