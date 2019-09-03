using UnityEngine;

[RequireComponent(typeof(ObjectPool))]
public class StarSpawn : MonoBehaviour
{
    [SerializeField] private RectTransform target = null;
    [Header("☆プール生成数")]
    [SerializeField] private int spawnMax;
    [Header("☆プレハブ")]
    [SerializeField] private GameObject starEffectPrefab;
    private ObjectPool pool;
    [SerializeField] private Camera mainCamera = null;
    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        pool = GetComponent<ObjectPool>();
        pool.CreatePool(starEffectPrefab, spawnMax);
    }
    /// <summary>
    /// 対象オブジェクトを生成します
    /// </summary>
    /// <param name="sponPos"></param>
    public void CreatStarEffect(Vector3 sponPos)
    {
        //sponPosはワールド座標で取得するのでスクリーン座標に変換
        var screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, sponPos);
        var starEffect = pool.GetObject();
        if (starEffect != null)
        {
            starEffect.GetComponent<RectTransform>().position = screenPos;
            starEffect.GetComponent<StarEffect>().Init(this.target);
        }
    }
}
