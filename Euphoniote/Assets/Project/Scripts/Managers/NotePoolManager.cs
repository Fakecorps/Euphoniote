// _Project/Scripts/Managers/NotePoolManager.cs (增强诊断最终版)

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor; // 需要这个来进行Prefab检查
#endif

public class NotePoolManager : MonoBehaviour
{
    public static NotePoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [Header("对象池配置")]
    public List<Pool> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogError($"对象池 '{pool.tag}' 的 Prefab 未设置！", this.gameObject);
                continue;
            }

            Queue<GameObject> objectQueue = new Queue<GameObject>();

            // 将对象池的根对象设置为 NotePoolManager 自身，便于在Hierarchy中管理
            Transform poolParent = new GameObject(pool.tag + " Pool").transform;
            poolParent.SetParent(this.transform);

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, poolParent); // 创建实例并指定父对象
                obj.name = $"{pool.tag}_{i}"; // 给实例一个清晰的名字
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectQueue);
        }

        Debug.Log("NotePoolManager 初始化完成，所有对象池已预热。");
    }

    public GameObject GetFromPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"标签为 '{tag}' 的对象池不存在。");
            return null;
        }

        if (poolDictionary[tag].Count == 0)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null)
            {
                // 动态扩容时也指定父对象
                Transform poolParent = transform.Find(pool.tag + " Pool");
                GameObject newObj = Instantiate(pool.prefab, poolParent);
                newObj.name = $"{pool.tag}_Expanded";
                // 动态扩容的对象在取出时应该是激活的，所以不需要 SetActive(true)
                return newObj;
            }
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // --- 核心诊断代码 ---
#if UNITY_EDITOR
        // 检查取出的对象是否是一个Prefab资产
        if (PrefabUtility.IsPartOfPrefabAsset(objectToSpawn))
        {
            Debug.LogError($"严重错误！对象池 '{tag}' 返回了一个Prefab资产，而不是实例！正在尝试创建一个新实例来修复。", objectToSpawn);

            // 尝试修复：销毁错误的引用并创建一个正确的实例
            // (注意：不能销毁资产，这里只是示意，正确的做法是找到问题的根源)
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null)
            {
                Transform poolParent = transform.Find(pool.tag + " Pool");
                objectToSpawn = Instantiate(pool.prefab, poolParent);
                objectToSpawn.name = $"{pool.tag}_Hotfix";
            }
        }
#endif

        objectToSpawn.SetActive(true);

        // ... 原有的 HoldNote Failsafe 逻辑保持不变 ...

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"标签为 '{tag}' 的对象池不存在。");
            Destroy(objectToReturn);
            return;
        }

        // --- 核心诊断代码 ---
#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabAsset(objectToReturn))
        {
            Debug.LogError($"严重错误！正在尝试将一个Prefab资产 '{objectToReturn.name}' 返回到对象池 '{tag}'！这是不被允许的。", objectToReturn);
            return; // 阻止错误的操作
        }
#endif

        objectToReturn.SetActive(false);

        // 返回时，重新设置父对象，以防它在运行时被移动到别处
        Transform poolParent = transform.Find(tag + " Pool");
        if (poolParent != null)
        {
            objectToReturn.transform.SetParent(poolParent);
        }

        poolDictionary[tag].Enqueue(objectToReturn);
    }
}