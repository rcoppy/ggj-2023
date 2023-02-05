using System;
using System.Collections;
using System.Collections.Generic;
using GGJ2022.EnemyAI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

public class EnemySpawner : MonoBehaviour
{
   [SerializeField] float _maxEntitiesPerChunk = 20f;
   
   private ObjectPool<GameObject> _enemyPool;
   // TODO: use queue as pool 

   private HashSet<GameObject> _currentEnemies = new HashSet<GameObject>();

   public List<GameObject> EnemyPrefabs;
   private void Awake()
   {
     _enemyPool = new ObjectPool<GameObject>(
         createFunc: CreateEnemy,
         actionOnGet: (obj) => obj.SetActive(true),
         actionOnRelease: (obj) =>
         {
            // Debug.Log(obj.name);
            obj.SetActive(false);
         },
         actionOnDestroy: (obj) => Destroy(obj),
         false,
         defaultCapacity:20,
         100);

     grid = GetComponent<ChunkGrid>();
   }


   private void SpawnEnemy(Vector3 chunkWorldPos)
   {
      var origin = chunkWorldPos - new Vector3(0.5f * grid.SizeOfChunk, 0f, 0.5f * grid.SizeOfChunk); 
      
      var x = UnityEngine.Random.Range(0, grid.SizeOfChunk);
      var y = UnityEngine.Random.Range(0, grid.SizeOfChunk);
      
      if (!GetDecisionToSpawnByNoise(x, y)) return; 
      
      GameObject e = _enemyPool.Get();
      Vector3 v = origin + new Vector3(x, 0f, y); 
      
      e.transform.position = v;
      _currentEnemies.Add(e);
   }

   /*private float CalculateOffset()
   {
      float f = UnityEngine.Random.Range(25, grid.SizeOfChunk);
      float coin = UnityEngine.Random.Range(0, 2);
      if (coin == 0)
      {
         f *= -1;
      }
      return f;
   }*/
   
   public void SpawnEnemyEvent(Vector3 chunkWorldPos)
   {
      for (int i = 0; i < _maxEntitiesPerChunk; i++)
      {
         SpawnEnemy(chunkWorldPos);
      } 
   }

   private ChunkGrid grid;
   
   public void RemoveEnemy(Vector3 worldPos)
   {
      List<GameObject> tempGOsToRemove = new List<GameObject>();
      foreach (var e in _currentEnemies)
      {
         var myChunk = grid.WorldCoordsToChunk(e.transform.position);
         var despawnedChunk = grid.WorldCoordsToChunk(worldPos);

         if (myChunk == despawnedChunk)
         {
            Debug.Log("chunk: " + myChunk);
            Debug.DrawRay(worldPos, Vector3.up);
            tempGOsToRemove.Add(e);
         }
      }

      foreach (var e in tempGOsToRemove)
      {
         // Debug.Log("i am despawning");
         
         _enemyPool.Release(e);
         _currentEnemies.Remove(e);
         
      }
   }

   public Texture2D NoiseyBoi;

   public GameObject Player;
   private bool GetDecisionToSpawnByNoise(float x, float y)
   {
      var w = NoiseyBoi.width;
      var h = NoiseyBoi.height; 
      
      Color c = NoiseyBoi.GetPixel((int) x % w, (int) y % h);

      return c.grayscale < 55f;
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

      e.GetComponent<EnemyState>().OnDeathTimedOut += () => _enemyPool.Release(e); 
      
      return e;
   }
}
