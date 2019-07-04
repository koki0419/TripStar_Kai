using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    //-------------Unityコンポーネント関係-------------------
    //エフェクト
    [SerializeField] private GameObject breakEffect = null;
    [SerializeField] private Renderer moaiRenderer = null;
    [HideInInspector] public Camera tragetCamera;
    //-------------クラス関係--------------------------------

    //『PlayerMove』を取得します
    [HideInInspector]
    public PlayerMove playerMove;
    [HideInInspector]
    public ObstacleSpawn obstacleSpawn;
    //-------------数値用変数--------------------------------
    //生成する星の数
    [HideInInspector]
    public int spawnStarNum = 0;

    //ポイントを獲得した回数
    private int acquisitionPoint = 0;

    private float deleteTime = 2.0f;

    //Hp
    [HideInInspector]
    public float foundationHP;
    //HpMax
    private float foundationHPMax;
    //破壊音インデックス
    private const int breakSeNum = 7;
    //オブジェクトをReMoveするポジション
    private const float reMoveX = 10.0f;

    [SerializeField] private GameObject obstaclesHeadObj = null;

    private const string normalLayer = "Obstacles";
    private const string breakLayer = "BreakObstacls";
    //-------------フラグ用変数------------------------------
    private bool onRemoveObjFlag = false;

    public bool isDestroyed
    {
        get; private set;
    }

    public void Init()
    {
        foundationHPMax = foundationHP;
        //オブジェクトを削除するかどうか
        onRemoveObjFlag = false;
        //ポイントを獲得した回数
        acquisitionPoint = 0;
        deleteTime = 2.0f;

        isDestroyed = false;
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
            if (tragetCamera.transform.position.x- reMoveX > gameObject.transform.localPosition.x)
            {
                ObjectBreak();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player" && acquisitionPoint == 0 && playerMove.canDamage)
        {
            //Hpをへらす
            foundationHP -= playerMove.attackPower;

            //新しく生成したオブジェクト
            Singleton.Instance.damageTextSpawn.CreatDamageEffect(transform.position, (int)playerMove.attackPower);

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
        isDestroyed = true;
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
        obstacleSpawn.activeCount--;
    }
}
