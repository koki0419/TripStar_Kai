using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChargePointManager
{
    public int temporaryStorage
    {
        get; set;
    }
    // 小さい☆の獲得状況
    public int StarChildCount
    {
        get; set;
    }
    public float StarChildCountMax
    {
        get; private set;
    }
    // 小さい☆の獲得状況スキップ
    public int StarChildCountSkip
    {
        set; get;
    }

    // 一気に沢山の星を獲得したかどうか
    public bool IsSkipStar
    {
        set; get;
    }

    public void Init()
    {
        //チャージポイント
        temporaryStorage = 0;
        StarChildCount = 0;
        StarChildCountSkip = 0;
        StarChildCountMax = 50;
        IsSkipStar = false;
    }

    public void OnUpdate()
    {
        //一気に大量の☆を獲得したとき☆獲得数が現在の獲得数と足したときに最大獲得数を超えないか確認
        //越えなければ、現在の獲得数にプラスする
        if (StarChildCount < StarChildCountMax)
        {
            if(temporaryStorage != 0)
            {
                GetStarCountUp();
            }
        }
    }
    // 星獲得アニメーションカウントアップ用
    private float checkTime = 0.0f;
    private void GetStarCountUp()
    {
        var isMultipleAcquisition = Singleton.Instance.gameSceneController.isMultipleAcquisition;
        checkTime += Time.deltaTime;
        if (isMultipleAcquisition)
        {
            if (checkTime >= 0.15f)
            {
                StarChildCount++;
                temporaryStorage--;
                checkTime = 0.0f;
                Singleton.Instance.gameSceneController.StarChargeController.UpdateDisplayAcquisitionSpriteStar(StarChildCount);
                return;
            }
        }
        else
        {
            if (checkTime >= 0.5f)
            {
                StarChildCount++;
                temporaryStorage--;
                checkTime = 0.0f;
                Singleton.Instance.gameSceneController.StarChargeController.UpdateDisplayAcquisitionSpriteStar(StarChildCount);
                return;
            }
        }
    }
}
