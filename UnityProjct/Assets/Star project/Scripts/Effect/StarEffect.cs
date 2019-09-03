using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarEffect : MonoBehaviour
{
    private RectTransform target = null;
    private float moveSpeed = 15;
    private float errorPosition = 5.0f;
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="target"></param>
    public void Init(RectTransform target)
    {
        this.target = target;
        var starScale = transform.localScale;
        starScale.x =1.0f;
        starScale.y =1.0f;
        transform.localScale = starScale;
        isMove = true;
        isScale = false;
    }
    /// <summary>
    /// ターゲットへのベクトルを生成します
    /// </summary>
    /// <returns></returns>
    private Vector3 Direction()
    {
        //自分の座標をプレイヤーの座標からベクトル作成
        Vector3 targetVec = this.target.position - GetComponent<RectTransform>().position;
        //単位ベクトル作成（上記のベクトル）
        Vector3 targetVecE = targetVec.normalized;
        //長さを調節
        targetVecE.z = 0;
        var moveForce = targetVecE * moveSpeed;

        return moveForce;
    }
    private bool isMove = true;
    private bool isScale = false;
    private void Update()
    {
        if (this.target != null && isMove)
        {
            gameObject.transform.localPosition += Direction();
            if (target.position.x + errorPosition >= transform.position.x && target.position.y - errorPosition <= transform.position.y)
            {
                isMove = false;
                isScale = true;
            }
        }
        if (isScale)
        {
            StarScale();
        }
    }
    private void StarScale()
    {
        var starScale = transform.localScale;
        starScale.x -= 0.1f;
        starScale.y -= 0.1f;
        transform.localScale = starScale;
        if(starScale.x <= 0.0f)
        {
            gameObject.SetActive(false);
        }
    }
}
