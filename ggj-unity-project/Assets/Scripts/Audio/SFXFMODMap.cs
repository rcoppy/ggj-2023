using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using System;

namespace GGJ2022.Audio
{
    [CreateAssetMenu(fileName = "SFXMap", menuName = "GGJ2022/SFX event map", order = 1)]
    public class SFXFMODMap : ScriptableObject
    {
        [Serializable]
        public struct KeyValue
        {
            [SerializeField]
            public string Action;

            [SerializeField]
            public FMODUnity.EventReference FMODEvent;
        }

        [SerializeField]
        List<KeyValue> _keyValues;

        Dictionary<string, FMOD.GUID> _sfxMap;

        public Dictionary<string, FMOD.GUID> Map {
            get { return _sfxMap; }
        }

        public void RefreshMap()
        {
            _sfxMap = new Dictionary<string, FMOD.GUID>();

            foreach (var kvp in _keyValues) {
                _sfxMap[kvp.Action] = kvp.FMODEvent.Guid;
            }

        }
    }
}
