using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{

    //---------Unityコンポーネント宣言--------------
    GameObject playerObj;
    [SerializeField] private Material goldStarMaterial;//黄金マテリアル   ☆獲得ポイント3
    [SerializeField] private Material silverStarMaterial;//銀色マテリアル   ☆獲得ポイント2
    [SerializeField] private Material bronzeStarMaterial;//銅色マテリアル   ☆獲得ポイント1

    [SerializeField] private Renderer starRenderer;
    //------------クラスの宣言----------------------
    PlayerMove playerMove;
    public StarGenerator starGenerator;
    //------------数値変数の宣言--------------------
    [SerializeField] private int starPoint;
    [SerializeField] private int goldStarPoint;
    [SerializeField] private int silverStarPoint;
    [SerializeField] private int bronzeStarPoint;
    //------------フラグ変数の宣言------------------
    public enum StarSponType
    {
        None,
        ObstacleSpon,//モアイから生成
        SpecifiedSpon,//スクリプトから生成
    }
    public StarSponType starSponType = StarSponType.None;
    private const string gameOverLineLayerName = "GameOverObj";

    // Start is called before the first frame update
    public void Init(PlayerMove playermove, int point)
    {
        playerMove = playermove;
        starPoint = point;

        if(starPoint == goldStarPoint)//黄金の☆マテリアル
        {
            starRenderer.material = goldStarMaterial;
        }else if(starPoint == silverStarPoint)//銀色の☆マテリアル
        {
            starRenderer.material = silverStarMaterial;
        }else if(starPoint == bronzeStarPoint)//銅色の☆マテリアル
        {
            starRenderer.material = bronzeStarMaterial;
        }
    }

    private void Update()
    {
        switch (starSponType)
        {
            case StarSponType.ObstacleSpon:
                if (transform.localPosition.x < (Camera.main.transform.position.x - 10.0f))
                {
                    gameObject.SetActive(false);
                }
                break;
            case StarSponType.SpecifiedSpon:
                if (transform.localPosition.x < (Camera.main.transform.position.x - 10.0f))
                {
                    starGenerator.activeCount--;
                    gameObject.SetActive(false);
                }
                break;
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Player")
        {
            Singleton.Instance.gameSceneController.isGetStar = true;
            Singleton.Instance.starSpawn.CreatStarEffect(transform.localPosition);
            if (starPoint == 0 && starPoint == 1)
            {
                playerMove.isAcquisitionStar = true;
                Singleton.Instance.gameSceneController.ChargePointManager.starChildCount += starPoint;
            }
            else
            {
                playerMove.isAcquisitionStar = true;
                Singleton.Instance.gameSceneController.ChargePointManager.starChildCountSkip += starPoint;
                Singleton.Instance.gameSceneController.ChargePointManager.isSkipStar = true;
            }

            switch (starSponType)
            {
                case StarSponType.ObstacleSpon:
                    gameObject.SetActive(false);
                    break;
                case StarSponType.SpecifiedSpon:
                    starGenerator.activeCount--;
                    gameObject.SetActive(false);
                    break;
            }
        }
    }
}
