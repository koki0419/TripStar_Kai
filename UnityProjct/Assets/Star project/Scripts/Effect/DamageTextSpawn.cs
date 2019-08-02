using UnityEngine;

/// <summary>
/// ダメージテクストを生成する
/// </summary>
[RequireComponent(typeof(ObjectPool))]
public class DamageTextSpawn : MonoBehaviour
{
    [Header("☆プール生成数")]
    [SerializeField] private int spawnMax;
    [Header("☆プレハブ")]
    // ダメージエフェクトのプレハブ
    [SerializeField] private GameObject damageEffectPrefab = null;
    // ダメージ表示ナンバー画像の配列
    [SerializeField] private Sprite[] scoreNumbreSprite = null;
    // メインカメラ
    [SerializeField] private Camera mainCamera = null;
    // オブジェクトプール
    private ObjectPool pool;
    /// <summary>
    /// 初期化
    /// </summary>
    /// 「ObjectPool」の設定と「damageEffectPrefab」を指定数生成する
    public void Init()
    {
        pool = GetComponent<ObjectPool>();
        pool.CreatePool(damageEffectPrefab, spawnMax);
    }
    /// <summary>
    /// CreatDamageEffectを外部から参照して生成する
    /// </summary>
    /// <param name="sponPos">ダメージを表示したいポジション</param>
    /// <param name="damage">出力するダメージ量</param>
    public void CreatDamageEffect(Vector3 sponPos, int damage)
    {
        // リザルトシーンに表示用の”総合ダメージ”にダメージ量をプラスする
        StarProject.Result.ResultScreenController.all_damage += damage;
        // sponPosはワールド座標で取得するのでスクリーン座標に変換
        var screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, sponPos);
        // ダメージエフェクトを「objectPool」から呼び出す
        var damageEffect = pool.GetObject();
        // ダメージエフェクトの初期化
        if (damageEffect != null)
        {
            damageEffect.GetComponent<DamageEffect>().Init();
            damageEffect.GetComponent<DamageEffect>().scoreNumbreSprite = new Sprite[10];
            for (int i = 0; i < scoreNumbreSprite.Length; i++)
            {
                damageEffect.GetComponent<DamageEffect>().scoreNumbreSprite[i] = this.scoreNumbreSprite[i];
            }
            damageEffect.GetComponent<DamageEffect>().SetDamage(damage);
            damageEffect.GetComponent<RectTransform>().position = screenPos;
        }
    }
}
