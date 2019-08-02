using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ダメージエフェクトの処理
/// </summary>
public class DamageEffect : MonoBehaviour
{
    // Damage用にテキストオブジェ
    [SerializeField]private GameObject[] damageText = null;
    // スコア表示画像
    public Sprite[] scoreNumbreSprite = new Sprite[10];
    // 最大スコア
    private const int MaxScore = 99999999;
    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        // ダメージ表示を全て非表示する
        ScoreUIDysplay(damageText.Length, false);
    }
    /// <summary>
    /// ダメージを設定してダメージ桁分セットアクティブをtrueにする
    /// </summary>
    /// <param name="damage"></param>
    public void SetDamage(int damage)
    {
        // テキストを設定する
        if (damage > MaxScore) damage = MaxScore;
        // 1の桁
        var score1 = damage % 10;
        // 10の桁
        var score10 = damage / 10 % 10;
        // 100の桁
        var score100 = damage / 100 % 10;
        // 1000の桁
        var score1000 = damage / 1000 % 10;
        // 10000の桁
        var score10000 = damage / 10000 % 10;
        // 100000の桁
        var score100000 = damage / 100000 % 10;
        // 1000000の桁
        var score1000000 = damage / 1000000 % 10;
        // 10000000の桁
        var score10000000 = damage / 10000000 % 10;
        damageText[0].GetComponent<Image>().sprite = scoreNumbreSprite[score1];
        damageText[1].GetComponent<Image>().sprite = scoreNumbreSprite[score10];
        damageText[2].GetComponent<Image>().sprite = scoreNumbreSprite[score100];
        damageText[3].GetComponent<Image>().sprite = scoreNumbreSprite[score1000];
        damageText[4].GetComponent<Image>().sprite = scoreNumbreSprite[score10000];
        damageText[5].GetComponent<Image>().sprite = scoreNumbreSprite[score100000];
        damageText[6].GetComponent<Image>().sprite = scoreNumbreSprite[score1000000];
        damageText[7].GetComponent<Image>().sprite = scoreNumbreSprite[score10000000];
        if (damage < 1000) ScoreUIDysplay(3, true);
        else if (damage < 10000) ScoreUIDysplay(4, true);
        else if (damage < 100000) ScoreUIDysplay(5, true);
        else if (damage < 1000000) ScoreUIDysplay(6, true);
        else if (damage < 10000000) ScoreUIDysplay(7, true);
        else ScoreUIDysplay(damageText.Length, true);
    }

    /// <summary>
    /// スコアによって最大桁数以上の数値UIを非表示にするために使用します
    /// </summary>
    /// <param name="dysPlayNum"></param>
    /// <param name="isDysPlay"></param>
    private void ScoreUIDysplay(int dysPlayNum, bool isDysPlay)
    {
        for (int i = 0; i < dysPlayNum; i++)
        {
            damageText[i].SetActive(isDysPlay);
        }
    }
    // ダメージ表示時間
    float lifeTime = 2.0f;
    [SerializeField] private Animator damageEffectAnimator;
    private void Update()
    {
        // アニメーターがplay中かどうか
        AnimatorStateInfo animInfo = damageEffectAnimator.GetCurrentAnimatorStateInfo(0);
        if (animInfo.normalizedTime < 1.0f)
        {
            // 再生中の処理
        }
        else
        {
            // 終了したときの処理
            gameObject.SetActive(false);
        }
            lifeTime -= Time.deltaTime;
        // if (lifeTime <= 0) gameObject.SetActive(false);
    }
}
