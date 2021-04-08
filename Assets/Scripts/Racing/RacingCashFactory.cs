using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingCashFactory : MonoBehaviour
{    
    // Factory operates on server side
    public float spawnTime = .5f;
    public float xSpawnRange = 13f;
    public float zSpawnRange = 13f;

    public GameObject m_CashPrefab;

    // Start is called before the first frame update
    public void Start()
    {
        InvokeRepeating(nameof(Spawn), spawnTime, spawnTime);
    }

    public void Spawn()
    {
        // Randomize spawn location
        Vector3 position = new Vector3(Random.Range(-xSpawnRange, xSpawnRange), 0, Random.Range(-zSpawnRange, zSpawnRange));
        GameObject spawnedObj = Instantiate(m_CashPrefab, position, Quaternion.identity) as GameObject;
    }
}
