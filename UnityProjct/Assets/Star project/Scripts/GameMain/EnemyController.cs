using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StarProject.Gamemain;

public class EnemyController : MonoBehaviour
{
    /// <summary>
    /// エネミーの現状ステータス
    /// </summary>
    public enum EnemyState
    {
        None,
        Search,         // 索敵
        Discovery,      // プレイヤー発見
        StunAttack,     // スタン攻撃
        Stun,           // スタン攻撃後動かなくなる
        Died,           // 死んだ（壊れた）とき）
    }
    /// <summary>
    /// エネミーのステータスを外部入力できるように設定
    /// </summary>
    public EnemyState SetEnemyState
    {
        get; private set;
    }
    /// <summary>
    /// エネミーのタイプ
    /// </summary>
    public enum EnemyTyp
    {
        None,
        NotMoveEnemy,// 静動エネミー
        MoveEnemy,   // 動くエネミー
        AirMoveEnemy,// 空飛ぶエネミー
    }
    private EnemyTyp enemyTyp;
    // エネミーの『ObstacleManager』参照
    private ObstacleManager obstacleManager;
    // プレイヤーポジション
    private GameObject playerObj = null;
    // エネミー動きはじめの「startPos」
    private Vector3 startPos = Vector3.zero;
    // エネミー動き終点の「endPos」
    private Vector3 endPos = Vector3.zero;
    // 移動距離
    private Vector3 amountOfMovement;
    // 探索スピード
    private float searchMoveSpeed;
    // 攻撃時のスピード
    private float normalAttackMoveSpeed;
    // 上からの攻撃スピード
    private float airAttackMoveSpeed;
    // 移動方向
    private Vector3 moveForce = Vector3.zero;
    // 進む戻る
    private bool isReturn;
    private Rigidbody enemyRigidbody = null;
    // 戻る移動方向
    private Vector3 removeForce = Vector3.zero;
    // 攻撃時待機時間
    private float attackRugTime;
    // 攻撃待機時間を計測するカウンター
    private float attackTime = 0;
    // 砂煙エフェクト
    [SerializeField] private GameObject sandEffect = null;
    // 攻撃フラグ
    private bool attack;
    // スタンフラグ
    private bool stun;
    // エネミー用のanimator
    [SerializeField] private Animator enemyAnimator;
    public Animator EnemyAnimator
    {
        get; private set;
    }
    // オブジェクトを配置してからのStandby状態を管理するフラグ
    private bool playObj;
    /// <summary>
    /// エネミーデータのセッティング
    /// </summary>
    /// <param name="enemyType">エネミーの種類</param>
    /// <param name="amountOfMovement">移動距離</param>
    /// <param name="moveSpeed">移動速度</param>
    /// <param name="normalAttackMoveSpeed">地面に居るときの攻撃速度</param>
    /// <param name="airAttackMoveSpeed">空中にいる際の攻撃速度</param>
    /// <param name="attackRugTime">攻撃するまでの待機時間</param>
    public void SetEnemyDatas(string enemyType, Vector3 amountOfMovement, float moveSpeed, float normalAttackMoveSpeed, float airAttackMoveSpeed, float attackRugTime)
    {
        SetEnemyType(enemyType);
        this.amountOfMovement = amountOfMovement;
        this.searchMoveSpeed = moveSpeed;
        this.normalAttackMoveSpeed = normalAttackMoveSpeed;
        this.airAttackMoveSpeed = airAttackMoveSpeed;
        this.attackRugTime = attackRugTime;
    }
    private void SetEnemyType(string type)
    {
        switch (type)
        {
            case "NotMoveEnemy":
                enemyTyp = EnemyTyp.NotMoveEnemy;
                break;
            case "MoveEnemy":
                enemyTyp = EnemyTyp.MoveEnemy;
                break;
            case "AirMoveEnemy":
                enemyTyp = EnemyTyp.AirMoveEnemy;
                break;
        }
    }
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="player">プレイヤオブジェクト</param>
    public void Init(GameObject player)
    {
        playObj = false;
        SetEnemyState = EnemyState.None;
        playerObj = player;
        enemyRigidbody = GetComponent<Rigidbody>();
        EnemyAnimator = enemyAnimator;
        // StartPosをオブジェクトに初期位置に設定
        startPos = transform.localPosition;
        var pos = transform.localPosition;
        endPos = pos += amountOfMovement;
        if (searchMoveSpeed == 0) searchMoveSpeed = 1;// 1秒当たりの移動量を算出
        // スタート位置と移動終点の差を確認
        // スタート位置が終点よりも大きいときスタート位置と終点を入れ替える
        if (startPos.x > endPos.x)
        {
            isReturn = true;
            var temp = startPos;
            startPos = endPos;
            endPos = temp;
        }
        else isReturn = false;
        // エネミータイプによって「enemyRigidbody」と「FreezePosition」を設定する
        if (enemyTyp == EnemyTyp.NotMoveEnemy)
        {
            // 「enemyRigidbody」を消去する
            FreezePositionOll(); Destroy(enemyRigidbody);
        }
        else FreezePositionAir();
        // -----------各種初期化--------
        obstacleManager = GetComponent<ObstacleManager>();
        SandEffectPlay(false);
        attack = false;
        stun = false;
        attackTime = 0.0f;
        // Standbyコルーチンを発動
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
        SetEnemyState = EnemyState.Search;
        playObj = true;
    }
    /// <summary>
    /// エネミーアップデート
    /// </summary>
    private void Update()
    {
        if (GameSceneController.isPlaying)
        {
            switch (enemyTyp)
            {
                case EnemyTyp.AirMoveEnemy:
                    switch (SetEnemyState)
                    {
                        case EnemyState.Search:
                            SearchUpdate();
                            break;
                        case EnemyState.Discovery:
                            DiscoveryUpdate("AirMoveEnemy");
                            break;
                        case EnemyState.StunAttack:
                            StunAttackUpdate();
                            break;
                        case EnemyState.Stun:
                            StanUpdate();
                            break;
                        case EnemyState.Died:
                            break;
                    }
                    break;
                case EnemyTyp.MoveEnemy:
                    switch (SetEnemyState)
                    {
                        case EnemyState.Search:
                            SearchUpdate();
                            break;
                        case EnemyState.Discovery:
                            DiscoveryUpdate("MoveEnemy");
                            break;
                        case EnemyState.StunAttack:
                            StunAttackUpdate();
                            break;
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
    }
    /// <summary>
    /// モアイの間合いに入った時、一定時間後にスタン攻撃します
    /// </summary>
    private void DiscoveryUpdate(string type)
    {
        enemyAnimator.SetBool("IsAttackPreparation", true);
        if (enemyRigidbody != null) FreezePositionOll();
        // プレイヤーポジション取得
        var playerPos = playerObj.transform.position;
        // 自分の座標をプレイヤーの座標からベクトル作成
        Vector3 enemyVec = playerPos - gameObject.transform.localPosition;
        // 単位ベクトル作成（上記のベクトル）
        Vector3 enemyVecE = enemyVec.normalized;
        // 長さを調節
        enemyVecE.z = 0;
        removeForce = enemyVecE;
        // 攻撃
        attackTime += Time.deltaTime;
        // 攻撃待ち時間が経過したら攻撃
        if (attackTime >= attackRugTime)
        {
            enemyAnimator.SetBool("IsAttackPreparation", false);
            attack = true;
            if (enemyRigidbody != null) FreezePositionSet();
            if (type == "AirMoveEnemy")
                enemyRigidbody.AddForce(enemyVecE * airAttackMoveSpeed, ForceMode.Impulse);
            else
                enemyRigidbody.AddForce(enemyVecE * normalAttackMoveSpeed, ForceMode.Impulse);
            SetEnemyState = EnemyState.StunAttack;
        }
        // どんな状態でもプレイヤーに倒されたら死ぬ
        if (obstacleManager.IsDestroyed) SetEnemyState = EnemyState.Died;
    }
    // スタン攻撃のアップデート
    private void StunAttackUpdate()
    {
        var velocity = enemyRigidbody.velocity;
        // 下降中
        if (velocity.y < 0 && attack)
        {
            enemyRigidbody.AddForce(removeForce * normalAttackMoveSpeed, ForceMode.Impulse);
            SandEffectPlay(true);
            attack = false;
        }
        else
        {
            return;
        }
        // どんな状態でもプレイヤーに倒されたら死ぬ
        if (obstacleManager.IsDestroyed) SetEnemyState = EnemyState.Died;
    }
    /// <summary>
    /// 攻撃後スタン状態
    /// </summary>
    /// スタン状態は何もできないが壊させた判定だけはできる
    private void StanUpdate()
    {
        // どんな状態でもプレイヤーに倒されたら死ぬ
        if (obstacleManager.IsDestroyed) SetEnemyState = EnemyState.Died;
        return;
    }
    /// <summary>
    /// 索敵状態
    /// スタート位置と終了位置を反復移動します
    /// </summary>
    private void SearchUpdate()
    {
        var velocity = enemyRigidbody.velocity;
        // +方向に進む
        if (!isReturn)
        {
            if (transform.localPosition.x > endPos.x)
            {
                isReturn = true;
                LookRotateLeft();
            }
            moveForce.x = searchMoveSpeed;
        }
        // -方向に進む
        else
        {
            if (transform.localPosition.x < startPos.x)
            {
                isReturn = false;
                LookRotateRight();
            }
            moveForce.x = -searchMoveSpeed;
        }
        velocity.x = moveForce.x;
        enemyRigidbody.velocity = velocity;
        // どんな状態でもプレイヤーに倒されたら死ぬ
        if (obstacleManager.IsDestroyed) SetEnemyState = EnemyState.Died;
    }
    // 当たり判定から出たときの処理
    private void OnCollisionEnter(Collision collision)
    {
        if (CheckIsPlaying() && playObj)
        {
            if (enemyTyp == EnemyTyp.AirMoveEnemy)
            {
                if (CheckChangeStanStata(collision) && !stun)
                {
                    StartCoroutine(SandEffectEnumerator());
                }
            }
            else
            {
                if (CheckChangeStanStata(collision))
                {
                    StartCoroutine(SandEffectEnumerator());
                }
            }
        }
    }
    // 当たり判定に居続ける処理
    private void OnCollisionStay(Collision collision)
    {
        //プレイヤーに対してスタンアタックを食らわしたときの処理
        if (CheckCollisionHitPlayer(collision) && SetEnemyState == EnemyState.StunAttack && playObj)
        {
            if (!stun)
            {
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
        SetEnemyState = EnemyState.Stun;
        if (enemyRigidbody != null) FreezePositionOll();
        yield return null;
        Destroy(enemyRigidbody);
        yield return new WaitForSeconds(1.0f);
        SandEffectPlay(false);
    }
    // 当たり判定に居続ける処理
    private void OnTriggerStay(Collider collider)
    {
        // プレイキャラクターを発見
        if (CheckColliderHitPlayer(collider) && SetEnemyState == EnemyState.Search)
        {
            // プレイヤーが手前に居るとき
            if (playerObj.transform.position.x < transform.localPosition.x)
            {
                LookRotateLeft();
            }
            else
            {
                LookRotateRight();
            }
            attackTime = 0;
            SetEnemyState = EnemyState.Discovery;
        }
    }
    // 当たり判定から出たときの処理
    private void OnTriggerExit(Collider collider)
    {
        // プレイヤーが攻撃していないときにプレイヤー発見エリアから脱出したときの処理
        if (CheckColliderHitPlayer(collider) && SetEnemyState == EnemyState.Discovery && !attack)
        {
            enemyAnimator.SetBool("IsAttackPreparation", false);
            if (enemyRigidbody != null) FreezePositionAir();
            // 元の移動方向に戻るために「isReturn」を判断
            if (isReturn)
            {
                LookRotateLeft();
            }
            else
            {
                LookRotateRight();
            }
            SetEnemyState = EnemyState.Search;
        }
    }
    /// <summary>
    /// 攻撃後”地面”もしくは”メインキャラ”に当たったかどうか
    /// 当たっていたらスタン状態になる
    /// </summary>
    /// <param name="collision"></param>
    /// <returns></returns>
    bool CheckChangeStanStata(Collision collision)
    {
        if (CheckCollisionHitGround(collision) && SetEnemyState == EnemyState.StunAttack || CheckCollisionHitPlayer(collision) && SetEnemyState == EnemyState.StunAttack)
            return true;
        else return false;
    }
    /// <summary>
    /// ゲーム進行中判定
    /// </summary>
    bool CheckIsPlaying()
    {
        if (Singleton.Instance.gameSceneController.gameMainState == StarProject.Gamemain.GameSceneController.GameMainState.Play)
            return true;
        else return false;

    }
    /// <summary>
    /// 地面との当たり判定(collision)
    /// </summary>
    /// <param name="collision"></param>
    /// <returns></returns>
    bool CheckCollisionHitGround(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground")
            return true;
        else return false;
    }
    /// <summary>
    /// メインキャラとの当たり判定（collider）
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    bool CheckColliderHitPlayer(Collider collider)
    {
        if (LayerMask.LayerToName(collider.gameObject.layer) == "Player")
            return true;
        else return false;
    }
    /// <summary>
    /// メインキャラとの当たり判定（collision）
    /// </summary>
    /// <param name="collision"></param>
    /// <returns></returns>
    bool CheckCollisionHitPlayer(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player")
            return true;
        else return false;
    }
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
    /// <summary>
    /// 右向く（90度まで回転）
    /// </summary>
    void LookRotateRight()
    {
        var rot = 90;
        transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
    }
    /// <summary>
    /// 左向く（-90度まで回転）
    /// </summary>
    void LookRotateLeft()
    {
        var rot = -90;
        transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
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

