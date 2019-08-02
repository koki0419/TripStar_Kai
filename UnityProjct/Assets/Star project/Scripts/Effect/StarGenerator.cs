using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransformObjectPool))]
public class StarGenerator : MonoBehaviour
{
    // プレイヤーオブジェクト
    [SerializeField] private GameObject playerObj = null;
    // プレイヤースクリプト
    [SerializeField] private PlayerMove playerMove = null;
    [Header("☆プール生成数")]
    [SerializeField] private int spawnMax;
    [Header("画面内に最大何個☆を表示するか")]
    [SerializeField] private int starDysplayCount;
    [Header("☆プレハブ")]
    [SerializeField] private GameObject starPrefab;
    private ObjectPool pool;
    // ☆現在の表示数
    [HideInInspector]
    public int ActiveCount
    {
        set; get;
    }
    // ☆生成数（経過）→次生成する☆のインデックス
    private int spawnIndex = 0;
    // 星データを取得します（ScriptableObject）
    [SerializeField] private StarData starData = null;
    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        pool = GetComponent<ObjectPool>();
        pool.CreatePool(starPrefab, spawnMax);
        CreatStar();
    }
    /// <summary>
    /// 対象オブジェクトを生成します
    /// </summary>
    public void CreatStar()
    {
        if (spawnIndex < starData.starDatas.Count)
        {
            while (ActiveCount < starDysplayCount)
            {
                var star = pool.GetObject();
                if (star != null)
                {
                    //プレイヤーの位置座標をスクリーン座標に変換
                    var setData = starData.starDatas[spawnIndex];
                    star.transform.localPosition = setData.star_Position;
                    var newStarObj = star.GetComponent<StarController>();
                    newStarObj.SetStarDatas(this, playerMove,"SpecifiedSpawn", setData.star_Point);
                    newStarObj.Init();
                    spawnIndex++;
                    ActiveCount++;
                }
            }
        }
        else return;
    }

    public void StarSponUpdate()
    {
        CreatStar();
    }
    /// <summary>
    /// 障害物を壊した際に☆生成時に使用します
    /// </summary>
    /// <param name="targetPos">☆生成するときのポジション</param>
    /// <param name="sponIndex">☆生成数</param>
    public void ObstaclesToStarSpon(Vector3 targetPos, int sponIndex)
    {
        var spon = 0;
        var randPoint = Random.Range(1, 3);
        while (true)
        {
            var star = pool.GetObject();
            if (star == null)
            {
                break;
            }
            var randX = Random.Range(-1, 1);
            var randY = Random.Range(1, 2);
            star.transform.localPosition = new Vector3(targetPos.x + randX, targetPos.y + randY + targetPos.z);
            var newStarObj = star.GetComponent<StarController>();
            newStarObj.SetStarDatas(this, playerMove, "ObstacleSpawn", randPoint);
            newStarObj.Init();
            spon++;
            if (spon >= sponIndex) break;
        }
    }

}
