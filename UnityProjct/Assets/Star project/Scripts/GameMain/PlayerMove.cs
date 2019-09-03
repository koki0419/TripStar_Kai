using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // 攻撃時設定数値
    private enum PlayerAttackIndex
    {
        None,
        AttackNormal = 1000,
        ChargeAttackNormal = 1010,
        ChargeAttackDown = 1001,
        ChargeAttackUp = 1011,
    }
    private PlayerAttackIndex payerAttackIndex = PlayerAttackIndex.None;
    // オブジェクトステータス
    private enum ObjState
    {
        None,
        Normal,// 通常状態
        Stun,// スタン状態
        NotAttackMode,// スタン状態
        Attack,// 攻撃状態
        OnCharge,// チャージ中状態
        ChargeSpecial,
        CharacterGameOver,// ゲームオーバー状態
    }
    private ObjState objState = ObjState.None;
    // オブジェクトステータス
    private enum AttackState
    {
        None,
        AttackJab,// ジャブ攻撃状態
        AttackUp,// 上攻撃状態
        AttackDown,// 下攻撃状態
        ChargeAttack,// チャージ攻撃状態
    }
    private AttackState attackState = AttackState.None;
    // -------------Unityコンポーネント関係-------------------
    [SerializeField] private StarChargeController starChargeController = null;
    // 自分のアニメーションコンポーネント
    [SerializeField] private Animator animatorComponent = null;
    public Animator PlayerAnimator
    {
        get { return animatorComponent; }
    }
    private new Rigidbody rigidbody;
    [Header("エフェクト関係")]
    // スター獲得エフェクト
    [SerializeField] private GameObject starAcquisitionEffect = null;
    // チャージエフェクト1
    [SerializeField] private GameObject chargeEffect1 = null;
    // チャージエフェクト2
    [SerializeField] private GameObject chargeEffect2 = null;
    // 砂煙エフェクト
    [SerializeField] private GameObject sandEffect = null;
    // パンチエフェクト
    [SerializeField] private GameObject punchEffect = null;
    // プレイヤーの拳オブジェクト
    [SerializeField] private GameObject playerFistObj = null;
    // 星条旗ちゃんObj
    [SerializeField] private GameObject seijoukityanObj = null;
    // -------------クラス関係--------------------------------
    // 『Attack』をインスタンス
    Attack attack = new Attack();
    // -------------数値用変数--------------------------------
    [Header("プレイヤー情報")]
    // 移動速度を設定します
    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float knockbackMoveSpeedMax = 0;
    private float knockbackMoveSpeed = 0;
    // ジャンプ中の移動速度
    [SerializeField] private float airUpMoveSpeed = 0;
    [SerializeField] private float airDownMoveSpeed = 0;
    // 空気抵抗
    [SerializeField] private float dragPower = 0;
    // ジャンプ力
    [SerializeField] private float jumpPower = 0;
    [Header("プレイヤー攻撃初期情報")]
    // 初期攻撃力
    private const float foundationoffensivePower = 1000000;

    [Header("チャージ回数に掛け算される力")]
    // 攻撃力
    private const float fastOffensivePower = 700000;
    private const float secondOffensivePower = 800000;
    // 移動量
    private const float speedForce = 300;
    private const float speedForceUp = 350;
    // 現在のチャージ量
    private float chargeNow = 0.0f;
    // 現在のチャージ量
    private float chargeNowHand = 0.0f;
    // 何回チャージしたか
    private int chargeCount = 0;
    // 回転
    private float rot = 90;
    // 攻撃時Speed
    public float AttackSpeed
    {
        get; private set;
    }
    // 攻撃時パワー
    public float AttackPower
    {
        get; private set;
    }
    [Header("スタン時ステータス")]
    // スタン状態からの復活時間
    [SerializeField] private float stunTime;
    // スタン時の移動量
    [SerializeField] private float stunAmountMovement;
    [SerializeField] private float stunAirAmountMovement;
    private bool positiveDirection;
    private Vector3 enemyPosition;

    [Header("攻撃時の攻撃時間")]
    [SerializeField] private float attackJabTime;
    [SerializeField] private float chargeAttackTime;
    [SerializeField] private float chargeAttackUpTime;
    [SerializeField] private float chargeAttackDownTime;

    [Header("キャラクターSEの種類番号")]
    [SerializeField] private int dashSeNum;
    [SerializeField] private int jumpSeNum;
    [SerializeField] private int chargeSeNum;
    [SerializeField] private int chargeAttackSeNum;
    [SerializeField] private int punchSeNum;
    [SerializeField] private int getStarSeNum;
    // スペシャルアニメーション時攻撃モーション番号を一時保管
    private int specialAttackNum;

    // チャージポイント使用時のユーザーゲージ上昇量
    private int starPointNormalization = 10;
    [SerializeField] private float fastChargeAmountOfIncrease = 0.1f;
    [SerializeField] private float secondChargeAmountOfIncrease = 0.075f;
    [SerializeField] private float thirdChargeAmountOfIncrease = 0.05f;
    [SerializeField] private float fourthChargeAmountOfIncrease = 0.025f;
    [SerializeField] private float fifthChargeAmountOfIncrease = 0.01f;
    [SerializeField] private float chargePointAddition = 10.0f;
    private float amountOfRotationPerSecond = 0.0f;// 一秒あたりの回転量
    private float checkTime = 0.0f;
    private int inKeyCount = 0;
    // -------------フラグ用変数------------------------------
    // アタックフラグ//ダメージをあたえられる
    public bool canDamage
    {
        get; private set;
    }
    // 地面との接触
    private bool isGround;
    public bool IsGround
    {
        set { isGround = value; }
    }
    // チャージ中かどうか
    private bool isChargeFlag;
    // ☆獲得時フラグ
    public bool IsAcquisitionStar
    {
        set; get;
    }
    // キャラクターの向き
    private bool isRightDirection;  // 右を向いている
    private bool isLeftDirection;   // 左を向いている
    private bool isUpAttack;        // 上攻撃
    private bool isDownAttack;  // 下攻撃
    private bool isAttack;      // 通常攻撃
    private bool canAttack;     // 攻撃できるか
    private bool isStun;        // スタン状態か
    private bool notKey;        // キー入力制御
    private bool rightNotKey;   // 右キー入力制御
    private bool leftNotKey;    // 左キー入力制御
    public bool IsHit           // 攻撃を当てることが出来る
    {
        get; private set;
    }
    // エネミーが破壊されたかどうか
    public bool enemyBreak
    {
        set; get;
    }
    // 攻撃をキャンセルするか
    public bool attackCancel = false;
    // スペシャル演出中か
    public bool IsSpecialProduction
    {
        private set; get;
    }
    // チャージスペシャルアップデートの進行確認
    private bool checkSpecial = false;
    // 新旧システム切替
    [SerializeField] private bool oldSystem = false;
    [SerializeField] private bool newSystem = false;
    [SerializeField] private bool characterChargeRotate = false;
    // 各種レイヤーを設定
    private const string groundLayerName = "Ground";// 地面
    private const string gameOverLineLayerName = "GameOverObj";// ゲームオーバーライン
    private const string screenOutLineLayerName = "SceneRestrictionBar";// スクリーン右端制御用
    private const string enemyLayerName = "Obstacles";// エネミー
    private const string enemyHeadLayerName = "ObstaclesHead";// エネミーの頭
    // 壁ずり対策
    private const string rightProgressionControlLayerName = "RightProgressionControlObject";// 右入力規制用
    private const string leftProgressionControlLayerName = "LeftProgressionControlObject";// 左入力規制用
    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        // プレイヤーの状態を通常状態に設定します
        objState = ObjState.Normal;
        // 右向きに指定
        transform.rotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
        // Rigidbodyを取得します
        rigidbody = GetComponent<Rigidbody>();
        dragPower = rigidbody.drag;
        // チャージゲージをリセットします
        Singleton.Instance.gameSceneController.StarChargeController.UpdateChargePoint(0);
        // ----初期化-----
        canDamage = false;
        isGround = false;
        IsAcquisitionStar = false;
        SandEffectPlay(false);
        PunchEffectPlay(false);
        GetStarEffectPlay(false);
        ChargeEffectPlay(false, false);
        isUpAttack = false;
        isDownAttack = false;
        isAttack = false;
        canAttack = true;
        isStun = false;
        IsSpecialProduction = false;
        if (!newSystem && !oldSystem)
        {
            oldSystem = true;
        }
        auraEfect.SetActive(false);
    }
    public void OnUpdate(float deltaTime)
    {
        switch (objState)
        {
            case ObjState.Normal:
                NormalModeUpdate(deltaTime);
                break;
            case ObjState.Stun:
                StanUpdate();
                break;
            case ObjState.NotAttackMode:
                NotAttackModeUpdate(deltaTime);
                break;
            case ObjState.OnCharge:
                ChargeUpdate();
                break;
            case ObjState.Attack:
                AttackUpdate();
                break;
            case ObjState.ChargeSpecial:
                ChargeSpecialUpdate();
                break;
            case ObjState.CharacterGameOver:
                CharacterGameOver();
                break;
        }
        // ☆獲得時のエフェクト発生
        if (IsAcquisitionStar)
        {
            StartCoroutine(OnGetStar());
        }
    }
    // --------------関数-----------------------------
    /// <summary>
    /// スタンアタックを食らったときにキャラクターがエネミーに接触状態だと「OnCollisionEnter」だけだと
    /// 反応しないので「OnCollisionStay」も使用します
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay(Collision collision)
    {
        // エネミーとの当たり判定
        if (LayerMask.LayerToName(collision.gameObject.layer) == enemyLayerName)
        {
            if (collision.gameObject.GetComponent<EnemyController>().SetEnemyState == EnemyController.EnemyState.StunAttack && objState != ObjState.Stun)
            {
                enemyPosition = collision.gameObject.GetComponent<Transform>().localPosition;
                positiveDirection = false;
                isStun = true;
                objState = ObjState.Stun;
            }
        }
    }
    /// <summary>
    /// コリジョンでの当たり判定
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // ゲームオーバーの当たり判定
        if (LayerMask.LayerToName(collision.gameObject.layer) == gameOverLineLayerName)
        {
            objState = ObjState.CharacterGameOver;
        }
        // エネミーとの当たり判定
        else if (LayerMask.LayerToName(collision.gameObject.layer) == enemyLayerName)
        {
            if (collision.gameObject.GetComponent<EnemyController>().SetEnemyState == EnemyController.EnemyState.StunAttack)
            {
                enemyPosition = collision.gameObject.GetComponent<Transform>().localPosition;
                positiveDirection = false;
                isStun = true;
                objState = ObjState.Stun;
            }
            else if (isAttack)
            {
                IsHit = true;
            }
            else
            {
                // 壁ずり抑制
                StartCoroutine(KnockBackIEnumerator());
            }
        }
        else if (LayerMask.LayerToName(collision.gameObject.layer) == screenOutLineLayerName)
        {
            // 壁ずり抑制
            StartCoroutine(KnockBackIEnumerator());
        }
    }
    // プレイヤーと対象が接触中の判定
    private void OnTriggerStay(Collider other)
    {
        // 地面とエネミー頭は同じ判定→ただし、エネミーの場合はダメージが入るのでレイヤー分け
        if (CheckHitOtherGround(other))
        {
            isGround = true;
        }
        else if (LayerMask.LayerToName(other.gameObject.layer) == rightProgressionControlLayerName)
        {
            // 右壁ずり
            rightNotKey = true;
        }
        else if (LayerMask.LayerToName(other.gameObject.layer) == leftProgressionControlLayerName)
        {
            // 左壁ずり
            leftNotKey = true;
        }
    }
    // プレイヤーと対象が接触したときの判定
    private void OnTriggerEnter(Collider other)
    {
        // 地面とエネミー頭接触時の判定
        if (CheckHitOtherGround(other))
        {
            isGround = true;
            rightNotKey = false;
            leftNotKey = false;
            rigidbody.drag = dragPower;
            if (objState == ObjState.NotAttackMode)
            {
                objState = ObjState.Normal;
            }
            if (notKey)
            {
                notKey = false;
            }
        }
    }
    // プレイヤーが対象との接触しなくなった時
    private void OnTriggerExit(Collider other)
    {
        if (!rightNotKey && !leftNotKey)
        {
            if (CheckHitOtherGround(other))
            {
                isGround = false;
                if (!canDamage)
                {
                    Singleton.Instance.soundManager.StopPlayerSe();
                    // ジャンプ音再生
                    Singleton.Instance.soundManager.PlayPlayerSe(jumpSeNum);
                }
            }
        }// LRどちらかのフラグが入っているとき
        else if (rightNotKey)
        {
            // 右進行不能フラグから出たとき
            if (LayerMask.LayerToName(other.gameObject.layer) == rightProgressionControlLayerName)
            {
                rightNotKey = false;
            }
            else if (CheckHitOtherGround(other))
            {
                if (isGround)
                {
                    rightNotKey = false;
                }
                else
                {
                    isGround = false;
                    rightNotKey = false;
                    if (!canDamage)
                    {
                        Singleton.Instance.soundManager.StopPlayerSe();
                        // ジャンプ音再生
                        Singleton.Instance.soundManager.PlayPlayerSe(jumpSeNum);
                    }
                }
            }
        }
        else if (leftNotKey)
        {
            // 右進行不能フラグから出たとき
            if (LayerMask.LayerToName(other.gameObject.layer) == leftProgressionControlLayerName)
            {
                leftNotKey = false;
            }
            else if (CheckHitOtherGround(other))
            {
                if (isGround)
                {
                    leftNotKey = false;
                }
                else
                {
                    isGround = false;
                    leftNotKey = false;
                    if (!canDamage)
                    {
                        Singleton.Instance.soundManager.StopPlayerSe();
                        // ジャンプ音再生
                        Singleton.Instance.soundManager.PlayPlayerSe(jumpSeNum);
                    }
                }
            }
        }
    }
    /// <summary>
    /// ノックバックのコルーチン
    /// </summary>
    /// <returns></returns>
    private IEnumerator KnockBackIEnumerator()
    {
        notKey = true;
        knockbackMoveSpeed = knockbackMoveSpeedMax;
        yield return new WaitForSeconds(0.5f);
        knockbackMoveSpeed = 0;
        if (isGround) notKey = false;
    }
    /// <summary>
    /// 対象が地面と接触しているかどうか
    /// </summary>
    /// <param name="other">対象</param>
    /// <returns></returns>
    private bool CheckHitOtherGround(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == groundLayerName || LayerMask.LayerToName(other.gameObject.layer) == enemyHeadLayerName)
            return true;
        else return false;
    }
    /// <summary>
    /// キャラクターの移動です
    /// </summary>
    /// <param name="horizontal">横移動のキー入力値</param>
    /// <param name="deltaTime">GameSceneManagerから受け取ります</param>
    void CharacterMove(float horizontal, float deltaTime)
    {
        // 移動
        var velocity = rigidbody.velocity;
        velocity.x = horizontal * moveSpeed;
        // 地面に接触していない時
        if (!isGround)
        {
            velocity.x = horizontal * airUpMoveSpeed;
            // 下降中
            // ジャンプの上りと降りでスピードが異なるため
            // dragを下げている
            if (velocity.y < 0)
            {
                rigidbody.drag = 0;
                velocity.x = horizontal * airDownMoveSpeed;
            }
        }
        // knockbackを受けたらキー入力を禁止する
        if (notKey)
        {
            // キャラクターの向きに合わせて
            // x軸にknockbackさせる
            if (horizontal > 0)
            {
                velocity.x = -knockbackMoveSpeed;
            }
            else if (horizontal < 0)
            {
                velocity.x = knockbackMoveSpeed;
            }
        }
        // 左右入力規制が入った時
        // 入力を禁止する
        if (rightNotKey && horizontal > 0)
        {
            velocity.x = 0;
        }
        else if (leftNotKey && horizontal < 0)
        {
            velocity.x = 0;
        }
        rigidbody.velocity = velocity;// velocityの更新
        // キャラクターの向きを修正します
        // 入力キーの方向に向きを変えます
        if (horizontal > 0)
        {
            // 右向き
            transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
            isRightDirection = true;
            isLeftDirection = false;
        }
        else if (horizontal < 0)
        {
            // 左向き
            transform.localRotation = Quaternion.AngleAxis(-rot, new Vector3(0, 1, 0));
            isRightDirection = false;
            isLeftDirection = true;
        }
    }
    /// <summary>
    /// キャラクターの向きを変更します
    /// 右向き左向きに変更します
    /// rot(90°）回転させます
    /// </summary>
    /// <param name="horizontal">左右キー入力値</param>
    void DirectionMove(float horizontal)
    {
        // キャラクターの向きを修正します
        // 入力キーの方向に向きを変えます
        if (horizontal > 0)
        {
            // 右向き
            transform.rotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
            isRightDirection = true;
            isLeftDirection = false;
        }
        else if (horizontal < 0)
        {
            // 左向き
            transform.rotation = Quaternion.AngleAxis(-rot, new Vector3(0, 1, 0));
            isRightDirection = false;
            isLeftDirection = true;
        }

    }
    /// <summary>
    /// attack時の手の大きさを大きくする
    /// </summary>
    /// <param name="charge"></param>
    /// <returns></returns>
    void ChargeAttackHand(float charge)
    {
        var chargeMax = Singleton.Instance.gameSceneController.ChargePointManager.StarChildCountMax;
        var charaHand = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        // チャージ量の+-量
        float chargeProportion = fastChargeAmountOfIncrease * 10;

        if (chargeNowHand <= charge)
        {
            chargeNowHand += chargeProportion;
        }
        charaHand.SetBlendShapeWeight(0, chargeNowHand / chargeMax * 100);
    }
    /// <summary>
    /// チャージ時のチャージ量
    /// 何回チャージできるのか（Fillを何回0～1にできるのか）を返します
    /// </summary>
    /// <param name="charge"></param>
    /// <returns></returns>
    float OnCharge(float charge, bool checkChargeUp, int chargeUpGoodLuckValue)
    {
        // マックスのチャージ量
        var chargeMax = charge;
        if (charge >= 1 && charge < 2)
        {
            charge = 1;
        }
        else if (charge >= 2 && charge < 3)
        {
            charge = 2;
        }
        else if (charge >= 3 && charge < 4)
        {
            charge = 3;
        }
        else if (charge >= 4 && charge < 5)
        {
            charge = 4;
        }
        else if (charge >= 5)
        {
            charge = 5;
        }
        else
        {
            charge = 0;
            chargeNow = 0.0f;
        }

        if (charge != 0 && chargeNow <= chargeMax && chargeCount < charge)
        {
            //チャージアップしないときは1;
            float chargePointUp = 0;
            if (checkChargeUp)
            {
                chargePointUp = chargePointAddition;
            }
            else
            {
                chargePointUp = 1;
            }
            float chargeUpValue = 0.0f;
            if (chargeUpGoodLuckValue != 0)
            {
                chargeUpValue = (float)chargeUpGoodLuckValue / 100.0f + 1.0f;
            }
            else
            {
                chargeUpValue = 1.0f;
            }
            switch (chargeCount)
            {
                case 0:
                    chargeNow += fastChargeAmountOfIncrease * chargePointUp * chargeUpValue;
                    break;
                case 1:
                    chargeNow += secondChargeAmountOfIncrease * chargePointUp * chargeUpValue;
                    break;
                case 2:
                    chargeNow += thirdChargeAmountOfIncrease * chargePointUp * chargeUpValue;
                    break;
                case 3:
                    chargeNow += fourthChargeAmountOfIncrease * chargePointUp * chargeUpValue;
                    break;
                case 4:
                    chargeNow += fifthChargeAmountOfIncrease * chargePointUp * chargeUpValue;
                    break;
            }
            if (chargeNow >= 1)
            {
                chargeCount++;
                if (chargeCount < charge)
                {
                    chargeNow = 0.0f;
                }
            }
        }
        return chargeNow;
    }
    /// <summary>
    /// 攻撃の種類を選択します
    /// </summary>
    /// <param name="attackNum"></param>
    void OnAttackMotion(int attackNum)
    {
        switch (attackNum)
        {
            case (int)PlayerAttackIndex.AttackNormal:
                CharacterAnimation("punch");
                attackState = AttackState.AttackJab;
                break;
            case (int)PlayerAttackIndex.ChargeAttackNormal:
                CharacterAnimation("chargepunch");
                FreezeChargeAttack();
                attackState = AttackState.ChargeAttack;
                break;
            case (int)PlayerAttackIndex.ChargeAttackDown:
                CharacterAnimation("chargepunchDown");
                FreezePositionCancel();
                attackState = AttackState.AttackDown;
                isDownAttack = true;
                break;
            case (int)PlayerAttackIndex.ChargeAttackUp:
                CharacterAnimation("chargepunchUp");
                attackState = AttackState.AttackUp;
                FreezePositionCancel();
                isUpAttack = true;
                break;
        }
    }


    /// <summary>
    /// ☆獲得時の獲得エフェクトを表示する為のコルーチン
    /// </summary>
    /// <returns></returns>
    private IEnumerator OnGetStar()
    {
        // Singleton.Instance.soundManager.StopPlayerSe();
        Singleton.Instance.soundManager.PlayPlayerSe(getStarSeNum);
        IsAcquisitionStar = false;
        GetStarEffectPlay(true);
        yield return new WaitForSeconds(1.5f);
        GetStarEffectPlay(false);
    }
    /// <summary>
    /// キャラクターのアニメーションです
    /// いろんなところでアニメーションをセットするとどこでいじったか
    /// 分からなくなると思うのでここにまとめました。
    /// </summary>
    /// <param name="animationName">使用するアニメーション名</param>
    public void CharacterAnimation(string animationName)
    {
        switch (animationName)
        {
            case "gameStart":
                GameStartAnimation();
                break;
            case "idol":// 待機状態
                Attack_Idol();
                break;
            case "dash":// 走る
                Attack_Dash();
                break;
            case "jump":// ジャンプ
                Attack_Jump();
                break;
            case "knockback":// ノックバック
                Attack_KnockBack();
                break;
            case "charge":// チャージ
                Attack_Charge();
                break;
            case "punch":// パンチ
                Attack_Punch();
                break;
            case "chargepunch":// チャージパンチ
                Attack_Chargepunch();
                break;
            case "chargepunchUp":// チャージアッパー
                Attack_ChargepunchUp();
                break;
            case "chargepunchDown":// チャージダウン
                Attack_ChargepunchDown();
                break;
            case "chargeSpecial":// スペシャル
                Charge5SpecialAnimation();
                break;
            case "GameOver":// ゲームオーバー
                GameOverAnimation();
                break;
            case "ExitAnimation":
                ExitAnimation();
                break;
            case "chargeCancel":
                ChargeCancelAnimation();
                break;
        }
    }
    // ---------- 各種アニメーションの設定---------------
    void GameStartAnimation()
    {
        animatorComponent.SetTrigger("isPlay");
    }
    void Attack_Idol()
    {
        animatorComponent.SetBool("isKnockBack", false);
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetBool("isJump", false);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void Attack_Dash()
    {
        animatorComponent.SetBool("isKnockBack", false);
        animatorComponent.SetBool("isDash", true);
        animatorComponent.SetBool("isJump", false);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void Attack_Jump()
    {
        animatorComponent.SetBool("isKnockBack", false);
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetBool("isJump", true);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void Attack_KnockBack()
    {
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetBool("isJump", false);
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetInteger("setPunchNum", 0);
        animatorComponent.SetBool("isKnockBack", true);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void Attack_Charge()
    {
        animatorComponent.SetBool("isKnockBack", false);
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetTrigger("isCharge");
        animatorComponent.SetBool("isCharge", true);
        animatorComponent.SetInteger("setPunchNum", 0);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void Attack_Punch()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetTrigger("isPunch");
        animatorComponent.SetInteger("setPunchNum", 1000);
        animatorComponent.SetInteger("ChargeNum", chargeCount);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void Attack_Chargepunch()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetTrigger("isPunch");
        animatorComponent.SetInteger("setPunchNum", 1010);
        animatorComponent.SetInteger("ChargeNum", chargeCount);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void Attack_ChargepunchUp()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetTrigger("isPunch");
        animatorComponent.SetInteger("setPunchNum", 1011);
        animatorComponent.SetInteger("ChargeNum", chargeCount);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void Attack_ChargepunchDown()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetTrigger("isPunch");
        animatorComponent.SetInteger("setPunchNum", 1001);
        animatorComponent.SetInteger("ChargeNum", chargeCount);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void GameOverAnimation()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetBool("isJump", false);
        animatorComponent.SetTrigger("isGameOver");
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void ExitAnimation()
    {
        animatorComponent.SetBool("ExitAnimation2", true);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void Charge5SpecialAnimation()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetTrigger("isPunch");
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetInteger("ChargeNum", chargeCount);
        animatorComponent.SetBool("ChargeCancel", false);
    }
    void ChargeCancelAnimation()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ChargeCancel", true);
    }
    /// <summary>
    /// 通常状態でのキャラクターの処理
    /// </summary>
    /// <param name="dx">横移動キー入力値</param>
    /// <param name="deltaTime">deltaTimeをGameSceneManagerからもらう</param>
    void NormalModeUpdate(float deltaTime)
    {
        // 移動
        float dx = Input.GetAxis("Horizontal");
        float dy = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump") && isGround)
        {
            CharacterAnimation("jump");
            rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }

        // アニメーション
        if (dx != 0 && !isChargeFlag && isGround)
        {
            CharacterAnimation("dash");
            Singleton.Instance.soundManager.PlayPlayerSe(dashSeNum);
            SandEffectPlay(true);
        }
        else if (!isChargeFlag && isGround)
        {
            CharacterAnimation("idol");
            Singleton.Instance.soundManager.StopPlayerSe();
            SandEffectPlay(false);
        }
        else if (!isGround)
        {
            CharacterAnimation("jump");
            SandEffectPlay(false);
        }
        if (dx < 0)
        {
            dx = -1.0f;
        }else if(dx > 0)
        {
            dx = 1.0f;
        }
        // 移動
        CharacterMove(dx, deltaTime);

        if (Input.GetKey(KeyCode.T) && !isChargeFlag || Input.GetButton("Charge") && !isChargeFlag)
        {
            canAttack = false;
            FreezePositionOll();
            isChargeFlag = true;
            // チャージSE再生
            Singleton.Instance.soundManager.StopPlayerSe();
            objState = ObjState.OnCharge;
        }

        if (newSystem)
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button3))
            {
                // これはPlay中に移動する
                NewAttack(dx, dy);
            }
        }
    }

    void NotAttackModeUpdate(float deltaTime)
    {
        // 移動
        float dx = Input.GetAxis("Horizontal");
        // アニメーション
        if (dx != 0 && !isChargeFlag && isGround)
        {
            CharacterAnimation("dash");
            Singleton.Instance.soundManager.PlayPlayerSe(dashSeNum);
            SandEffectPlay(true);
        }
        else if (!isChargeFlag && isGround)
        {
            CharacterAnimation("idol");
            Singleton.Instance.soundManager.StopPlayerSe();
            SandEffectPlay(false);
        }
        else if (!isGround)
        {
            CharacterAnimation("jump");
            SandEffectPlay(false);
        }
        // 移動
        CharacterMove(dx, deltaTime);
    }
    /// <summary>
    /// スタン状態時に実行します
    /// </summary>
    void StanUpdate()
    {
        if (isStun)
        {
            isStun = false;
            CharacterAnimation("knockback");
            FreezePositionCancel();
            ChargeEffectPlay(false, false);
            auraEfect.SetActive(false);
            // チャージゲージをリセットします
            Singleton.Instance.gameSceneController.StarChargeController.UpdateChargePoint(0);
            // チャージ中☆を戻します
            Singleton.Instance.gameSceneController.StarChargeController.UpdateBigStarUI(chargeCount);
            chargeCount = 0;
            chargeNow = 0.0f;
            var rig = rigidbody;
            if (isGround)
            {
                if (enemyPosition.x < transform.position.x)
                {
                    rig.AddForce(Vector3.right * stunAmountMovement, ForceMode.Impulse);
                }
                else
                {
                    rig.AddForce(Vector3.left * stunAmountMovement, ForceMode.Impulse);
                }
            }
            else
            {
                if (enemyPosition.x < transform.position.x)
                {
                    rig.AddForce(Vector3.right * stunAirAmountMovement, ForceMode.Impulse);
                }
                else
                {
                    rig.AddForce(Vector3.left * stunAirAmountMovement, ForceMode.Impulse);
                }
            }
            StartCoroutine(StunEnumerator(stunTime));
        }
    }
    IEnumerator StunEnumerator(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        canAttack = true;
        isChargeFlag = false;
        FreezePositionCancel();
        objState = ObjState.Normal;
    }
    /// <summary>
    /// キャラクターのチャージ時に呼び出します
    /// </summary>
    /// <param name="dx">横移動キー入力値</param>
    /// <param name="dy">上下移動キー入力値</param>
    void ChargeUpdate()
    {
        // 移動
        float dy = Input.GetAxis("Vertical");
        float dx = Input.GetAxis("Horizontal");
        if (oldSystem)
        {
            DirectionMove(dx);

            CharacterAnimation("charge");
            // 通常時
            // チャージ
            if (Input.GetKey(KeyCode.T) || Input.GetButton("Charge"))
            {
                Singleton.Instance.soundManager.PlayPlayerLoopSe(chargeSeNum);

                // チャージ中ジョイスティックが回転（動か）していた時の処理
                var inLoadKey = CheckLoadKey(dx, dy);

                var chargeUpGoodLuckValue = CheckAmountOfRotationPerSecond(inLoadKey);
                // チャージ中
                ChargeUp(starPointNormalization, inLoadKey, chargeUpGoodLuckValue);
                Singleton.Instance.gameSceneController.StarChargeController.ChargeBigStar(chargeCount);
                ChargeAttackHand(Singleton.Instance.gameSceneController.ChargePointManager.StarChildCount);
                // チャージエフェクト
                if (chargeCount < 3)
                {
                    ChargeEffectPlay(true, false);
                }
                else
                {
                    ChargeEffectPlay(false, true);
                    auraEfect.SetActive(true);
                    Debug.Log("chargeCount : " + chargeCount);
                    ChargeEfectScaleChange(chargeCount);
                }
                starChargeController.ChargeStarUIAnimationInt(chargeCount);
                var chargeStarMax = Singleton.Instance.gameSceneController.ChargePointManager.StarChildCount / 10;
                if (chargeCount == chargeStarMax)
                {
                    starChargeController.ChargeStarUIAnimationBool(true);
                }
            }
            // 解放（攻撃実行）
            if (Input.GetKeyUp(KeyCode.T) || Input.GetButtonUp("Charge"))
            {
                if (chargeCount < 3)
                {
                    // チャージ終了（チャージゲージを0に戻す）
                    AttackPower = chargeCount * fastOffensivePower + foundationoffensivePower;
                }
                else
                {
                    // チャージ終了（チャージゲージを0に戻す）
                    AttackPower = chargeCount * secondOffensivePower + foundationoffensivePower;
                }
                // チャージゲージをリセットします
                Singleton.Instance.gameSceneController.StarChargeController.UpdateChargePoint(0);
                // チャージ中☆を戻します
                Singleton.Instance.gameSceneController.StarChargeController.UpdateBigStarUI(chargeCount);
                // 攻撃アニメーション
                // チャージ回数が1回までなら通常パンチ
                // チャージしたなら入力角度を計算して上下左右を判断して攻撃
                if (chargeCount <= 1)
                {
                    OnAttackMotion(1000);
                    Singleton.Instance.soundManager.StopPlayerSe();
                    Singleton.Instance.soundManager.PlayPlayerSe(punchSeNum);

                }
                else if (chargeCount >= 5)
                {
                    // ここにSpecialついか
                    Charge5SpecialAnimation();
                    specialAttackNum = attack.OnAttack(new Vector2(dx, dy), this.gameObject);
                    ChargeReset(false, true);
                    objState = ObjState.ChargeSpecial;
                    return;
                    // Singleton.Instance.soundManager.StopPlayerSe();
                    // Singleton.Instance.soundManager.PlayPlayerSe(punchSeNum);
                }
                else
                {
                    OnAttackMotion(attack.OnAttack(new Vector2(dx, dy), this.gameObject));
                    Singleton.Instance.soundManager.StopPlayerSe();
                    Singleton.Instance.soundManager.PlayPlayerSe(chargeAttackSeNum);
                }
                ChargeReset(false, false);
                auraEfect.SetActive(false);
                objState = ObjState.Attack;
            }
        }
        //新しいシステム
        if (newSystem)
        {
            CharacterAnimation("charge");
            // 通常時
            // チャージ
            if (Input.GetKey(KeyCode.T) || Input.GetButton("Charge"))
            {
                Singleton.Instance.soundManager.PlayPlayerLoopSe(chargeSeNum);

                // チャージ中ジョイスティックが回転（動か）していた時の処理
                var inLoadKey = CheckLoadKey(dx, dy);

                var chargeUpGoodLuckValue = CheckAmountOfRotationPerSecond(inLoadKey);
                // チャージ中
                ChargeUp(starPointNormalization, inLoadKey, chargeUpGoodLuckValue);
                Singleton.Instance.gameSceneController.StarChargeController.ChargeBigStar(chargeCount);
                ChargeAttackHand(Singleton.Instance.gameSceneController.ChargePointManager.StarChildCount);
                // チャージエフェクト
                if (chargeCount < 3)
                {
                    ChargeEffectPlay(true, false);
                    ChargeEfectScaleChange(chargeCount);
                }
                else
                {
                    ChargeEffectPlay(false, true);
                }
                starChargeController.ChargeStarUIAnimationInt(chargeCount);
                var chargeStarMax = Singleton.Instance.gameSceneController.ChargePointManager.StarChildCount / 10;
                if (chargeCount == chargeStarMax)
                {
                    starChargeController.ChargeStarUIAnimationBool(true);
                }
            }
            // 解放（攻撃実行）
            if (Input.GetKeyUp(KeyCode.T) || Input.GetButtonUp("Charge"))
            {
                NewResetCharge();
            }
        }
        // 攻撃をキャンセルする際の設定
        if (attackCancel)
        {
            attackCancel = false;
            ChargeEffectPlay(false, false);
            auraEfect.SetActive(false);
            chargeNow = 0.0f;
            isAttack = false;
            canDamage = false;
            chargeNowHand = 0.0f;
            FreezePositionCancel();
            PunchEffectPlay(false);
            isUpAttack = false;
            isDownAttack = false;
            chargeCount = 0;
            CharacterAnimation("ExitAnimation");
            canAttack = true;
            IsHit = false;
            if (isGround) objState = ObjState.Normal;
            else objState = ObjState.NotAttackMode;
        }
    }
    void NewAttack(float dx,float dy)
    {
        FreezePositionOll();
        if (chargeCount < 3)
        {
            // チャージ終了（チャージゲージを0に戻す）
            AttackPower = chargeCount * fastOffensivePower + foundationoffensivePower;
        }
        else
        {
            // チャージ終了（チャージゲージを0に戻す）
            AttackPower = chargeCount * secondOffensivePower + foundationoffensivePower;
        }
        // チャージゲージをリセットします
        Singleton.Instance.gameSceneController.StarChargeController.UpdateChargePoint(0);
        // チャージ中☆を戻します
        Singleton.Instance.gameSceneController.StarChargeController.UpdateBigStarUI(chargeCount);
        // 攻撃アニメーション
        // チャージ回数が1回までなら通常パンチ
        // チャージしたなら入力角度を計算して上下左右を判断して攻撃
        if (chargeCount <= 1)
        {
            OnAttackMotion(1000);
            Singleton.Instance.soundManager.StopPlayerSe();
            Singleton.Instance.soundManager.PlayPlayerSe(punchSeNum);

        }
        else if (chargeCount >= 5)
        {
            // ここにSpecialついか
            Charge5SpecialAnimation();
            specialAttackNum = attack.OnAttack(new Vector2(dx, dy), this.gameObject);
            ChargeReset(false, true);
            objState = ObjState.ChargeSpecial;
            return;
            // Singleton.Instance.soundManager.StopPlayerSe();
            // Singleton.Instance.soundManager.PlayPlayerSe(punchSeNum);
        }
        else
        {
            OnAttackMotion(attack.OnAttack(new Vector2(dx, dy), this.gameObject));
            Singleton.Instance.soundManager.StopPlayerSe();
            Singleton.Instance.soundManager.PlayPlayerSe(chargeAttackSeNum);
        }
        ChargeReset(false, false);
        objState = ObjState.Attack;
    }
    void NewResetCharge()
    {
        FreezePositionCancel();
        CharacterAnimation("chargeCancel");
        isChargeFlag = false;
        if (isGround) objState = ObjState.Normal;
        else objState = ObjState.NotAttackMode;
    }
    void ChargeReset(bool chargeEfect1, bool chargeEfect2)
    {
        // チャージ時の☆アニメーションを戻す
        // starChargeController.ChargeStarUIAnimationInt(0);
        starChargeController.ChargeStarUIAnimationBool(false);
        ChargeEffectPlay(chargeEfect1, chargeEfect2);
        auraEfect.SetActive(false);
        PunchEffectPlay(true);
        chargeNow = 0.0f;
        canDamage = true;
        isAttack = true;
    }
    /// <summary>
    /// チャージポイントをUpする
    /// </summary>
    /// <param name="ChargeAmountOfIncrease"></param>
    void ChargeUp(float ChargeAmountOfIncrease, bool checkChargeUp, int chargeUpGoodLuckValue)
    {
        Singleton.Instance.gameSceneController.StarChargeController.UpdateChargePoint(OnCharge(Singleton.Instance.gameSceneController.ChargePointManager.StarChildCount / ChargeAmountOfIncrease, checkChargeUp, chargeUpGoodLuckValue));
    }
    /// <summary>
    /// 攻撃時のキャラクター更新
    /// </summary>
    /// <param name="animationTime">アニメーション時間</param>
    void AttackUpdate()
    {
        var animationTime = 0.0f;
        if (isAttack)
        {
            isAttack = false;
            switch (attackState)
            {
                case AttackState.AttackJab:
                    animationTime = attackJabTime;
                    break;
                case AttackState.ChargeAttack:
                    animationTime = chargeAttackTime;
                    break;
                case AttackState.AttackUp:
                    animationTime = chargeAttackUpTime;
                    break;
                case AttackState.AttackDown:
                    animationTime = chargeAttackDownTime;
                    break;
            }
            StartCoroutine(OnAttack(animationTime));
            MoveAttack();
        }
        if (enemyBreak && chargeCount == 5)
        {
            enemyBreak = false;
            MoveAttack();
        }
    }
    // 攻撃時の移動
    /// <summary>
    /// チャージ攻撃時のキャラクターの移動
    /// 上下左右の動きを管理しています
    /// </summary>
    /// <param name="speedForce">チャージ攻撃時の移動量</param>
    void MoveAttack(bool onceAgainAttack = false)
    {
        var rig = rigidbody;
        rig.drag = dragPower;
        if (!onceAgainAttack)
        {
            AttackSpeed = (chargeCount * speedForce);
        }
        else
        {
            AttackSpeed = (chargeCount * (speedForce / 2));
        }
        if (!isUpAttack && !isDownAttack)
        {
            // 右向きの時
            if (isRightDirection && !isLeftDirection)
            {
                rig.AddForce(Vector3.right * AttackSpeed, ForceMode.Impulse);
            }
            // 左向きの時
            else
            {
                rig.AddForce(Vector3.left * AttackSpeed, ForceMode.Impulse);
            }
        }
        else if (isUpAttack)
        {
            AttackSpeed = (chargeCount * speedForceUp);
            rig.AddForce(Vector3.up * AttackSpeed, ForceMode.Impulse);
        }
        else if (isDownAttack)
        {
            rig.AddForce(Vector3.down * AttackSpeed, ForceMode.Impulse);
        }
        isChargeFlag = false;
    }
    // アタック時
    public IEnumerator OnAttack(float animationTime)
    {
        yield return new WaitForSeconds(animationTime);
        ChargeEffectPlay(false, false);
        auraEfect.SetActive(false);
        ReSetSpecal();
        canDamage = false;
        chargeNowHand = 0.0f;
        FreezePositionCancel();
        PunchEffectPlay(false);
        isUpAttack = false;
        isDownAttack = false;
        chargeCount = 0;
        specialAttackNum = 0;
        CharacterAnimation("ExitAnimation");
        canAttack = true;
        IsHit = false;
        if (isGround) objState = ObjState.Normal;
        else objState = ObjState.NotAttackMode;
    }
    /// <summary>
    /// ゲームオーバー時の処理
    /// </summary>
    void CharacterGameOver()
    {
        StartCoroutine(GameOverIEnumerator());
    }

    IEnumerator GameOverIEnumerator()
    {
        ChargeEffectPlay(false, false);
        auraEfect.SetActive(false);
        CharacterAnimation("GameOver");

        yield return new WaitForSeconds(1.5f);
        Singleton.Instance.gameSceneController.isGameOver = true;
    }
    /// <summary>
    /// 攻撃時キャラクターを停止する
    /// </summary>
    void FreezeChargeAttack()
    {
        rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    }
    /// <summary>
    /// 初期状態に戻します
    /// </summary>
    void FreezePositionCancel()
    {
        rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }
    /// <summary>
    /// チャージ時動かないようにする
    /// チャージ攻撃横移動のみこれ使う
    /// 落下しなくなる
    /// </summary>
    void FreezePositionOll()
    {
        rigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
    }
    /// <summary>
    /// 砂煙エフェクトの表示非表示
    /// </summary>
    /// <param name="isPlay">表示非表示</param>
    void SandEffectPlay(bool isPlay)
    {
        sandEffect.SetActive(isPlay);
    }
    /// <summary>
    /// ☆獲得エフェクトの表示非表示
    /// </summary>
    /// <param name="isPlay">表示非表示</param>
    void GetStarEffectPlay(bool isPlay)
    {
        starAcquisitionEffect.SetActive(isPlay);
    }
    /// <summary>
    /// パンチエフェクトの表示非表示
    /// </summary>
    /// <param name="isPlay">表示非表示</param>
    void PunchEffectPlay(bool isPlay)
    {
        punchEffect.SetActive(isPlay);
    }
    /// <summary>
    /// チャージ時のエフェクト表示非表示
    /// チャージエフェクトは2種類あってどちらか使用中の時はどちらか使用しないのでまとめました。
    /// 2種類のエフェクトだが使い方は1通りです。
    /// </summary>
    /// <param name="effect1_isPlay">1段階目のチャージエフェクト表示非表示</param>
    /// <param name="effect2_isPlay">2段階目のチャージエフェクト表示非表示</param>
    void ChargeEffectPlay(bool effect1_isPlay, bool effect2_isPlay)
    {
        chargeEffect1.SetActive(effect1_isPlay);
        chargeEffect2.SetActive(effect2_isPlay);
    }
    // チャージ5スペシャルのUpdate
    void ChargeSpecialUpdate()
    {
        if (!checkSpecial)
        {
            IsSpecialProduction = true;
            checkSpecial = true;
            StartCoroutine(SpecialUpdateEnumerator());
        }
    }
    /// <summary>
    /// スペシャル演出のコルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator SpecialUpdateEnumerator()
    {
        yield return new WaitForSeconds(2.0f);
        IsSpecialProduction = false;
        SetSpecal();
        checkSpecial = false;
        OnAttackMotion(specialAttackNum);
        Singleton.Instance.soundManager.StopPlayerSe();
        Singleton.Instance.soundManager.PlayPlayerSe(chargeAttackSeNum);
        objState = ObjState.Attack;
    }
    /// <summary>
    /// チャージ2エフェクトを拳にセットします
    /// </summary>
    void SetSpecal()
    {
        chargeEffect2.transform.parent = playerFistObj.transform;
        chargeEffect2.transform.position = Vector3.zero;
    }
    /// <summary>
    /// チャージ2エフェクトを体に戻します
    /// </summary>
    void ReSetSpecal()
    {
        chargeEffect2.transform.parent = seijoukityanObj.transform;
        chargeEffect2.transform.position = Vector3.zero;
        chargeEffect2.transform.rotation = Quaternion.Euler(-90, 0, 0);
    }
    /// <summary>
    /// ジョイスティックが回転（動か）されていいるか確認
    /// チャージ時チャージを早くするのに使用します
    /// </summary>
    /// <param name="joystickX"></param>
    /// <param name="joystickY"></param>
    /// <returns></returns>
    int oldDegree = 0;// チェック済みの角度を格納
    public bool CheckLoadKey(float joystickX, float joystickY)
    {
        bool xInKey = false;
        bool yInKey = false;
        // ジョイスティックの入力判定
        if (joystickX != 0)
            xInKey = true;
        if (joystickY != 0)
            yInKey = true;
        // 角度の計算
        int degree = 0;// チェック用に返り値を取得
        if (xInKey || yInKey)
        {
            degree = CheckDegree(new Vector2(joystickX, joystickY));
        }
        //確認
        if (oldDegree != degree)
        {
            oldDegree = degree;
            return true;
        }
        return false;
    }
    /// <summary>
    /// 上下左右のどのコマンドが入ったかを返します
    /// </summary>
    /// <param name="direction">入力情報</param>
    /// <returns></returns>
    private int CheckDegree(Vector2 direction)
    {
        // 単位ベクトル計算
        Vector2 firing = direction.normalized;
        // ラジアン
        float radian = Mathf.Atan2(firing.y, firing.x);
        // 角度
        float degree = radian * Mathf.Rad2Deg;
        if (firing.x == 0 && firing.y == 0)
        {
            return 0;
        }
        else
        {
            // 右
            if (degree < 30 && degree > -30)
            {
                return 1;
            }
            // 左
            else if (degree > 150 && degree <= 180 || degree < -150 && degree >= -180)
            {
                return 2;
            }
            // 上
            else if (degree > 30 && degree < 150)
            {
                return 3;
            }
            // 下
            else if (degree < -30 && degree > -150)
            {
                return 4;
            }
        }
        return -1;
    }
    //973
    //ジョイスティックの回転量
    private int CheckAmountOfRotationPerSecond(bool checkLoadKey)
    {
        checkTime += Time.deltaTime;
        if (checkTime >= 1.0f)
        {
            checkTime = 0.0f;
            inKeyCount = 0;
        }
        if (checkLoadKey)
        {
            inKeyCount++;
        }
        return inKeyCount;
    }
    // ゲームクリア
    public void GameClear()
    {
        Attack_Idol();
    }
    // チャージオーラのサイズ変更
    private const float charge_3_EffectSize = 0.75f;
    private const float charge_4_EffectSize = 1.0f;
    private const float charge_5_EffectSize = 2.0f;
    [SerializeField] private GameObject auraEfect = null;
    private void ChargeEfectScaleChange(int chargeCount)
    {
        var targetScale = auraEfect.transform.lossyScale;
        var efectScaleSize = 0.0f;
        switch (chargeCount)
        {
            case 3:
                Debug.Log("111");
                efectScaleSize = charge_3_EffectSize;
                break;
            case 4:
                Debug.Log("222");
                efectScaleSize = charge_4_EffectSize;
                break;
            case 5:
                Debug.Log("333");
                efectScaleSize = charge_5_EffectSize;
                break;
        }
        targetScale = new Vector3( efectScaleSize, efectScaleSize, efectScaleSize);
        auraEfect.transform.localScale = targetScale;
    }
}
