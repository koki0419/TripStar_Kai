using UnityEngine;

public class Boss : MonoBehaviour
{
    // -------------Unityコンポーネント関係-------------------
    [SerializeField] private Animator animator = null;
    [SerializeField] private GameObject sandEffect = null;
    // -------------クラス関係--------------------------------
    // 『PlayerMove』を取得します
    private PlayerMove playerMove;
    // -------------数値用変数--------------------------------
    // 生成する星の数
    [SerializeField] private int starNum;
    //ポイントを獲得した回数
    private int acquisitionPoint = 0;
    [SerializeField] private float deleteTime = 2.0f;
    // Hp
    [SerializeField] private float foundationHP;
    private float foundationHPMax;
    // -------------フラグ用変数------------------------------
    private bool onRemoveObjFlag = false;

    public void Init()
    {
        foundationHPMax = foundationHP;
        // 『PlayerMove』を取得します
        playerMove = Singleton.Instance.gameSceneController.PlayerMove;
        // オブジェクトを削除するかどうか
        onRemoveObjFlag = false;
        // ポイントを獲得した回数
        acquisitionPoint = 0;
        var hp = 1.0;
        hp -= (foundationHP / foundationHPMax);
        animator = gameObject.GetComponent<Animator>();
        SandEffectDysplay(false);
    }
    void Update()
    {
        // オブジェクトを消去します
        if (onRemoveObjFlag)
        {
            deleteTime -= Time.deltaTime;
            animator.SetTrigger("Break");
            //OnRemoveObj();
            SandEffectDysplay(true);
            if (deleteTime <= 0)
            {
                Destroy(this.gameObject);
            }
        }
        // 破壊されたらクリアフラグをTrueにする
        if (foundationHP <= 0)
        {
            Singleton.Instance.gameSceneController.isGameClear = true;
        }
    }

    // プレイヤーとの当たり判定
    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤーが「アタック状態」このボスが「1回も倒されていない」時
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player" && acquisitionPoint == 0 && playerMove.canDamage)
        {
            foundationHP -= OnDamage(playerMove.AttackPower, playerMove.AttackSpeed);
            // 新しく生成したオブジェクト
            Singleton.Instance.damageTextSpawn.CreatDamageEffect(transform.localPosition, (int)playerMove.AttackPower);
            var hp = 1.0;
            hp -= foundationHP / foundationHPMax;
            if (foundationHP <= 0)
            {
                acquisitionPoint++;
                onRemoveObjFlag = true;
            }
        }
    }

    // ダメージ量
    int OnDamage(float damage, float speed)
    {
        return (int)damage;
    }
    /// <summary>
    /// 砂煙エフェクトの表示非表示
    /// </summary>
    /// <param name="isDysplay">表示非表示</param>
    private void SandEffectDysplay(bool isDysplay)
    {
        sandEffect.SetActive(isDysplay);
    }
}
