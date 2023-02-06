using System;
using UnityEngine;
using GGJ2022.Audio;
using GGJ2022.EnemyAI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GGJ2022
{
    public class LevelStateManager : MonoBehaviour
    {
        public UnityEvent OnLevelStarted; 
        public static LevelStateManager Instance = null; 
        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            
            // this manager is transient; the most recently instantiated version should always be used.
            Instance = this;
        }
        public enum States
        {
            MainLevel, 
            BossFight,
            LevelCompleted
        }

        [SerializeField] GameObject _defaultPlayerObject;
        public GameObject PlayerObject => _defaultPlayerObject;

        private int _bodyCount = 0;
        public int BodyCount => _bodyCount;

        public float ScaleIncrement = 1.05f;

        private float originalPlayerScale = 1.0f;

        private void Start()
        {
            originalPlayerScale = PlayerObject.transform.localScale.magnitude;
            OnLevelStarted?.Invoke();
        }

        public int GetDamageRatio()
        {
            return Mathf.RoundToInt(PlayerObject.transform.localScale.magnitude / originalPlayerScale);
        }
        
        public int NumberOfEnemiesToComplete = 10;
        
        public void IncrementBodyCount()
        {
            _bodyCount++;

            if (_bodyCount > NumberOfEnemiesToComplete)
            {
                SetState(States.LevelCompleted);
            }
            OnEnemyKill?.Invoke();
            PlayerObject.transform.localScale *= ScaleIncrement;
        }

        public UnityEvent OnEnemyKill;
        
        private bool _isPaused = false;
        private bool _shouldPlayHorror = false; 
        private States _state = States.MainLevel;

        public void TogglePause()
        {
            SetIsPaused(!_isPaused);
        }
        
        /*public void SetShouldPlayHorror(bool flag)
        {
            _shouldPlayHorror = flag;

            if (_shouldPlayHorror)
            {
                MusicManager.Instance.SetMusicState(MusicManager.States.HorrorAmbience);
            }
            else
            {
                SetState(_state);
            }
        }*/
        
        public void SetIsPaused(bool flag)
        {
            _isPaused = flag;

            if (_isPaused)
            {
                Debug.Log("pause");
                MusicManager.Instance.SetMusicState(MusicManager.States.Paused);
            }
            else
            {
                Debug.Log("unpause");
                SetState(_state);
            }
        }

        public void TriggerBossTheme()
        {
            SetState(States.BossFight);
        }
        
        public void TriggerMainTheme()
        {
            SetState(States.MainLevel);
        }
        
        public string LevelCompletedSceneName;
        public string OnDeathSceneName;

        public void LoadDeathScene()
        {
            SceneManager.LoadScene(OnDeathSceneName);
        }
        
        public void SetState(States state)
        {
            // if (_state == state) return;

            _state = state;

            var music = MusicManager.States.Paused;

            if (!_isPaused)
            {
                switch (state)
                {
                    case States.BossFight:
                        music = MusicManager.States.BossTheme;
                        break;
                    case States.LevelCompleted:
                        music = MusicManager.States.Paused;
                        SceneManager.LoadScene(LevelCompletedSceneName);
                        break;
                    case States.MainLevel:
                        music = MusicManager.States.MainTheme;
                        break;
                }
            }

            MusicManager.Instance.SetMusicState(music);
        }
    }
}