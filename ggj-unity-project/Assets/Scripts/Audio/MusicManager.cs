using System;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using Unity.VisualScripting;
using STOP_MODE = FMOD.Studio.STOP_MODE;

// using GGJ2022.EnemyAI;


namespace GGJ2022.Audio
{
    public class MusicManager : MonoBehaviour
    {
        public enum States
        {
            TitleScreen,
            Paused,
            MainTheme,
            BossTheme
        }
        public static MusicManager Instance = null;

        [SerializeField] private States _state = States.TitleScreen;
        
        private EventInstance _music;

        [SerializeField] private EventReference _musicReference;
        [SerializeField] private string _parameterName;

        private Dictionary<States, float> _parameterMap;
        
        void Awake()
        {
            #region singleton
            //Check if instance already exists
            if (Instance != null)
            {

                //if not, set instance to this
                Destroy(Instance.gameObject);
            }

            Instance = this; 
            
            
            /*//If instance already exists and it's not this:
            else if (Instance != this)
            {

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a CameraManager.
                Destroy(gameObject);
                return;
            }*/
            
            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);
            #endregion
            
            _music = FMODUnity.RuntimeManager.CreateInstance(_musicReference.Guid);
            
            _parameterMap = new Dictionary<States, float>()
            {
                [States.Paused] = 5.1f,
                [States.BossTheme] = 2.1f,
                [States.MainTheme] = 1.1f,
                [States.TitleScreen] = 0f
            };
        }

        private void Start()
        {
            SetMusicState(_state);
            _music.start();
        }

        public void SetMusicState(States state)
        {
            _state = state;
            _music.setParameterByName(_parameterName, _parameterMap[state]);
        }

        public void OnDestroy()
        {
            _music.stop(STOP_MODE.ALLOWFADEOUT);
            _music.release();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) SetMusicState(_state);
        }
#endif
    }
}