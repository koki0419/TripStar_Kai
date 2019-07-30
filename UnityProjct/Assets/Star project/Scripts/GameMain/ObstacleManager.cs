﻿using System.Collections;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    //-------------Unityコンポーネント関係-------------------
    //エフェクト
    [SerializeField] private GameObject breakEffect = null;
    [SerializeField] private Renderer moaiRenderer = null;
    private Camera tragetCamera = null;
    //-------------クラス関係--------------------------------
    private PlayerMove playerMove;
    private ObstacleSpawn obstacleSpawn;
    [SerializeField] private EnemyController enemyController;
    //-------------数値用変数--------------------------------
    //生成する星の数
    private int spawnStarNum = 0;
    //ポイントを獲得した回数
    private int acquisitionPoint = 0;
    //破壊時消えるまでの時間
    private float deleteTime = 2.0f;
    //基礎Hp
    private float foundationHP;
    //破壊音インデックス
    private const int breakSeNum = 7;
    //オブジェクトをReMoveするポジション
    private const float reMoveX = 10.0f;
    [SerializeField] private GameObject obstaclesHeadObj = null;
    private const string normalLayer = "Obstacles";
    private const string breakLayer = "BreakObstacls";
    //-------------フラグ用変数------------------------------
    private bool onRemoveObjFlag = false;
    private bool isDamage;
    private bool canDamage = true;
    public bool IsDestroyed
    {
        get; private set;
    }
    public void SetObstacleDatas(Camera targetCamera,PlayerMove playerMove,ObstacleSpawn obstacleSpawn, int hp,int spawnStarNum)
    {
        this.tragetCamera = targetCamera;
        this.playerMove = playerMove;
        this.obstacleSpawn = obstacleSpawn;
        this.foundationHP = hp;
        this.spawnStarNum = spawnStarNum;
    }
    public void Init()
    {
        //オブジェクトを削除するかどうか
        onRemoveObjFlag = false;
        //ポイントを獲得した回数
        acquisitionPoint = 0;
        deleteTime = 2.0f;
        IsDestroyed = false;
        breakEffect.SetActive(false);
        obstaclesHeadObj.SetActive(true);
        //壊れたときにキャラクターと当たり判定を持たなくします
        //レイヤーの変更
        //レイヤーはやりすぎか？コライダー消去の方がよけれは修正要
        gameObject.layer = LayerMask.NameToLayer(normalLayer);
        moaiRenderer.enabled = true;
    }
    // Update is called once per frame
    private void Update()
    {
        //オブジェクトを消去します
        if (onRemoveObjFlag)
        {
            deleteTime -= Time.deltaTime;
            if (deleteTime <= 0)
            {
                onRemoveObjFlag = false;
                gameObject.SetActive(false);
            }
        }
        if (tragetCamera != null)
        {
            if (tragetCamera.transform.position.x - reMoveX > gameObject.transform.localPosition.x)
            {
                ObjectBreak();
            }
        }
        //ダメージを受けたと時の処理
        if (isDamage)
        {
            isDamage = false;
            if (canDamage)
            {
                StartCoroutine(IsDamageIEnumerator());
            }
        }
    }
    //ダメージを受けたときのアニメーションを制御するコルーチン
    private IEnumerator IsDamageIEnumerator()
    {
        enemyController.EnemyAnimator.SetBool("IsDamage", true);
        yield return new WaitForSeconds(0.25f);
        canDamage = true;
        enemyController.EnemyAnimator.SetBool("IsDamage", false);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player" && acquisitionPoint == 0 && playerMove.canDamage)
        {
            isDamage = true;
            //Hpをへらす
            foundationHP -= playerMove.AttackPower;

            //新しく生成したオブジェクト
            Singleton.Instance.damageTextSpawn.CreatDamageEffect(transform.position, (int)playerMove.AttackPower);

            //ObjHｐがOになった時
            if (foundationHP <= 0)
            {
                playerMove.enemyBreak = true;
                ObjectBreak();
                playerMove.IsGround = false;
                if (spawnStarNum != 0)
                {
                    Singleton.Instance.starGenerator.ObstaclesToStarSpon(this.transform.position, spawnStarNum);
                }
            }
        }
    }

    private void ObjectBreak()
    {
        obstaclesHeadObj.SetActive(false);
        IsDestroyed = true;
        Singleton.Instance.soundManager.StopObstaclesSe();
        Singleton.Instance.soundManager.PlayObstaclesSe(breakSeNum);
        //壊れたときにキャラクターと当たり判定を持たなくします
        //レイヤーの変更
        //レイヤーはやりすぎか？コライダー消去の方がよけれは修正要
        gameObject.layer = LayerMask.NameToLayer(breakLayer);
        acquisitionPoint++;
        breakEffect.SetActive(true);
        onRemoveObjFlag = true;
        moaiRenderer.enabled = false;
        obstacleSpawn.ActiveCount--;
    }
}
