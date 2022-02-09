using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public static class PoolManager{
    public static Dictionary<GameObject, Pool> poolByPrefab = new Dictionary<GameObject, Pool>();       //used to find which pool belongs to which prefab
    public static Dictionary<GameObject, Pool> poolByGameObject = new Dictionary<GameObject, Pool>();   //used to find which pool a gameobject belongs to
    
    //Allocate (enable and init) a gameobject, if the pool doesnt exist, it will be created, if the pool is full, it will be extended
    public static GameObject Allocate(GameObject prefab, Vector3 position, Quaternion rotation){
        if(!poolByPrefab.ContainsKey(prefab)){
            poolByPrefab.Add(prefab, new Pool(prefab, 100));
        }

        return poolByPrefab[prefab].Allocate(position, rotation);
    }

    //Deallocate (disable and cleanup) a gameobject, if the object doesnt belong to a pool, destroy is used instead
    public static void Deallocate(GameObject obj){
        if(!poolByGameObject.ContainsKey(obj)){
            GameObject.Destroy(obj);
            Debug.LogWarning("<color=#57f542>[PoolManager::Deallocate]</color> Hard Destroy " + obj);
            return;
        }
        
        poolByGameObject[obj].Deallocate(obj);
    }

    //Deallocate every object from every pool, useful on scene changes
    public static void ClearAllPools(){
        foreach(KeyValuePair<GameObject, Pool> tuple in poolByGameObject){
            Deallocate(tuple.Key);
        }
    }

}

public class Pool{

    public GameObject pooledPrefab;
    public int initSize;
    public List<GameObject> enabled = new List<GameObject>();
    public ConcurrentQueue<GameObject> disabled = new ConcurrentQueue<GameObject>();    //can be used by other threads
    public GameObject parent;
    
    //Constructor uses initSize to set the initial size of the pool, and also a parent obj
    //to keep the hierarchy nice and neat
    public Pool(GameObject pooledPrefab, int initSize){
       
        this.pooledPrefab = pooledPrefab;
        this.initSize = initSize;
		parent = new GameObject(this.pooledPrefab.name);

        for(int i = 0; i <= this.initSize; i++){
            var go = GameObject.Instantiate(this.pooledPrefab);
            disabled.Enqueue(go);
            go.transform.parent = parent.transform;
            go.SetActive(false);
			PoolManager.poolByGameObject.Add(go, this);
        }

        parent.name = this.pooledPrefab.name + " (Managed Pool) (" + (enabled.Count + disabled.Count) + ")";

    }

    //Disables a obj in the pool, and sends a callback
    public void Deallocate(GameObject obj){
        obj.SendMessage("OnDeallocate", SendMessageOptions.DontRequireReceiver);
        obj.SetActive(false);
        enabled.Remove(obj);
        disabled.Enqueue(obj);
    }
    
    // Tries to enable and init another obj in the pool, if the pool is full, then a obj is created
    // also sends a callback thru the interface
    public GameObject Allocate(Vector3 position, Quaternion rotation){

        GameObject obj = null;

        if(!disabled.TryDequeue(out obj)){
            obj = ExtendPool();
        }
        
        obj.SetActive(true);
        enabled.Add(obj);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SendMessage("OnAllocate", SendMessageOptions.DontRequireReceiver);
        return obj;
    }

    //Extends the pool by one
    private GameObject ExtendPool(){

        GameObject obj = GameObject.Instantiate(pooledPrefab);
        enabled.Add(obj);
        PoolManager.poolByGameObject.Add(obj, this);
        obj.transform.parent = parent.transform;
        parent.name = pooledPrefab.name + " (Managed Pool) (" + (enabled.Count + disabled.Count) + ")";
        
        Debug.Log("<color=#57f542>[PoolManager::Allocate]</color> Extending Pool: " + pooledPrefab);
        
        return obj;
    }

}

// Silly interface that has the OnAllocate and OnDeallocate callbacks, using this is optional
// (and even not recommended sometimes, use OnEnable and OnDisable for performance)
public interface IPoolable{
    public abstract void OnAllocate();
    public abstract void OnDeallocate();
}
