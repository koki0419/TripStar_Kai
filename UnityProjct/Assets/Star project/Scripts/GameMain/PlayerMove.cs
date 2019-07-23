using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    //攻撃時設定数値
    private enum PlayerAttackIndex
    {
        None,
        AttackNormal = 1000,
        ChargeAttackNormal = 1010,
        ChargeAttackDown = 1001,
        ChargeAttackUp = 1011,
    }
    private PlayerAttackIndex payerAttackIndex = PlayerAttackIndex.None;
    //オブジェクトステータス
    private enum ObjState
    {
        None,
        Normal,//通常状態
        Stun,//スタン状態
        NotAttackMode,//スタン状態
        Attack,//攻撃状態
        OnCharge,//チャージ中状態
        CharacterGameOver,//ゲームオーバー状態
    }
    private ObjState objState = ObjState.None;
    //オブジェクトステータス
    private enum AttackState
    {
        None,
        AttackJab,//ジャブ攻撃状態
        AttackUp,//上攻撃状態
        AttackDown,//下攻撃状態
        ChargeAttack,//チャージ攻撃状態
    }
    private AttackState attackState = AttackState.None;
    //-------------Unityコンポーネント関係-------------------
    [SerializeField] private StarChargeController starChargeController = null;
    // 自分のアニメーションコンポーネント
    [SerializeField] private Animator animatorComponent = null;
    public Animator PlayerAnimator
    {
        get { return animatorComponent; }
    }
    private new Rigidbody rigidbody;
    [Header("エフェクト関係")]
    //スター獲得エフェクト
    [SerializeField] private GameObject starAcquisitionEffect = null;
    //チャージエフェクト1
    [SerializeField] private GameObject chargeEffect1 = null;
    //チャージエフェクト2
    [SerializeField] private GameObject chargeEffect2 = null;
    //砂煙エフェクト
    [SerializeField] private GameObject sandEffect = null;
    //パンチエフェクト
    [SerializeField] private GameObject punchEffect = null;
    //-------------クラス関係--------------------------------
    //『Attack』をインスタンス
    Attack attack = new Attack();
    //-------------数値用変数--------------------------------
    [Header("プレイヤー情報")]
    //移動速度を設定します
    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float knockbackMoveSpeedMax = 0;
    private float knockbackMoveSpeed = 0;
    //ジャンプ中の移動速度
    [SerializeField] private float airUpMoveSpeed = 0;
    [SerializeField] private float airDownMoveSpeed = 0;
    //空気抵抗
    [SerializeField] private float dragPower = 0;
    //キー入力制御
    [SerializeField] private float inputMoveKey = 0;
    //ジャンプ力
    [SerializeField] private float jumpPower = 0;
    //チャージポイント使用時のユーザーゲージ上昇量
    [SerializeField] private float userChargePonitUp;
    [Header("プレイヤー攻撃初期情報")]
    //初期攻撃力
    private const float foundationoffensivePower = 1000000;

    [Header("チャージ回数に掛け算される力")]
    //攻撃力
    private const float fastOffensivePower = 700000;
    private const float secondOffensivePower = 800000;
    //移動量
    private const float speedForce = 300;
    private const float speedForceUp = 350;
    //現在のチャージ量
    private float chargeNow = 0.0f;
    //現在のチャージ量
    private float chargeNowHand = 0.0f;
    //何回チャージしたか
    private int chargeCount = 0;
    //回転
    private float rot = 90;
    private float nowAttackSpeed = 0;
    //攻撃時Speed
    public float AttackSpeed
    {
        get; private set;
    }
    //攻撃時パワー
    public float AttackPower
    {
        get; private set;
    }
    [Header("追加攻撃するときの残スピード量")]
    [SerializeField] private float addAttackCheck = 0;
    [Header("スタン時ステータス")]
    //スタン状態からの復活時間
    [SerializeField] private float stunTime;
    //スタン時の移動量
    [SerializeField] private float stunAmountMovement;
    [SerializeField] private float stunAirAmountMovement;
    private bool positiveDirection;
    private Vector3 enemyPosition;

    [Header("攻撃時の攻撃時間")]
    [SerializeField] private  float attackJabTime;
    [SerializeField] private  float chargeAttackTime;
    [SerializeField] private  float chargeAttackUpTime;
    [SerializeField] private  float chargeAttackDownTime;

    [Header("キャラクターSEの種類番号")]
    [SerializeField] private int dashSeNum;
    [SerializeField] private int jumpSeNum;
    [SerializeField] private int chargeSeNum;
    [SerializeField] private int chargeAttackSeNum;
    [SerializeField] private int punchSeNum;
    [SerializeField] private int getStarSeNum;
    //-------------フラグ用変数------------------------------
    //アタックフラグ//ダメージをあたえられる
    public bool canDamage
    {
        get; private set;
    }
    //地面との接触
    private bool isGround;
    public bool IsGround
    {
        set { isGround = value; }
    }

    //チャージ中かどうか
    private bool isChargeFlag;
    //☆獲得時フラグ
    public bool IsAcquisitionStar
    {
        set; get;
    }
    //キャラクターの向き
    private bool isRightDirection;//右を向いている
    private bool isLeftDirection;//左を向いている
    private bool isUpAttack;//上攻撃
    private bool isDownAttack;//下攻撃
    private bool isAttack;//通常攻撃
    private bool canAttack;//攻撃できるか
    private bool isStun;//スタン状態か
    private bool notKey;//キー入力制御
    private bool rightNotKey;//右キー入力制御
    private bool leftNotKey;//左キー入力制御
    public bool IsHit//攻撃を当てることが出来る
    {
        get; private set;
    }
    public bool enemyBreak//エネミーが破壊されたかどうか
    {
        set; get;
    }
    //各種レイヤーを設定
    private const string groundLayerName = "Ground";//地面
    private const string gameOverLineLayerName = "GameOverObj";//ゲームオーバーライン
    private const string screenOutLineLayerName = "SceneRestrictionBar";//スクリーン右端制御用
    private const string enemyLayerName = "Obstacles";//エネミー
    private const string enemyHeadLayerName = "ObstaclesHead";//エネミーの頭
    //壁ずり対策
    private const string rightProgressionControlLayerName = "RightProgressionControlObject";//右入力規制用
    private const string leftProgressionControlLayerName = "LeftProgressionControlObject";//左入力規制用
    //初期化
    public void Init()
    {
        //プレイヤーの状態を通常状態に設定します
        objState = ObjState.Normal;
        //右向きに指定
        transform.rotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
        //Rigidbodyを取得します
        rigidbody = GetComponent<Rigidbody>();
        dragPower = rigidbody.drag;
        //チャージゲージをリセットします
        Singleton.Instance.gameSceneController.StarChargeController.UpdateChargePoint(0);
        //----初期化-----
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
            case ObjState.CharacterGameOver:
                CharacterGameOver();
                break;
        }
        //☆獲得時のエフェクト発生
        if (IsAcquisitionStar)
        {
            StartCoroutine(OnGetStar());
        }
    }
    //--------------関数-----------------------------
    /// <summary>
    /// スタンアタックを食らったときにキャラクターがエネミーに接触状態だと「OnCollisionEnter」だけだと
    /// 反応しないので「OnCollisionStay」も使用します
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay(Collision collision)
    {
        //エネミーとの当たり判定
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
        //ゲームオーバーの当たり判定
        if (LayerMask.LayerToName(collision.gameObject.layer) == gameOverLineLayerName)
        {
            objState = ObjState.CharacterGameOver;
        }
        //エネミーとの当たり判定
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
                //壁ずり抑制
                StartCoroutine(KnockBackIEnumerator());
            }
        }
        else if (LayerMask.LayerToName(collision.gameObject.layer) == screenOutLineLayerName)
        {
            //壁ずり抑制
            StartCoroutine(KnockBackIEnumerator());
        }
    }
    //プレイヤーと対象が接触中の判定
    private void OnTriggerStay(Collider other)
    {
        //地面とエネミー頭は同じ判定→ただし、エネミーの場合はダメージが入るのでレイヤー分け
        if (CheckHitOtherGround(other))
        {
            Debug.Log("頭111]");
            isGround = true;
        }
        else if (LayerMask.LayerToName(other.gameObject.layer) == rightProgressionControlLayerName)
        {
            //右壁ずり
            rightNotKey = true;
        }
        else if (LayerMask.LayerToName(other.gameObject.layer) == leftProgressionControlLayerName)
        {
            //左壁ずり
            leftNotKey = true;
        }
    }
    //プレイヤーと対象が接触したときの判定
    private void OnTriggerEnter(Collider other)
    {
        //地面とエネミー頭接触時の判定
        if (CheckHitOtherGround(other))
        {
            Debug.Log("頭");
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
    //プレイヤーが対象との接触しなくなった時
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
                    //ジャンプ音再生
                    Singleton.Instance.soundManager.PlayPlayerSe(jumpSeNum);
                }
            }
        }//LRどちらかのフラグが入っているとき
        else if (rightNotKey)
        {
            //右進行不能フラグから出たとき
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
                        //ジャンプ音再生
                        Singleton.Instance.soundManager.PlayPlayerSe(jumpSeNum);
                    }
                }
            }
        }
        else if (leftNotKey)
        {
            //右進行不能フラグから出たとき
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
                        //ジャンプ音再生
                        Singleton.Instance.soundManager.PlayPlayerSe(jumpSeNum);
                    }
                }
            }
        }
    }
    private IEnumerator KnockBackIEnumerator()
    {
        notKey = true;
        knockbackMoveSpeed = knockbackMoveSpeedMax;
        yield return new WaitForSeconds(0.5f);
        knockbackMoveSpeed = 0;
        if (isGround) notKey = false;
    }
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
        //移動
        var velocity = rigidbody.velocity;
        velocity.x = horizontal * moveSpeed;
        //地面に接触していない時
        if (!isGround)
        {
            velocity.x = horizontal * airUpMoveSpeed;
            // 下降中
            //ジャンプの上りと降りでスピードが異なるため
            //dragを下げている
            if (velocity.y < 0)
            {
                rigidbody.drag = 0;
                velocity.x = horizontal * airDownMoveSpeed;
            }
        }
        //knockbackを受けたらキー入力を禁止する
        if (notKey)
        {
            //キャラクターの向きに合わせて
            //x軸にknockbackさせる
            if (horizontal > 0)
            {
                velocity.x = -knockbackMoveSpeed;
            }
            else if (horizontal < 0)
            {
                velocity.x = knockbackMoveSpeed;
            }
        }
        //左右入力規制が入った時
        //入力を禁止する
        if (rightNotKey && horizontal > 0)
        {
            velocity.x = 0;
        }
        else if (leftNotKey && horizontal < 0)
        {
            velocity.x = 0;
        }
        rigidbody.velocity = velocity;//velocityの更新
        //キャラクターの向きを修正します
        //入力キーの方向に向きを変えます
        if (horizontal > 0)
        {
            //右向き
            transform.localRotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
            isRightDirection = true;
            isLeftDirection = false;
        }
        else if (horizontal < 0)
        {
            //左向き
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
        //キャラクターの向きを修正します
        //入力キーの方向に向きを変えます
        if (horizontal > 0)
        {
            //右向き
            transform.rotation = Quaternion.AngleAxis(rot, new Vector3(0, 1, 0));
            isRightDirection = true;
            isLeftDirection = false;
        }
        else if (horizontal < 0)
        {
            //左向き
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
        //チャージ量の+-量
        float chargeProportion = userChargePonitUp * 10;

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
    float OnCharge(float charge)
    {
        //マックスのチャージ量
        var chargeMax = charge;
        //チャージ量の+-量
        float chargeProportion = userChargePonitUp;
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
            chargeNow += chargeProportion;
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
        //Singleton.Instance.soundManager.StopPlayerSe();
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
            case "idol"://待機状態
                Attack_Idol();
                break;
            case "dash"://走る
                Attack_Dash();
                break;
            case "jump"://ジャンプ
                Attack_Jump();
                break;
            case "knockback"://ノックバック
                Attack_KnockBack();
                break;
            case "charge"://チャージ
                Attack_Charge();
                break;
            case "punch"://パンチ
                Attack_Punch();
                break;
            case "chargepunch"://チャージパンチ
                Attack_Chargepunch();
                break;
            case "chargepunchUp"://チャージアッパー
                Attack_ChargepunchUp();
                break;
            case "chargepunchDown"://チャージダウン
                Attack_ChargepunchDown();
                break;
            case "GameOver"://ゲームオーバー
                GameOverAnimation();
                break;
            case "ExitAnimation":
                ExitAnimation();
                break;
        }
    }
    void GameStartAnimation()
    {
        animatorComponent.SetTrigger("isPlay");
    }
    void Attack_Idol()
    {
        animatorComponent.SetBool("isKnockBack", false);
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetBool("isJump", false);
    }
    void Attack_Dash()
    {
        animatorComponent.SetBool("isKnockBack", false);
        animatorComponent.SetBool("isDash", true);
        animatorComponent.SetBool("isJump", false);
    }
    void Attack_Jump()
    {
        animatorComponent.SetBool("isKnockBack", false);
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetBool("isJump", true);
    }
    void Attack_KnockBack()
    {
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetBool("isJump", false);
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetInteger("setPunchNum", 0);
        animatorComponent.SetBool("isKnockBack", true);
    }
    void Attack_Charge()
    {
        animatorComponent.SetBool("isKnockBack", false);
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetTrigger("isCharge");
        animatorComponent.SetBool("isCharge", true);
        animatorComponent.SetInteger("setPunchNum", 0);
    }
    void Attack_Punch()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetTrigger("isPunch");
        animatorComponent.SetInteger("setPunchNum", 1000);
    }
    void Attack_Chargepunch()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetTrigger("isPunch");
        animatorComponent.SetInteger("setPunchNum", 1010);
    }
    void Attack_ChargepunchUp()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetTrigger("isPunch");
        animatorComponent.SetInteger("setPunchNum", 1011);
    }
    void Attack_ChargepunchDown()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetTrigger("isPunch");
        animatorComponent.SetInteger("setPunchNum", 1001);
    }
    void GameOverAnimation()
    {
        animatorComponent.SetBool("isCharge", false);
        animatorComponent.SetBool("ExitAnimation2", false);
        animatorComponent.SetBool("isDash", false);
        animatorComponent.SetBool("isJump", false);
        animatorComponent.SetTrigger("isGameOver");
    }
    void ExitAnimation()
    {
        animatorComponent.SetBool("ExitAnimation2", true);
    }
    /// <summary>
    /// 通常状態でのキャラクターの処理
    /// </summary>
    /// <param name="dx">横移動キー入力値</param>
    /// <param name="deltaTime">deltaTimeをGameSceneManagerからもらう</param>
    void NormalModeUpdate(float deltaTime)
    {
        //移動
        float dx = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && isGround)
        {
            CharacterAnimation("jump");
            rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }

        //アニメーション
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

        //移動
        CharacterMove(dx, deltaTime);

        if (Input.GetKey(KeyCode.T) && canAttack || Input.GetButton("Charge") && canAttack)
        {
            canAttack = false;
            FreezePositionOll();
            isChargeFlag = true;
            //チャージSE再生
            Singleton.Instance.soundManager.StopPlayerSe();
            objState = ObjState.OnCharge;
        }
    }

    void NotAttackModeUpdate(float deltaTime)
    {
        //移動
        float dx = Input.GetAxis("Horizontal");

        //アニメーション
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
        //移動
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
            //チャージゲージをリセットします
            Singleton.Instance.gameSceneController.StarChargeController.UpdateChargePoint(0);
            //チャージ中☆を戻します
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
        float dy = Input.GetAxis("Vertical");
        //移動
        float dx = Input.GetAxis("Horizontal");
        DirectionMove(dx);
        CharacterAnimation("charge");
        //通常時
        //チャージ
        if (Input.GetKey(KeyCode.T) || Input.GetButton("Charge"))
        {
            Singleton.Instance.soundManager.PlayPlayerLoopSe(chargeSeNum);
            //チャージ中
            Singleton.Instance.gameSceneController.StarChargeController.UpdateChargePoint(OnCharge(Singleton.Instance.gameSceneController.ChargePointManager.StarChildCount / 10));
            Singleton.Instance.gameSceneController.StarChargeController.ChargeBigStar(chargeCount);
            ChargeAttackHand(Singleton.Instance.gameSceneController.ChargePointManager.StarChildCount);
            //チャージエフェクト
            if (chargeCount < 3)
            {
                ChargeEffectPlay(true, false);
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
        if (Input.GetKeyUp(KeyCode.T) || Input.GetButtonUp("Charge"))
        {
            if (chargeCount < 3)
            {
                //チャージ終了（チャージゲージを0に戻す）
                AttackPower = chargeCount * fastOffensivePower + foundationoffensivePower;
            }
            else
            {
                //チャージ終了（チャージゲージを0に戻す）
                AttackPower = chargeCount * secondOffensivePower + foundationoffensivePower;
            }
            //チャージゲージをリセットします
            Singleton.Instance.gameSceneController.StarChargeController.UpdateChargePoint(0);
            //チャージ中☆を戻します
            Singleton.Instance.gameSceneController.StarChargeController.UpdateBigStarUI(chargeCount);
            //攻撃アニメーション
            //チャージ回数が1回までなら通常パンチ
            //チャージしたなら入力角度を計算して上下左右を判断して攻撃
            if (chargeCount <= 1)
            {
                OnAttackMotion(1000);
                Singleton.Instance.soundManager.StopPlayerSe();
                Singleton.Instance.soundManager.PlayPlayerSe(punchSeNum);

            }
            else
            {
                OnAttackMotion(attack.OnAttack(new Vector2(dx, dy), this.gameObject));
                Singleton.Instance.soundManager.StopPlayerSe();
                Singleton.Instance.soundManager.PlayPlayerSe(chargeAttackSeNum);
            }
            //チャージ時の☆アニメーションを戻す
            //starChargeController.ChargeStarUIAnimationInt(0);
            starChargeController.ChargeStarUIAnimationBool(false);

            ChargeEffectPlay(false, false);
            PunchEffectPlay(true);
            chargeNow = 0.0f;
            canDamage = true;
            isAttack = true;
            objState = ObjState.Attack;
        }
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
    //攻撃時の移動
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
            //右向きの時
            if (isRightDirection && !isLeftDirection)
            {
                rig.AddForce(Vector3.right * AttackSpeed, ForceMode.Impulse);
            }
            //左向きの時
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
    //アタック時
    public IEnumerator OnAttack(float animationTime)
    {
        yield return new WaitForSeconds(animationTime);
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
    void CharacterGameOver()
    {
        StartCoroutine(GameOverIEnumerator());
    }

    IEnumerator GameOverIEnumerator()
    {
        ChargeEffectPlay(false, false);
        CharacterAnimation("GameOver");

        yield return new WaitForSeconds(1.5f);
        Singleton.Instance.gameSceneController.isGameOver = true;
    }
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
}
