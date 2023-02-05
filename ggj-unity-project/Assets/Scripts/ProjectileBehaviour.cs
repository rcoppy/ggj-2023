using System;
using GGJ2022.EnemyAI;
using UnityEngine;

namespace GGJ2022
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        public GameObject parent; 
        public int Damage = 1;
        public Vector3 velocity;
        public Type targetType = typeof(EnemyState);
        
        
        private float _maxLifeTime = 5f;
        private float _deathTime;

        void Update()
        {
            transform.position += Time.deltaTime * velocity;
            if (Time.time > _deathTime)
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            _deathTime = Time.time + _maxLifeTime;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var state = collision.gameObject.GetComponent(targetType);
            if (state != null)
            {
                // ideally player and enemy should share a common interface
                // but out of time
                if (targetType == typeof(PlayerState))
                    ((PlayerState)state).DoDamage(Damage);
                else if (targetType == typeof(EnemyState))
                    ((EnemyState)state).DoDamage(Damage);
            }

            if (collision.gameObject != parent) Destroy(gameObject);
        }
    }
}