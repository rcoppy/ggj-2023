using UnityEngine;
using System.Collections;

namespace GGJ2022.FX
{
    [RequireComponent(typeof(TimedSquishing))]
    public class SquishIndefinitely : MonoBehaviour
    {
        TimedSquishing _squish;

        [SerializeField]
        TimedSquishing.SquishParams _squishSettings;

        // Use this for initialization
        void Start()
        {
            _squish = GetComponent<TimedSquishing>();
            TriggerSquish();
        }

        public void TriggerSquish()
        {
            _squish.ApplyParams(_squishSettings);
            _squish.TriggerLoopingSquish();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_squish != null)
            {
                _squish.ApplyParams(_squishSettings);
            }
        }
#endif
    }
}
