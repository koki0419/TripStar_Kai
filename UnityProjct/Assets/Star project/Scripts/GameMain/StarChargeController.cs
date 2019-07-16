using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarChargeController : MonoBehaviour
{
    //大きい☆使用状態
    public enum Star
    {
        None = 0, //☆取得していないとき
        Normal = 1010,//☆獲得状態
        Chage = 1100,//チャージ中（大きい☆用）
    }
    public Star star = Star.None;
    [Header("チャージ用☆UI")]
    //大きい☆UIを取得します →5個
    [SerializeField] private StarState[] starChargeUI = null;
    //☆獲得ポイント
    [SerializeField] private Image chargeFill = null;
    //小さい☆UIが10個溜まったフラグ
    private bool starChargeMaxFlag = false;
    //現在の大きい☆の数
    [SerializeField] private int bigStarCount = 0;
    public int StarCount
    {
        set { bigStarCount = value; }
        get { return bigStarCount; }
    }
    //小さい☆獲得表示UI(1/10)
    [SerializeField] private GameObject AcquisitionSpriteStarCount0 = null;
    //小さい☆獲得表示UI(10/10)
    [SerializeField] private GameObject AcquisitionSpriteStarCount1 = null;
    //小さい☆の獲得数画像
    [SerializeField] private Sprite[] smallStarAcquisitionSprite = null;
    //小さい☆獲得UI1/10のアニメーション
    [SerializeField] private Animator AcquisitionStarCount_1_10Animator = null;
    //小さい☆獲得UI10/10のアニメーション
    [SerializeField] private Animator AcquisitionStarCount_10_10Animator = null;
    [SerializeField] private Animator chargeStarUIAnimator = null;

    public void Init()
    {
        starChargeMaxFlag = false;
        bigStarCount = 0;
        for (int i = 0; i < starChargeUI.Length; i++)
        {
            starChargeUI[i].UpdateStarSprite((int)Star.None);
        }
        //小さい☆獲得UIの画像を0に設定します（1/10、10/10両方とも）
        AcquisitionSpriteStarCount0.GetComponent<Image>().sprite = smallStarAcquisitionSprite[0];
        AcquisitionSpriteStarCount1.GetComponent<Image>().sprite = smallStarAcquisitionSprite[0];
    }

    //大きい☆UIの更新
    public void UpdateBigStarUI(int starNum)
    {
        for (int i = 0; i < starNum; i++)
        {
            starChargeUI[i].UpdateStarSprite((int)Star.Normal);
        }
    }

    //チャージ時
    public void ChargeBigStar(int starNum)
    {
        for (int i = 0; i < starNum; i++)
        {
            starChargeUI[i].UpdateStarSprite((int)Star.Chage);
        }
    }

    //チャージポイントのupdete
    public void UpdateChargePoint(float percentage)
    {
        chargeFill.fillAmount = percentage;
    }
    /// <summary>
    /// 小さい☆の獲得状況を表示します
    /// ☆に合わせて0～9の画像を切り替えて表示します
    /// </summary>
    /// <param name="smollSratCount">小さい☆獲得数</param>
    public void UpdateDisplayAcquisitionSpriteStar(int smollSratCount)
    {
        if (smollSratCount < 10)
        {
            //1/10の桁
            var starCount0 = smollSratCount % 10;
            AcquisitionSpriteStarCount0.GetComponent<Image>().sprite = smallStarAcquisitionSprite[starCount0];
            AcquisitionSpriteStarCount1.GetComponent<Image>().sprite = smallStarAcquisitionSprite[0];
        }
        else
        {
            //1/10の桁
            var starCount0 = smollSratCount % 10;
            //10/10の桁
            var starCount1 = smollSratCount / 10 % 10;
            AcquisitionSpriteStarCount0.GetComponent<Image>().sprite = smallStarAcquisitionSprite[starCount0];
            AcquisitionSpriteStarCount1.GetComponent<Image>().sprite = smallStarAcquisitionSprite[starCount1];
        }
       if(smollSratCount % 10 == 0)
        {
            AcquisitionStarCount_1_10Animator.SetTrigger("isUpdate");
            AcquisitionStarCount_10_10Animator.SetTrigger("isUpdate");
        }
        else
        {
            AcquisitionStarCount_1_10Animator.SetTrigger("isUpdate");
        }
        UpdateBigStarUI(smollSratCount / 10);
    }
    /// <summary>
    /// チャージ時に星がたまった時のアニメーション
    /// </summary>
    /// <param name="chargeCount"></param>
    public void ChargeStarUIAnimationInt(int chargeCount)
    {
        chargeStarUIAnimator.SetInteger("ChargeStarCount", chargeCount);
    }
    public void ChargeStarUIAnimationBool(bool chargeCountMax)
    {
        chargeStarUIAnimator.SetBool("ChargeMax", chargeCountMax);
    }
}
