using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarState : MonoBehaviour
{
    // 大きい☆使用状態
    public enum Star
    {
        None = 0, // ☆取得していないとき
        Normal = 1010,// ☆獲得状態
        Chage = 1100,// チャージ中（大きい☆用）
    }
    public Star star = Star.None;

    [Header("☆の状態画像")]
    [SerializeField] private Sprite notAcquiredSprite = null;   // 使っていない
    [SerializeField] private Sprite normalSprite = null;        // 使っていない
    [SerializeField] private Sprite chageStarSprite = null;     // チャージ中（大きい☆用）
    [SerializeField] private GameObject starImage = null;
    [SerializeField] private bool isBigStatUI;

    public void UpdateStarSprite(int starState)
    {
        switch (starState)
        {
            case (int)Star.None:
                if (isBigStatUI)
                {
                    starImage.SetActive(false);
                }
                else starImage.GetComponent<Image>().sprite = notAcquiredSprite;
                break;
            case (int)Star.Normal:
                starImage.GetComponent<Image>().sprite = normalSprite;
                starImage.SetActive(true);
                break;
            case (int)Star.Chage:
                starImage.GetComponent<Image>().sprite = chageStarSprite;
                break;
        }
    }
}
