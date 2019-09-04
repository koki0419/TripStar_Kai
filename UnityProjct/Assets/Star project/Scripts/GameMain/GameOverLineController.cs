using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverLineController : MonoBehaviour
{
    /// <summary>
    /// オブジェクトのステータス
    /// </summary>
    public enum GameOverLineState
    {
        None,
        Sealed,// 封印状態
        Awakening,// 覚醒状態
    }
    public GameOverLineState gameOverLineState = GameOverLineState.None;
    // アニメーター取得
    private Animator gameOverLineAnimator;
    [SerializeField] private int awakeningSeNum;
    // カメラからどの距離にいたのかで元のポジションに戻ります
    private Vector3 returnPosition;//= new Vector3(-11.0f, 0.0f, 37.0f);
    // 各ステージ攻撃してくるポジション2か所
    [SerializeField] private Vector3 fastAttackPosition = Vector3.zero;
    public Vector3 FastAttackPosition
    {
        get { return fastAttackPosition; }
    }
    [SerializeField] private Vector3 secondAttackPosition = Vector3.zero;
    public Vector3 SecondAttackPosition
    {
        get { return secondAttackPosition; }
    }
    // 目スポットライト
    [SerializeField] private GameObject spotLight = null;
    // 攻撃エリアオブジェクトを取得
    [SerializeField] private GameObject attackRange_1_2 = null;
    [SerializeField] private GameObject attackRange_1_4 = null;
    private SoundManager soundManager = null;
    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        gameOverLineAnimator = GetComponent<Animator>();
        gameOverLineState = GameOverLineState.Sealed;
        SpotLightView(false);
        ViewObj(attackRange_1_2, false);
        ViewObj(attackRange_1_4, false);
        soundManager = Singleton.Instance.soundManager;

    }
    /// <summary>
    /// スタート時（起動）アニメーションを再生
    /// </summary>
    public void GameOverLineAnimation()
    {
        switch (gameOverLineState)
        {
            case GameOverLineState.Sealed:
                break;
            case GameOverLineState.Awakening:
                gameOverLineAnimator.SetTrigger("OnAwakening");
                break;
        }
    }
    /// <summary>
    /// Seを再生します
    /// </summary>
    public void PlayMoaiAwakeningSE()
    {
        soundManager.StopPlayerSe();
        soundManager.PlayPlayerSe(awakeningSeNum);
    }
    /// <summary>
    /// 移動update
    /// カメラの移動速度と同じです
    /// </summary>
    float moveSpeed = 2.0f;
    public void MoveUpdate(float deltaTime, bool isMove)
    {
        if (isMove)
        {
            var position = transform.position;
            position.x += moveSpeed * deltaTime;
            transform.position = position;
        }
        else
            return;
    }
    /// <summary>
    /// 攻撃update
    /// </summary>
    private float attackSpeed = 10.0f;
    private bool inReturnPos = false;
    private bool isSetattackRangeArea = false;
    //攻撃
    public bool BigMoaiAttack(float attackProgressRange, int num)
    {
        if (!inReturnPos)
        {
            returnPosition = transform.position;
            inReturnPos = true;
        }
        if (num == 0)
        {
            if (!isSetattackRangeArea)
            {
                isSetattackRangeArea = true;
                SetattackRangeArea(attackRange_1_4, 4.0f);
            }
            ViewObj(attackRange_1_4, true);
        }
        else
        {
            if (!isSetattackRangeArea)
            {
                isSetattackRangeArea = true;
                SetattackRangeArea(attackRange_1_2, 5.5f);
            }
            ViewObj(attackRange_1_2, true);
        }
        //スクリーン座標の何割進行するか
        var position = transform.localPosition;
        if (returnPosition.x + attackProgressRange >= position.x)
        {
            position.x += attackSpeed * Time.deltaTime;
            transform.localPosition = position;

            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 攻撃後戻る
    /// </summary>
    public bool ReturnPosipoin()
    {
        var position = transform.position;
        if (position.x >= returnPosition.x)
        {
            position.x -= attackSpeed * Time.deltaTime;
            transform.position = position;

            return true;
        }
        else
        {
            position.x = returnPosition.x;
            inReturnPos = false;
            SpotLightView(false);
            ViewObj(attackRange_1_2, false);
            ViewObj(attackRange_1_4, false);
            isSetattackRangeArea = false;
            return false;
        }
    }
    public IEnumerator SpotLightOnOff()
    {
        SpotLightView(true);
        yield return new WaitForSeconds(1.0f);
        SpotLightView(false);
        yield return new WaitForSeconds(1.0f);
        SpotLightView(true);
    }
    /// <summary>
    /// スポットライトの表示非表示
    /// </summary>
    /// <param name="isView">表示非表示</param>
    private void SpotLightView(bool isView)
    {
        spotLight.SetActive(isView);
    }
    /// <summary>
    /// 取得したオブジェクトの表示非表示
    /// </summary>
    /// <param name="viewObj">対象オブジェクト</param>
    /// <param name="isView">表示非表示</param>
    public void ViewObj(GameObject viewObj, bool isView)
    {
        viewObj.SetActive(isView);
    }
    /// <summary>
    /// 攻撃範囲表示オブジェクトを座標設定します
    /// </summary>
    /// <param name="attackRangeObj">攻撃範囲表示オブジェクト</param>
    /// <param name="postionX">正規化座標</param>
    void SetattackRangeArea(GameObject attackRangeObj,float postionX)
    {
        var pos = attackRangeObj.transform.position;
        pos.x = transform.position.x + postionX;
        pos.y = 0.1f;
        pos.z = 0.0f;
        attackRangeObj.transform.position = pos;
    }
}
