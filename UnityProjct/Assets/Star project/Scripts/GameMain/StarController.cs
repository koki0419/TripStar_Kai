using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{
    // ---------Unityコンポーネント宣言--------------
    GameObject playerObj;
    [SerializeField] private Material goldStarMaterial = null;  // 黄金マテリアル   ☆獲得ポイント3
    [SerializeField] private Material silverStarMaterial = null;// 銀色マテリアル   ☆獲得ポイント2
    [SerializeField] private Material bronzeStarMaterial = null;// 銅色マテリアル   ☆獲得ポイント1

    [SerializeField] private Renderer starRenderer = null;
    // ------------クラスの宣言----------------------
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private StarGenerator starGenerator;
    // ------------数値変数の宣言--------------------
    [SerializeField] private int starPoint;
    [SerializeField] private int goldStarPoint;
    [SerializeField] private int silverStarPoint;
    [SerializeField] private int bronzeStarPoint;
    // ------------フラグ変数の宣言------------------
    private enum StarSponType
    {
        None,
        ObstacleSpawn,  // モアイから生成
        SpecifiedSpawn, // スクリプトから生成
    }
    private StarSponType starSponType = StarSponType.None;
    private const string gameOverLineLayerName = "GameOverObj";

    /// <summary>
    /// データをセットします
    /// </summary>
    /// <param name="starGenerator">starGenerator</param>
    /// <param name="playermove">playermove</param>
    /// <param name="type">文字列でタイプ指定</param>
    /// <param name="point">ポイント</param>
    public void SetStarDatas(StarGenerator starGenerator, PlayerMove playermove, string type, int point)
    {
        this.starGenerator = starGenerator;
        playerMove = playermove;
        starPoint = point;
        if (type == "ObstacleSpawn")
        {
            starSponType = StarSponType.ObstacleSpawn;
        }
        else
        {
            starSponType = StarSponType.SpecifiedSpawn;
        }
    }
    public void Init()
    {
        if (starPoint == goldStarPoint)// 黄金の☆マテリアル
        {
            starRenderer.material = goldStarMaterial;
        }
        else if (starPoint == silverStarPoint)// 銀色の☆マテリアル
        {
            starRenderer.material = silverStarMaterial;
        }
        else if (starPoint == bronzeStarPoint)// 銅色の☆マテリアル
        {
            starRenderer.material = bronzeStarMaterial;
        }
    }

    private void Update()
    {
        CheckDeleteStar();
    }
    /// <summary>
    /// 星の座標が（カメラ座標-10）になったら☆のSetActiveをfalseにする
    /// </summary>
    void CheckDeleteStar()
    {
        if (Camera.main != null)
        {
            if (transform.localPosition.x < (Camera.main.transform.position.x - 10.0f))
            {
                switch (starSponType)
                {
                    case StarSponType.ObstacleSpawn:
                        gameObject.SetActive(false);
                        break;
                    case StarSponType.SpecifiedSpawn:
                        starGenerator.ActiveCount--;
                        gameObject.SetActive(false);
                        break;
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (CheckPlayerHit(other))
        {
            Singleton.Instance.starSpawn.CreatStarEffect(transform.localPosition);

            playerMove.IsAcquisitionStar = true;
            // 複数個の時とアニメーション変える
            Singleton.Instance.gameSceneController.ChargePointManager.temporaryStorage += starPoint;
            var isMultipleAcquisition = false;
            if(starPoint > 1)
            {
                isMultipleAcquisition = true;
            }
            Singleton.Instance.gameSceneController.isMultipleAcquisition = isMultipleAcquisition;
            // 「SpecifiedSpawn」タイプのみ「activeCount--」を行っている
            switch (starSponType)
            {
                case StarSponType.ObstacleSpawn:
                    gameObject.SetActive(false);
                    break;
                case StarSponType.SpecifiedSpawn:
                    starGenerator.ActiveCount--;
                    gameObject.SetActive(false);
                    break;
                default:
                    gameObject.SetActive(false);
                    break;
            }
        }
    }
    /// <summary>
    /// 対象と接触したかどうか
    /// </summary>
    /// <param name="other">対象</param>
    /// <returns></returns>
    private bool CheckPlayerHit(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Player")
            return true;
        else return false;
    }
}
