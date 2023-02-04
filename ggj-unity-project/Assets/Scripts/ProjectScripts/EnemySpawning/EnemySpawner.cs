using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
   private ObjectPool<GameObject> _enemyPool;


   public List<GameObject> EnemyPrefabs;
   private void Start()
   {
     _enemyPool = new ObjectPool<GameObject>(
         createFunc: CreateEnemy,
         actionOnGet: (obj) => obj.SetActive(true),
         actionOnRelease: (obj) => obj.SetActive(false),
         actionOnDestroy: (obj) => Destroy(obj),
         false,
         defaultCapacity:20,
         20
         );

     grid = GetComponent<ChunkGrid>();
     
     // spawn a bunch
   }

   public List<GameObject> _currentEnemies = new List<GameObject>();
   public void SpawnEnemy()
   {
      Vector3 v = GetLocationByNoise();
      if (v == Vector3.zero)
      {
         return;
      }
      GameObject e = _enemyPool.Get();
      e.transform.position = GetLocationByNoise();
      _currentEnemies.Add(e);
   }

   private ChunkGrid grid;
   
   public void RemoveEnemy()
   {
      foreach (var e in _currentEnemies)
      {
         if (grid.GetChunkByPos(e.transform.position) == null)
         {
             _enemyPool.Release(e);
            _currentEnemies.RemoveAt(_currentEnemies.IndexOf(e));
         }
      }
   }

   public Texture2D NoiseyBoi;

   public GameObject Player;
   private Vector3 GetLocationByNoise()
   {
      Color c = NoiseyBoi.GetPixel((int) Player.transform.position.x, (int) Player.transform.position.y);
      if (c.grayscale > 100f)
      {
         return Player.transform.position;
      }
      else
      {
        // die
      }
      
      return Vector3.zero;
   } 
   private void Update()
   {
       
   }

   private GameObject CreateEnemy()
   {
      GameObject e = Instantiate(
          EnemyPrefabs[UnityEngine.Random.Range(0, EnemyPrefabs.Count)], 
          Vector3.zero,
          Quaternion.identity);
      return e;
   }
}
