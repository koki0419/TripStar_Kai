using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//プレイヤーとエネミーなど動くオブジェクト用親オブジェクト
public class Pawn : MonoBehaviour
{
    [SerializeField] protected private Rigidbody pownRigidbody;

    /// <summary>
    /// オブジェクトの移動
    /// </summary>
    /// <param name="velocity">移動する向き(方向)</param>
    protected void Move(Vector3 velocity)
    {
        pownRigidbody.velocity = velocity;
    }


}
