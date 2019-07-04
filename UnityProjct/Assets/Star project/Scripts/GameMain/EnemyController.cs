using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    /// <summary>
    /// エネミーの現状ステータス
    /// </summary>
    public enum EnemyState
    {
        None,
        Search,         //索敵
        Discovery,      //プレイヤー発見
        ReMovePosition, //元の位置に戻る
        StunAttack,     //スタン攻撃
        Stun,           //スタン攻撃後動かなくなる
        Died,           //死んだ（壊れた）とき）
    }
    /// <summary>
    /// エネミーのステータスを外部入力できるように設定
    /// </summary>
    public EnemyState enemyState
    {
        get; private set;
    }
    /// <summary>
    /// エネミーのタイプ
    /// </summary>
    public enum EnemyTyp
    {
        None,
        NotMoveEnemy,//静動エネミー
        MoveEnemy,   //動くエネミー
        AirMoveEnemy,//空飛ぶエネミー
    }
    [HideInInspector] public EnemyTyp enemyTyp;
    //エネミーの『ObstacleManager』参照
    private ObstacleManager obstacleManager;

    //プレイヤーポジション
    private GameObject playerObj = null;
    //エネミー動きはじめの「startPos」
    private Vector3 startPos = Vector3.zero;
    //エネミー動き終点の「endPos」
    private Vector3 endPos = Vector3.zero;
    // 移動距離
    [HideInInspector] public Vector3 amountOfMovement;
    //移動スピード
    [HideInInspector] public float searchMoveSpeed;
    //攻撃時のスピード
    [HideInInspector] public float lockOnMoveSpeed;
    //戻るスピード
    [HideInInspector] public float attackUpOnMoveSpeed;
    ///[HideInInspector] public float removeMoveSpeed;
    //移動方向
    private Vector3 moveForce = Vector3.zero;
    //進む戻る
    private bool isReturn;
    private Rigidbody enemyRigidbody = null;
    //戻るポジション
    private Vector3 removePosition = Vector3.zero;
    //戻る移動方向
    private Vector3 removeForce = Vector3.zero;
    //攻撃時待機時間
    [HideInInspector] public float defaultAttackTime;
    //攻撃待機時間を計測するカウンター
    private float attackTime = 0;
    //砂煙エフェクト
    [SerializeField] private GameObject sandEffect = null;
    //攻撃フラグ
    private bool attack;
    //スタンフラグ
    private bool stun;
    //エネミー用のanimator
    [SerializeField] private Animator enemyAnimator;
    //オブジェクトを配置してからのStandby状態を管理するフラグ
    private bool playObj;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="player">プレイヤオブジェクト</param>
    public void Init(GameObject player)
    {
        playObj = false;
        enemyState = EnemyState.None;
        playerObj = player;
        enemyRigidbody = GetComponent<Rigidbody>();

        // StartPosをオブジェクトに初期位置に設定
        startPos = transform.localPosition;
        var pos = transform.localPosition;
        endPos = pos += amountOfMovement;
        if (searchMoveSpeed == 0) searchMoveSpeed = 1;// 1秒当たりの移動量を算出
        //スタート位置と移動終点の差を確認
        //スタート位置が終点よりも大きいときスタート位置と終点を入れ替える
        if (startPos.x > endPos.x)
        {
            isReturn = true;
            var temp = startPos;
            startPos = endPos;
            endPos = temp;
        }
        else isReturn = false;
        removePosition = transform.localPosition;
        //エネミータイプによって「enemyRigidbody」と「FreezePosition」を設定する
        if (enemyTyp == EnemyTyp.NotMoveEnemy)
        {
            //「enemyRigidbody」を消去する
            FreezePositionOll(); Destroy(enemyRigidbody);
        }
        else FreezePositionAir();
        //-----------各種初期化--------
        obstacleManager = GetComponent<ObstacleManager>();
        SandEffectPlay(false);
        attack = false;
        stun = false;
        attackTime = 0.0f;
        //Standbyコルーチンを発動
        StartCoroutine(standDelayTime());
    }
    /// <summary>
    /// Standbyコルーチン
    /// </summary>
    /// objectPool使用により配置移動が起こると当たり判定が入って「スタン状態」に
    /// なってしまうためコルーチンを使用して『playObjフラグ』＝trueになるまで
    /// 当たり判定を行わないようにした
    /// <returns></returns>
    private IEnumerator standDelayTime()
    {
        yield return new WaitForSeconds(1.0f);
        enemyState = EnemyState.Search;
        playObj = true;
    }
    /// <summary>
    /// エネミーアップデート
    /// </summary>
    private void Update()
    {
        switch (enemyTyp)
        {
            case EnemyTyp.AirMoveEnemy:
                switch (enemyState)
                {
                    case EnemyState.Search:
                        SearchUpdate();
                        break;
                    case EnemyState.Discovery:
                        DiscoveryUpdate();
                        break;
                    case EnemyState.StunAttack:
                        StunAttackUpdate();
                        break;
                    //case EnemyState.ReMovePosition:
                    //    ReMovePositionUpdate();
                    //    break;
                    case EnemyState.Stun:
                        StanUpdate();
                        break;
                    case EnemyState.Died:
                        break;
                }
                break;
            case EnemyTyp.MoveEnemy:
                switch (enemyState)
                {
                    case EnemyState.Search:
                        SearchUpdate();
                        break;
                    case EnemyState.Discovery:
                        MoveEnemy_DiscoveryUpdate();
                        break;
                    case EnemyState.StunAttack:
                        StunAttackUpdate();
                        break;
                    //case EnemyState.ReMovePosition:
                    //    ReMovePositionUpdate();
                    //    break;
                    case EnemyState.Stun:
                        StanUpdate();
                        break;
                    case EnemyState.Died:
                        break;
                }
                break;
            case EnemyTyp.NotMoveEnemy:
                break;
        }
    }

    /// <summary>
    /// モアイの間合いに入った時、一定時間後にスタン攻撃します
    /// </summary>
    private void DiscoveryUpdate()
    {
        enemyAnimator.SetBool("IsAttackPreparation", true);
        if (enemyRigidbody != null) FreezePositionOll();
        //プレイヤーポジション取得
        var playerPos = playerObj.transform.position;
        //自分の座標をプレイヤーの座標からベクトル作成
        Vector3 enemyVec = playerPos - gameObject.transform.localPosition;
        //単位ベクトル作成（上記のベクトル）
        Vector3 enemyVecE = enemyVec.normalized;
        //長さを調節
        enemyVecE.z = 0;

        removeForce = enemyVecE;
        //攻撃
        attackTime += Time.deltaTime;
        //攻撃待ち時間が経過したら攻撃
        if (attackTime >= defaultAttackTime)
        {
            enemyAnimator.SetBool("IsAttackPreparation", false);
            attack = true;
            if (enemyRigidbody != null) FreezePositionSet();
            enemyRigidbody.AddForce(enemyVecE * attackUpOnMoveSpeed, ForceMode.Impulse);

            enemyState = EnemyState.StunAttack;
        }
        //どんな状態でもプレイヤーに倒されたら死ぬ
        if (obstacleManager.isDestroyed) enemyState = EnemyState.Died;
    }
    /// <summary>
    /// モアイの間合いに入った時、一定時間後にスタン攻撃します
    /// </summary>
    private void MoveEnemy_DiscoveryUpdate()
    {
        enemyAnimator.SetBool("IsAttackPreparation", true);
        if (enemyRigidbody != null) FreezePositionOll();
        //プレイヤーポジション取得
        var playerPos = playerObj.transform.position;
        //自分の座標をプレイヤーの座標からベクトル作成
        Vector3 enemyVec = playerPos - gameObject.transform.localPosition;
        //単位ベクトル作成（上記のベクトル）
        Vector3 enemyVecE = enemyVec.normalized;
        //長さを調節
        enemyVecE.z = 0;
        enemyVecE.y = 0;

        removeForce = enemyVecE;
        //攻撃
        attackTime += Time.deltaTime;
        //攻撃待ち時間が経過したら攻撃
        if (attackTime >= defaultAttackTime)
        {
            enemyAnimator.SetBool("IsAttackPreparation", false);
            if (enemyRigidbody != null) FreezePositionSet();
            enemyRigidbody.AddForce(enemyVecE * lockOnMoveSpeed, ForceMode.Impulse);
            SandEffectPlay(true);
            enemyState = EnemyState.StunAttack;
        }
        //どんな状態でもプレイヤーに倒されたら死ぬ
        if (obstacleManager.isDestroyed) enemyState = EnemyState.Died;
    }
    //スタン攻撃のアップデート
    private void StunAttackUpdate()
    {
        var velocity = enemyRigidbody.velocity;
        // 下降中
        if (velocity.y < 0 && attack)
        {
            enemyRigidbody.AddForce(removeForce * lockOnMoveSpeed, ForceMode.Impulse);
            SandEffectPlay(true);
            attack = false;
        }
        //どんな状態でもプレイヤーに倒されたら死ぬ
        if (obstacleManager.isDestroyed) enemyState = EnemyState.Died;
    }
    /// <summary>
    /// 攻撃後スタン状態
    /// </summary>
    /// スタン状態は何もできないが壊させた判定だけはできる
    private void StanUpdate()
    {
        //どんな状態でもプレイヤーに倒されたら死ぬ
        if (obstacleManager.isDestroyed) enemyState = EnemyState.Died;
        return;
    }
    /// <summary>
    /// 索敵状態
    /// スタート位置と終了位置を反復移動します
    /// </summary>
    private void SearchUpdate()
    {
        var velocity = enemyRigidbody.velocity;
        //+方向に進む
        if (!isReturn)
        {
            if (transform.localPosition.x > endPos.x)
            {
                isReturn = true;
                var rot = -90;
                transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));//方向転換
            }
            moveForce.x = searchMoveSpeed;
        }
        //-方向に進む
        else
        {
            if (transform.localPosition.x < startPos.x)
            {
                isReturn = false;
                var rot = 90;
                transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));//方向転換
            }
            moveForce.x = -searchMoveSpeed;
        }

        velocity.x = moveForce.x;
        enemyRigidbody.velocity = velocity;
        //どんな状態でもプレイヤーに倒されたら死ぬ
        if (obstacleManager.isDestroyed) enemyState = EnemyState.Died;
    }
    /// <summary>
    /// スタン攻撃後元の位置に戻ります
    /// ※現状使いません
    /// </summary>
    //void ReMovePositionUpdate()
    //{
    //    var velocity = enemyRigidbody.velocity;
    //    velocity = -removeForce * removeMoveSpeed;
    //    enemyRigidbody.velocity = velocity;
    //    Debug.Log("removePosition : " + removePosition);
    //    Debug.Log("transform.localPosition : " + transform.localPosition);
    //    if (transform.localPosition.y <= removePosition.y + difference && transform.localPosition.y >= removePosition.y - difference)
    //    {
    //        Debug.Log("帰りました");
    //        FreezePositionAir();
    //        enemyState = EnemyState.Search;
    //    }
    //}
    /// <summary>
    /// Rigidbodyのフリーズポジション、ローテーションの固定
    /// 通常時の設定です
    /// </summary>
    private void FreezePositionOll()
    {
        enemyRigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
    }
    /// <summary>
    /// Rigidbodyのフリーズポジション、ローテーションの固定
    /// スタン攻撃時の設定です
    /// </summary>
    private void FreezePositionSet()
    {
        enemyRigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }
    /// <summary>
    /// Rigidbodyのフリーズポジション、ローテーションの固定
    /// 空中浮遊状態のの設定です
    /// </summary>
    private void FreezePositionAir()
    {
        enemyRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }
    //当たり判定から出たときの処理
    private void OnCollisionEnter(Collision collision)
    {
        if (Singleton.Instance.gameSceneController.gameMainState == StarProject.Gamemain.GameSceneController.GameMainState.Play && playObj)
        {
            if (enemyTyp == EnemyTyp.AirMoveEnemy)
            {
                if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground" && enemyState == EnemyState.StunAttack || LayerMask.LayerToName(collision.gameObject.layer) == "Player" && enemyState == EnemyState.StunAttack)// && enemyTyp == EnemyTyp.MoveEnemy )
                {
                    if (!stun)
                    {
                        stun = true;
                        StartCoroutine(SandEffectEnumerator());
                    }
                }
            }
            else
            {
                if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground" && enemyState == EnemyState.StunAttack || LayerMask.LayerToName(collision.gameObject.layer) == "Player" && enemyState == EnemyState.StunAttack)// && enemyTyp == EnemyTyp.MoveEnemy )
                {
                    stun = true;
                    StartCoroutine(SandEffectEnumerator());
                }
            }
        }
    }
    //当たり判定に居続ける処理
    private void OnCollisionStay(Collision collision)
    {
        //プレイヤーに対してスタンアタックを食らわしたときの処理
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player" && enemyState == EnemyState.StunAttack && playObj)
        {
            if (!stun)
            {
                stun = true;
                StartCoroutine(SandEffectEnumerator());
            }
        }
    }
    /// <summary>
    /// スタン状態で地面に着くときにエフェクト非表示＆リジットボディを消去します
    /// </summary>
    /// <returns></returns>
    private IEnumerator SandEffectEnumerator()
    {
        stun = true;
        yield return new WaitForSeconds(1.0f);
        enemyState = EnemyState.Stun;
        if (enemyRigidbody != null) FreezePositionOll();
        yield return null;
        Destroy(enemyRigidbody);
        yield return new WaitForSeconds(1.0f);
        SandEffectPlay(false);
    }
    //当たり判定に居続ける処理
    private void OnTriggerStay(Collider collision)
    {
        //プレイキャラクターを発見
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player" && enemyState == EnemyState.Search)
        {
            //プレイヤーが手前に居るとき
            if (playerObj.transform.position.x < transform.localPosition.x)
            {
                var rot = -90;
                transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
            }
            else
            {
                var rot = 90;
                transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
            }
            attackTime = 0;
            enemyState = EnemyState.Discovery;
        }
    }
    //当たり判定から出たときの処理
    private void OnTriggerExit(Collider collision)
    {
        //プレイヤーが攻撃していないときにプレイヤー発見エリアから脱出したときの処理
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player" && enemyState == EnemyState.Discovery && !attack)
        {
            enemyAnimator.SetBool("IsAttackPreparation", false);
            if (enemyRigidbody != null) FreezePositionAir();
            //元の移動方向に戻るために「isReturn」を判断
            if (isReturn)
            {
                var rot = -90;
                transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
            }
            else
            {
                var rot = 90;
                transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
            }
            enemyState = EnemyState.Search;
        }
    }
    /// <summary>
    /// 砂煙エフェクトのSetActiveを管理
    /// </summary>
    /// <param name="isPlay"></param>
    private void SandEffectPlay(bool isPlay)
    {
        sandEffect.SetActive(isPlay);
    }
}

