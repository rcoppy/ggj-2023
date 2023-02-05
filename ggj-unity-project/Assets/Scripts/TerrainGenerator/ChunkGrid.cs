using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using Random = System.Random;

public class ChunkGrid : MonoBehaviour
{

    public GameObject Chunk;
    
    [HideInInspector]
    private float _sizeOfChunk;

    public float SizeOfChunk => _sizeOfChunk;
    
    private ObjectPool<GameObject> _chunksPool;


    public GameObject Player;

    public List<Material> GroundMaterials;
    
    private List<Vector3> _currentNeighborPositions = new List<Vector3>();


    // using this since i can't seem to easily loop through the _chunksPool
    private List<GameObject> _currentChunks = new List<GameObject>();


    // this is in "chunk space" so {0,0, 1,0, 2,0,} etc.
    private Vector3 _lastChunkCoords;

    private Vector3 _chunkCoordSpaceOrigin = Vector3.zero;

    private void Start()
    {
        _sizeOfChunk = Chunk.GetComponent<Renderer>().bounds.size.x;
        _chunksPool = new ObjectPool<GameObject>(
           createFunc: CreateChunk, 
            actionOnGet: (obj) => obj.SetActive(true), 
            actionOnRelease: (obj) => obj.SetActive(false), 
            actionOnDestroy: (obj) => Destroy(obj), 
            false, 
            defaultCapacity: 20, 
            20);

        _chunkCoordSpaceOrigin = Player.transform.position; 
        
        GenerateNeighborsFromPosition();
    }

    public UnityEvent SpawnNewEnemies;
    public UnityEvent DestroyEnemy;
    
    private GameObject CreateChunk()
    {
        GameObject c = Instantiate(Chunk, new Vector3(0f, 0f, 0f), Quaternion.identity);
        c.GetComponent<Renderer>().material = GroundMaterials[UnityEngine.Random.Range(0, GroundMaterials.Count)];
        return c;
    }

    public void Update() {
        var currentChunkCoords = GetPlayerCurrentChunkCoords();
        var positionInChunk = PositionWithinChunk(Player.transform.position);

        var offset = positionInChunk - new Vector3(0.25f, 0, 0.25f); 
        bool inBoundsX = offset.x is > 0f and < 0.5f;
        bool inBoundsY = offset.z is > 0f and < 0.5f;
        bool inBounds = inBoundsX && inBoundsY; 

        // note the current chunk, and compare if this is different to the old chunk. if it is the same, do nothing.
       if (currentChunkCoords == _lastChunkCoords && inBounds) return; 

        // spawn new enemies
        // todo time for enemies and like noise and shit
        SpawnNewEnemies?.Invoke();
        
        // get the new neighbors, which should be 9 new chunk positions.
        
        var newNeighborPositions = GetNewNeighborPositions(currentChunkCoords);
        
        var positionsToSave = new HashSet<Vector3>();
        
        foreach (GameObject chunk in _currentChunks)
        {
            if (newNeighborPositions.Contains(chunk.transform.position))
            {
                positionsToSave.Add(chunk.transform.position);
            } else {
                _chunksPool.Release(chunk);
                _currentChunks.Remove(chunk);
                
                // TODO
                DestroyEnemy?.Invoke();
            }
        }
        
        // _currentNeighborPositions.Clear();
        _currentNeighborPositions = newNeighborPositions; 

        SpawnAllNeighbors(positionsToSave);
    }


    public Vector3 GetPlayerCurrentChunkCoords()
    {
        Vector3 pos = Player.transform.position;
        return WorldCoordsToChunk(pos); 
    }
    
    public Vector3 WorldCoordsToChunk(Vector3 pos)
    {
        return new Vector3(Mathf.Floor((pos.x - _chunkCoordSpaceOrigin.x) / _sizeOfChunk), 
            0f, Mathf.Floor((pos.z - _chunkCoordSpaceOrigin.z) / (_sizeOfChunk)));
    }

    public Vector3 PositionWithinChunk(Vector3 pos)
    {
        return new Vector3((pos.x - _chunkCoordSpaceOrigin.x) / _sizeOfChunk % 1f, 
            0f, (pos.z - _chunkCoordSpaceOrigin.z) / (_sizeOfChunk) % 1f);
    }

    public Vector3 ChunkCoordsToWorld(Vector3 coords)
    {
        return new Vector3(_chunkCoordSpaceOrigin.x + _sizeOfChunk * coords.x, 0f, _chunkCoordSpaceOrigin.z + _sizeOfChunk * coords.z);
    }

    private List<Vector3> GetNewNeighborPositions(Vector3 currentChunkCoords)
    {
        List<Vector3> neighbors = new List<Vector3>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var newChunkCoords = currentChunkCoords + new Vector3(x, 0, y);
                var worldCoords = ChunkCoordsToWorld(newChunkCoords); 
                
                neighbors.Add(worldCoords);
            }
        }

        return neighbors;
    }

    /*public GameObject GetChunkByPos(Vector3 position)
    {
        var chunkCoords = WorldCoordsToChunk(position);
        
        foreach (GameObject chunk in _currentChunks)
        {
            var currentChunk = WorldCoordsToChunk(chunk.transform.position); 
            if (chunk.activeInHierarchy &&  currentChunk == chunkCoords)
            {
                return chunk;
            }
        }

        return null;
    }*/

    private void GenerateNeighborsFromPosition()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 chunkPosition = ChunkCoordsToWorld(new Vector3(x, 0, y));  // new Vector3(_chunkCoordSpaceOrigin.x + _sizeOfChunk * x, 0f, _chunkCoordSpaceOrigin.z + _sizeOfChunk * y);
                GameObject chunk = _chunksPool.Get();
                SpawnChunk(chunk, chunkPosition);
            }
        }
    }

    private void SpawnChunk(GameObject chunk, Vector3 worldPos)
    {
        chunk.transform.position = worldPos;
        _currentChunks.Add(chunk);
        SpawnNewEnemies?.Invoke();
    }

    private void SpawnAllNeighbors(HashSet<Vector3> existingPositions)
    {
        foreach (var pos in _currentNeighborPositions)
        {
            if (existingPositions.Contains(pos)) continue; 
            
            var newChunk = _chunksPool.Get();
            SpawnChunk(newChunk, pos);
        }
    }

}
