using UnityEngine;
using GGJ2022.Audio;
using GGJ2022.EnemyAI;

namespace GGJ2022
{
    public class LevelStateManager : MonoBehaviour
    {
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
        
        private bool _isPaused = false;
        private bool _shouldPlayHorror = false; 
        private States _state = States.MainLevel;

        public void TogglePause()
        {
            SetIsPaused(!_isPaused);
        }
        
        public void SetShouldPlayHorror(bool flag)
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
        }
        
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