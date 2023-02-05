using System;
using UnityEngine;
using System.Collections;
using GGJ2022.Audio;

namespace GGJ2022
{
    public class FootstepsReceiver : MonoBehaviour
    {
        [SerializeField]
        string _actionName = "Walk";

        [SerializeField] private bool _onlyPlaySometimes = false;
        [SerializeField] private int _playChance = 3;


        public void TriggerStep(String sfx = null)
        {
            if (Vector3.Distance(LevelStateManager.Instance.PlayerObject.transform.position, transform.position) >
                17f) return; 
            
            sfx = string.IsNullOrEmpty(sfx) ? _actionName : sfx; 
            
            // if (!_onlyPlaySometimes)
            {
                SFXAudioEventDriver.Instance.FireSFXEvent(sfx);
            }
            /*else
            {
                int draw = UnityEngine.Random.Range(0, _playChance);

                if (draw == 0)
                {
                    SFXAudioEventDriver.Instance.FireSFXEvent(sfx);
                }
            }*/
        }
    }
}
