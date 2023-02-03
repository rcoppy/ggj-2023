using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ChunkGrid : MonoBehaviour
{

    public GameObject Chunk;
    private float sizeOfChunk;
    private ObjectPool<GameObject> _chunksPool;

    public GameObject Player;


    private List<Vector3> newNeighborPositions = new List<Vector3>();
    private List<Vector3> currentNeighborPositions = new List<Vector3>();


    // using this since i can't seem to easily loop through the _chunksPool
    private List<GameObject> currentChunks = new List<GameObject>();

    
    // this is in "chunk space" so {0,0, 1,0, 2,0,} etc.
    private Vector3 currentChunkCoords = new Vector3();
    
    private void Awake()
    {
        sizeOfChunk = Chunk.GetComponent<Renderer>().bounds.size.x;
    }

    private void Start()
    {
        _chunksPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(Chunk, new Vector3(0f,0f,0f), Quaternion.identity), 
            actionOnGet: (obj) => obj.SetActive(true), 
            actionOnRelease: (obj) => obj.SetActive(false), 
            actionOnDestroy: (obj) => Destroy(obj), 
            false, 
            defaultCapacity: 20, 
            20);
 
        GenerateNeighborsFromPosition(Player.transform.position);
    }

    public void Update()
    {
        //Vector3 currentChunkPos = GetCurrentChunkPos();
        //Debug.Log($"{currentChunkPos.x}, {currentChunkPos.z}");
        
        // get the current player's chunk
        Vector3 newChunkCoords = GetPlayerCurrentChunkCoords();
        Debug.Log($"{newChunkCoords.x}, {newChunkCoords.z}");
        
       // note the current chunk, and compare if this is different to the old chunk. if it is the same, do nothing.
        if (newChunkCoords == currentChunkCoords)
        {
            return;
        }

        currentChunkCoords = newChunkCoords;

        // get the new neighbors, which should be 9 new chunk positions.
        newNeighborPositions = GetNewNeighborPositions(currentChunkCoords);
        
        
        currentNeighborPositions.Clear();
        
        // now we should loop through all the current chunks (the old position's neighbors)
            // if there are neighbors of the current chunk and the old positions, keep them
            // if they are not neighbors of the current chunk, remove them
            
        List<GameObject> temporaryChunksToRemove = new List<GameObject>(); 
        foreach (GameObject chunk in currentChunks)
        {
            // TODO this chunk is no longer a current neighbor. this math doesnt seem right.
            if (!newNeighborPositions.Contains(chunk.transform.position))
            {
                Debug.Log($"removing {chunk.transform.position}");
                _chunksPool.Release(chunk);
                temporaryChunksToRemove.Add(chunk);
            }
            else
            {
                currentNeighborPositions.Add(chunk.transform.position);
            }
        }
        // 
        
        foreach (var o in temporaryChunksToRemove)
        {
            currentChunks.RemoveAt(currentChunks.IndexOf(o));
        }
        //currentChunks.RemoveAt(currentChunks.IndexOf(chunk));

        // now currentNeighborPositions only has the OLD chunks, that are STILL neighbors. now we have to add the new neighbors. im crying

        // now we need to add neighbors from the pool for the chunks that haven't been loaded in yet, and activate them    
        foreach (Vector3 neighborPos in newNeighborPositions)
        {
            if (!currentNeighborPositions.Contains(neighborPos))
            {
                
                GameObject chunk = _chunksPool.Get();
                chunk.transform.position = neighborPos;
                currentChunks.Add(chunk);
            }
        }
    }

    
    private Vector3 GetPlayerCurrentChunkCoords()
    {
        Vector3 pos = Player.transform.position;
        return new Vector3(Mathf.Floor(pos.x / sizeOfChunk), 0f, Mathf.Floor(pos.z / (sizeOfChunk)));
    }

    private List<Vector3> GetNewNeighborPositions(Vector3 chunkCoordinates)
    {
        List<Vector3> neighbors = new List<Vector3>();
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector3 chunkPosition = new Vector3
                    (chunkCoordinates.x * sizeOfChunk  + (sizeOfChunk * x), 0f, 
                    chunkCoordinates.z * sizeOfChunk + (sizeOfChunk * y));
                neighbors.Add(chunkPosition);
            } 
        }
        return neighbors;
    }

    private GameObject GetChunkByPos(Vector3 position)
    {
        foreach (GameObject chunk in currentChunks)
        {
            if (chunk.activeInHierarchy && chunk.transform.position == position)
            {
                return chunk;
            }
        }
        return null;
    }

    public void GenerateNeighborsFromPosition(Vector3 position)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 chunkPosition = new Vector3(position.x + (sizeOfChunk * x), 0f, position.z + (sizeOfChunk * y));
                GameObject chunk = GetChunkByPos(chunkPosition);
                if (chunk == null)
                {
                    chunk = _chunksPool.Get();
                    chunk.transform.position = chunkPosition;
                    currentChunks.Add(chunk);
                }
            } 
        }
    }

}

// place the 9 objects, starting at 0, 0

// update from chat gpt
//below suggestion from chatGPT
/*if (!currentNeighbors.Contains(currentChunkPos))
{
 GenerateNeighborsFromPosition(currentChunkPos);
 newNeighbors = GetNeighborsFromPosition(currentChunkPos);

 // remove chunks that aren't neighbors of the player's neighbors
 foreach (Vector3 chunkPos in currentNeighbors)
 {
     if (!newNeighbors.Contains(chunkPos))
     {
         GameObject chunk = GetChunkByPos(chunkPos);
         if (chunk != null)
         {
             currentChunks.RemoveAt(currentChunks.IndexOf(chunk));
             _chunksPool.Release(chunk);
         }
     }
 }

 currentNeighbors = newNeighbors;
}*/