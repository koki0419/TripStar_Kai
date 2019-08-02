using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack
{

    // Attack消費ポイント
    public enum PlayerAttackIndex
    {
        None,
        AttackNormal = 1010,
        AttackDown = 1001,
        AttackUp = 1011,
    }
    public PlayerAttackIndex payerAttackIndex = PlayerAttackIndex.None;


    /// <summary>
    /// 上下左右のどのコマンドが入ったかを返します
    /// </summary>
    /// <param name="direction">入力情報</param>
    /// <param name="obj">対象キャラ</param>
    /// <returns></returns>
    public int OnAttack(Vector2 direction, GameObject obj)
    {
        int animationName;
        // 座標設定用変数
        float x;
        // キャラクタ管理のデータ取得
        // 回転量
        Vector3 objRot = obj.transform.eulerAngles;
        // 座標
        Vector3 objPos = obj.transform.position;
        // キャラクタの向いている方向ベクトル計算
        x = Mathf.Sin(objRot.y * Mathf.Deg2Rad);
        // 単位ベクトル計算
        Vector2 firing = direction.normalized;
        // ラジアン
        float radian = Mathf.Atan2(firing.y, firing.x);
        // 角度
        float degree = radian * Mathf.Rad2Deg;
        if (firing.x == 0 && firing.y == 0)
        {
            animationName = (int)PlayerAttackIndex.AttackNormal;
        }
        else
        {
            // 右
            if (degree < 30 && degree > -30)
            {
                animationName = (int)PlayerAttackIndex.AttackNormal;
            }
            // 左
            else if (degree > 150 && degree <= 180 || degree < -150 && degree >= -180)
            {
                animationName = (int)PlayerAttackIndex.AttackNormal;
            }
            // 上
            else if (degree > 30 && degree < 150)
            {
                animationName = (int)PlayerAttackIndex.AttackUp;
            }
            // 下
            else if (degree < -30 && degree > -150)
            {
                animationName = (int)PlayerAttackIndex.AttackDown;
            }
            else
            {
                animationName = -1;
            }
        }
        return animationName;
    }
}
