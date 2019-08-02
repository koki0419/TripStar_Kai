using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyDataEntiry
{
    // エネミーのID
    public int enemy_id;
    // エネミーの名前
    public string enemy_Name;
    // エネミーの種類
    public string enemy_Type;
    // エネミーの位置座標
    public Vector3 enemy_Position;
    // エネミーの移動方向と距離
    public Vector3 enemy_MoveVector;
    // エネミーのHP
    public int enemy_Hp;
    // エネミーの移動速度
    public float enemy_MoveSpeed;
    // エネミーの攻撃ラグ時間
    public float enemy_AttackRugTime;
    // 破壊時出現星数
    public int enemy_AppearStarNum;
}


[CreateAssetMenu(menuName = "MyScriprable/Create EnemyData")]
public class EnemyData : ScriptableObject
{
    public List<EnemyDataEntiry> enemyDatas;
}
