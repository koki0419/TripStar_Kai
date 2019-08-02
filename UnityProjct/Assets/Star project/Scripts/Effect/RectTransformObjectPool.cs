using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformObjectPool : MonoBehaviour
{
    private List<GameObject> poolObjList;
    private GameObject poolObj;
    /// <summary>
    /// オブジェクトプールを作成
    /// </summary>
    /// <param name="obj">生成する対象オブジェクト</param>
    /// <param name="maxCount">生成する数</param>
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
    /// 対象のオブジェクトが使用されていなければ渡す
    /// 使用されていないモノがなければ生成する
    /// </summary>
    /// <returns></returns>
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
        newObj.GetComponent<RectTransform>().parent = gameObject.GetComponent<RectTransform>();

        return newObj;
    }

    /// <summary>
    /// 対象オブジェクトを生成します
    /// </summary>
    /// <returns></returns>
    public GameObject CreatNewObject()
    {
        var newObj = Instantiate(poolObj);
        newObj.name = poolObj.name + (poolObjList.Count + 1);

        return newObj;
    }
}
