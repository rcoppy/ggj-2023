using UnityEngine;
using System;
using System.Collections.Generic;


namespace GGJ2022.EnemyAI
{
    [CreateAssetMenu(fileName = "EnemyProps", menuName = "GGJ2022/New enemy profile", order = 0)]
    public class EnemyProps : ScriptableObject
    {
        [SerializeField] private string _damagedSound = "EnemyDamaged";
        [SerializeField] private string _doMeleeAttackSound = "EnemyDidMeleeAttack";
        [SerializeField] private string _doRangedAttackSound = "EnemyDidRangedAttack";
        [SerializeField] private string _fleeingSound = "EnemyFleeing";

        public string DamagedSound => _damagedSound;
        public string DoMeleeAttackSound => _doMeleeAttackSound;
        public string DoRangedAttackSound => _doRangedAttackSound;
        public string FleeingSound => _fleeingSound;

    }
}