// _Project/Scripts/Managers/NotePoolManager.cs

using UnityEngine;
using System.Collections.Generic;

public class NotePoolManager : MonoBehaviour
{
    public static NotePoolManager Instance { get; private set; }

    // 一个用于在 Inspector 面板中定义对象池的类
    [System.Serializable]
    public class Pool
    {
        public string tag; // 用于识别池的标签
        public GameObject prefab;
        public int size; // 初始创建的对象数量
    }

    [Header("对象池配置")]
    public List<Pool> pools;

    // 用于存放我们实际对象池的字典
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
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false); // 初始为非激活状态
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectQueue);
        }

        Debug.Log("NotePoolManager 初始化完成，所有对象池已预热。");
    }

    /// <summary>
    /// 从指定的池中获取一个对象。
    /// </summary>
    /// <param name="tag">要从中获取的池的标签。</param>
    /// <returns>一个准备好使用的 GameObject。</returns>
    public GameObject GetFromPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"标签为 '{tag}' 的对象池不存在。");
            return null;
        }

        // 如果池空了，动态创建一个新对象来扩充池（可选，但为了安全是好习惯）
        if (poolDictionary[tag].Count == 0)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null)
            {
                GameObject newObj = Instantiate(pool.prefab);
                return newObj; // 直接返回它，它之后会被回收
            }
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);

        if (tag == "HoldNote")
        {
            HoldNoteController controller = objectToSpawn.GetComponent<HoldNoteController>();
            if (controller != null && !controller.IsCleanForPooling())
            {
                // 如果我们检测到一个“脏”的音符，打印一条警告并强制清理它！
                Debug.LogWarning($"[Pool Failsafe] 检测到一个未清理干净的HoldNote ({objectToSpawn.name})！正在强制清理。这表明回收逻辑可能存在问题。", objectToSpawn);
                controller.PrepareForPooling(); // 强制执行清理
            }
        }

        return objectToSpawn;
    }

    /// <summary>
    /// 将一个对象返回到它的池中。
    /// </summary>
    /// <param name="tag">要返回到的池的标签。</param>
    /// <param name="objectToReturn">要返回的 GameObject 实例。</param>
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"标签为 '{tag}' 的对象池不存在。");
            // 如果池不存在（例如对于动态创建的对象），就直接销毁它
            Destroy(objectToReturn);
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}