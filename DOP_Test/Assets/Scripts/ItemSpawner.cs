using UnityEngine;
using UnityEngine.Serialization;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _itemPrefab; 
    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private Vector2 _spawnAreaSize = new Vector2(5f, 5f); 

    private float _nextSpawnTime;

    void Start()
    {
        _nextSpawnTime = Time.time + _spawnInterval;
    }

    void Update()
    {
        if (Time.time >= _nextSpawnTime)
        {
            SpawnItem();
            _nextSpawnTime = Time.time + _spawnInterval; 
        }
    }

    private void SpawnItem()
    {
        Vector2 spawnPosition = new Vector2(
            transform.position.x + Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
            transform.position.y + Random.Range(-_spawnAreaSize.y / 2, _spawnAreaSize.y / 2)
        );

        Instantiate(_itemPrefab, spawnPosition, Quaternion.identity);
    }
}