using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarEffect : MonoBehaviour
{

    private RectTransform target = null;
    private float moveSpeed = 15;
    private float errorPosition = 5.0f;

    public void Init(RectTransform target)
    {
        this.target = target;
    }

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

    private void Update()
    {
        //GetComponent<RectTransform>().LookAt(target);
        if (this.target != null)
        {
            gameObject.transform.localPosition += Direction();
            if (target.position.x + errorPosition >= transform.position.x && target.position.y - errorPosition <= transform.position.y)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
