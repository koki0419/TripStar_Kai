using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformObjectPool : MonoBehaviour
{
    private List<GameObject> poolObjList;
    private GameObject poolObj;

    //オブジェクトプールを作成
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

    public GameObject GetObject()
    {
        //使用中でないモノを探して返す
        foreach(var obj in poolObjList)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);

                return obj;
            }
        }
        //全て使用中だったら新たに生成して返す
        var newObj = CreatNewObject();
        newObj.SetActive(true);
        poolObjList.Add(newObj);
        newObj.GetComponent<RectTransform>().parent = gameObject.GetComponent<RectTransform>();

        return newObj;
    }


    public GameObject CreatNewObject()
    {
        var newObj = Instantiate(poolObj);
        newObj.name = poolObj.name + (poolObjList.Count + 1);

        return newObj;
    }
}
