using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverLineController : MonoBehaviour
{

    public enum GameOverLineState
    {
        None,
        Sealed,//封印状態
        Awakening,//覚醒状態
    }
    public GameOverLineState gameOverLineState = GameOverLineState.None;
    private Animator gameOverLineAnimator;
    [SerializeField] private int awakeningSeNum;
    //カメラからどの距離にいたのかで元のポジションに戻ります
    private Vector3 returnPosition;//= new Vector3(-11.0f, 0.0f, 37.0f);
    //各ステージ攻撃してくるポジション2か所
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
    [SerializeField] private GameObject spotLight = null;
    [SerializeField] private GameObject attackRange_1_2 = null;
    [SerializeField] private GameObject attackRange_1_4 = null;

    public void Init()
    {
        gameOverLineAnimator = GetComponent<Animator>();
        gameOverLineState = GameOverLineState.Sealed;
        SpotLightView(false);
        ViewObj(attackRange_1_2, false);
        ViewObj(attackRange_1_4, false);

    }
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
    public void PlayMoaiAwakeningSE()
    {
        Singleton.Instance.soundManager.StopPlayerSe();
        Singleton.Instance.soundManager.PlayPlayerSe(awakeningSeNum);
    }

    float moveSpeed = 1.5f;
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

    private float attackSpeed = 3.0f;
    private bool inReturnPos = false;
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
            ViewObj(attackRange_1_4, true);
        }
        else
        {
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
    //攻撃後戻る
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
    private void SpotLightView(bool isView)
    {
        spotLight.SetActive(isView);
    }
    public void ViewObj(GameObject viewObj, bool isView)
    {
        viewObj.SetActive(isView);
    }
}
