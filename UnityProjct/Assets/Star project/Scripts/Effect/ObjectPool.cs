using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オブジェクトプール
/// </summary>
public class ObjectPool : MonoBehaviour
{
    private List<GameObject> poolObjList;
    private GameObject poolObj;

    // オブジェクトプールを作成
    public void CreatePool(GameObject obj,int maxCount)
    {
        poolObj = obj;
        poolObjList = new List<GameObject>();
        for(int i = 0; i < maxCount; i++)
        {
            var newObj = CreatNewObject();
            newObj.transform.SetParent(gameObject.transform);
            newObj.SetActive(false);
            poolObjList.Add(newObj);
        }
    }
    /// <summary>
    /// 外部から参照されたときにオブジェクトを返します
    /// </summary>
    /// <returns>生成されたオブジェクトを返します</returns>
    public GameObject GetObject()
    {
        // 使用中でないモノを探して返す
        foreach(var obj in poolObjList)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        // 全て使用中だったら新たに生成して返す
        var newObj = CreatNewObject();
        newObj.SetActive(true);
        poolObjList.Add(newObj);
        newObj.transform.SetParent(transform);
        return newObj;
    }
    /// <summary>
    /// オブジェクトを生成します
    /// </summary>
    /// <returns></returns>
    public GameObject CreatNewObject()
    {
        var newObj = Instantiate(poolObj);
        newObj.name = poolObj.name + (poolObjList.Count + 1);
        return newObj;
    }
}
