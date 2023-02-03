using UnityEngine;
using System.Collections;
using GGJ2022.Audio;

namespace GGJ2022
{
    public class FootstepsReceiver : MonoBehaviour
    {
        [SerializeField]
        string _actionName = "Walk";

        public void TriggerStep()
        {
            SFXAudioEventDriver.Instance.FireSFXEvent(_actionName);
        }
    }
}
