using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

public class EnemySpawner : MonoBehaviour
{
   private ObjectPool<GameObject> _enemyPool;

   private List<GameObject> _currentEnemies = new List<GameObject>();

   public List<GameObject> EnemyPrefabs;
   private void Awake()
   {
     _enemyPool = new ObjectPool<GameObject>(
         createFunc: CreateEnemy,
         actionOnGet: (obj) => obj.SetActive(true),
         actionOnRelease: (obj) => obj.SetActive(false),
         actionOnDestroy: (obj) => Destroy(obj),
         false,
         defaultCapacity:20,
         100);

     grid = GetComponent<ChunkGrid>();
   }


   private void SpawnEnemy()
   {
      if (!GetDecisionToSpawnByNoise())
      {
         return;
      }
      GameObject e = _enemyPool.Get();
      Vector3 v = new Vector3(
         Player.transform.position.x + CalculateOffset(),
         0f,
         Player.transform.position.z + CalculateOffset());
      e.transform.position = v;
      _currentEnemies.Add(e);
   }

   private float CalculateOffset()
   {
      float f = UnityEngine.Random.Range(25, grid.SizeOfChunk);
      float coin = UnityEngine.Random.Range(0, 2);
      if (coin == 0)
      {
         f *= -1;
      }
      return f;
   }
   
   public void SpawnEnemyEvent(int number)
   {
      for (int i = 0; i < number; i++)
      {
         SpawnEnemy();
      } 
   }

   private ChunkGrid grid;
   
   public void RemoveEnemy()
   {
      List<GameObject> tempGOsToRemove = new List<GameObject>();
      foreach (var e in _currentEnemies)
      {
         // this logic feels wrong
         if (Vector3.Distance(Player.transform.position, e.transform.position) > grid.SizeOfChunk)
         {
             _enemyPool.Release(e);
             tempGOsToRemove.Add(e);
         }
      }

      foreach (var e in tempGOsToRemove)
      {
         _currentEnemies.RemoveAt(_currentEnemies.IndexOf(e));
      }
   }

   public Texture2D NoiseyBoi;

   public GameObject Player;
   private bool GetDecisionToSpawnByNoise()
   {
      
      Color c = NoiseyBoi.GetPixel((int) Player.transform.position.x, (int) Player.transform.position.y);
      if (c.grayscale < 55f)
      {
         return true;
      }
      else
      {
        // die
        return false;
      }
      
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
