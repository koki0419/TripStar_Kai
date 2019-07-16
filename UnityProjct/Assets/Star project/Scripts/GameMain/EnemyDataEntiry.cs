using UnityEngine;

public class EnemyDataEntiry
{
    //エネミーのID
    public int enemy_id;
    //エネミーの名前
    public string enemy_Name;
    //エネミーの種類
    public string enemy_Type;
    //エネミーの位置座標
    public Vector3 enemy_Position;
    //エネミーの移動方向と距離
    public Vector3 enemy_MoveVector;
    //エネミーのHP
    public int enemy_Hp;
    //エネミーの移動速度
    public float enemy_MoveSpeed;
    //エネミーの攻撃ラグ時間
    public float enemy_AttackRugTime;
    //破壊時出現星数
    public int enemy_AppearStarNum;

    public void SetEnemyDatas(int id, string name, string type, float position_x, float position_y, float position_z,
                        float moveVector, int hp, float moveSpeed, float attackTime, int starNum)
    {
        enemy_id = id;
        enemy_Name = name;
        enemy_Type = type;
        enemy_Position.x = position_x;
        enemy_Position.y = position_y;
        enemy_Position.z = position_z;
        enemy_MoveVector.x = moveVector;
        enemy_MoveVector.y = 0;
        enemy_MoveVector.z = 0;
        enemy_Hp = hp;
        enemy_MoveSpeed = moveSpeed;
        enemy_AttackRugTime = attackTime;
        enemy_AppearStarNum = starNum;
    }
}
