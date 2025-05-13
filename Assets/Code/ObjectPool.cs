using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;
    
    // Dictionary of pools
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();
    
    // Get the singleton instance
    public static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ObjectPool");
                instance = go.AddComponent<ObjectPool>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    // Initialize a new pool
    public void InitializePool(string poolName, GameObject prefab, int initialSize)
    {
        if (pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool {poolName} already exists!");
            return;
        }
        
        Queue<GameObject> pool = new Queue<GameObject>();
        prefabLookup[poolName] = prefab;
        
        // Create initial objects
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewInstance(poolName, prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        
        pools[poolName] = pool;
        Debug.Log($"Created pool: {poolName} with {initialSize} objects");
    }
    
    // Get an object from the pool
    public GameObject GetFromPool(string poolName, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        // Create pool if it doesn't exist
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool {poolName} doesn't exist. Make sure to initialize it first.");
            return null;
        }
        
        Queue<GameObject> pool = pools[poolName];
        GameObject prefab = prefabLookup[poolName];
        
        // If pool is empty, create a new instance
        if (pool.Count == 0)
        {
            GameObject newObj = CreateNewInstance(poolName, prefab);
            SetupPooledObject(newObj, position, rotation, parent);
            return newObj;
        }
        
        // Get existing object from pool
        GameObject obj = pool.Dequeue();
        
        // Reactivate the object
        SetupPooledObject(obj, position, rotation, parent);
        
        return obj;
    }
    
    // Return an object to the pool
    public void ReturnToPool(string poolName, GameObject obj)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool {poolName} doesn't exist. Creating it now.");
            GameObject prefab = obj;
            InitializePool(poolName, prefab, 5);
        }
        
        // Reset the object
        obj.SetActive(false);
        
        // If object has a parent, detach it
        obj.transform.SetParent(transform);
        
        // Add back to pool
        pools[poolName].Enqueue(obj);
    }
    
    // Helper to create a new instance
    private GameObject CreateNewInstance(string poolName, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.name = $"{poolName}_Object";
        
        // Add a component to track which pool this belongs to
        PooledObject pooledObj = obj.AddComponent<PooledObject>();
        pooledObj.PoolName = poolName;
        
        obj.transform.SetParent(transform);
        return obj;
    }
    
    // Helper to set up a pooled object for reuse
    private void SetupPooledObject(GameObject obj, Vector3 position, Quaternion rotation, Transform parent)
    {
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        
        // Reset RectTransform if it's a UI element
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        if (rectTransform != null && parent != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        obj.SetActive(true);
    }
}

// Helper component to track which pool an object belongs to
public class PooledObject : MonoBehaviour
{
    public string PoolName { get; set; }
    
    // Auto-return to pool after a delay
    public void ReturnToPoolAfterDelay(float delay)
    {
        StartCoroutine(ReturnAfterDelay(delay));
    }
    
    private System.Collections.IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }
    
    // Return to pool manually
    public void ReturnToPool()
    {
        if (gameObject.activeInHierarchy)
        {
            ObjectPool.Instance.ReturnToPool(PoolName, gameObject);
        }
    }
}