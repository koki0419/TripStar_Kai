using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class StarDataEntiry
{
    // 星のID
    public int star_id;
    // 星の位置座標
    public Vector3 star_Position;
    // 星の獲得ポイント
    public int star_Point;
}

[CreateAssetMenu(menuName = "MyScriprable/Create StarData")]
public class StarData : ScriptableObject
{
    // StarDataEntiryクラスを格納するリスト
    public List<StarDataEntiry> starDatas;
}