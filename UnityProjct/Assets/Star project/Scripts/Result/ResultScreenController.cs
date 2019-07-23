using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StarProject.Gamemain;

namespace StarProject.Result
{
    public class ResultScreenController : MonoBehaviour
    {
        private enum ResultState
        {
            None,
            ResultAnimation,//リザルトアニメーション
            ResultSerect,//リザルトボタンセレクト
            GameEnd,//すべてのゲームが終了したとき
        }
        private ResultState resultState = ResultState.None;
        //ボタン選択のステータス
        private enum ResultRetryState
        {
            None,
            ResultSelect,//リザルト初期選択時
            Retry,//リトライダブルチェック選択時
            Exit,//終了ダブルチェック選択時
        }
        private ResultRetryState resultRetryState = ResultRetryState.None;
        [SerializeField] private Animator resultAnimator = null;
        //フェード関係
        [Header("フェード関係")]
        [SerializeField] private GameObject fadeImageObj = null;
        [SerializeField] private GameObject fadeText = null;
        [SerializeField] private GameObject fadeChara = null;
        private Image fadeImage = null;
        [SerializeField] private Color fadeOutColor;
        [SerializeField] private float fadeOutTime;
        //ネクストステージダイアログ
        [SerializeField] private GameObject nextStageDiaLog = null;
        [SerializeField] private Image nextStageButton = null;
        [SerializeField] private Image exitTitleButton = null;
        [SerializeField] private Image retryButton = null;
        [Header("次のステージ選択ダイアログ用画像")]
        [SerializeField] private Sprite nextStageNormalSprite = null;
        [SerializeField] private Sprite nextStageSelectSprite = null;
        [SerializeField] private Sprite exitTitleNormalSprite = null;
        [SerializeField] private Sprite exitTitleSelectSprite = null;
        [SerializeField] private Sprite retryNormalSprite = null;
        [SerializeField] private Sprite retrySelectSprite = null;
        //2重確認ダイアログ用画像
        [SerializeField] private GameObject exitDoubleCheckDialog = null;
        [SerializeField] private GameObject retryDoubleCheckDialog = null;
        [Header("2重確認用ボタン")]
        [SerializeField] private Image exitDoubleCheckDialogYesButton = null;
        [SerializeField] private Image exitDoubleCheckDialogNoButton = null;
        [SerializeField] private Image retryDoubleCheckDialogYesButton = null;
        [SerializeField] private Image retryDoubleCheckDialogNoButton = null;
        [Header("2重確認用画像")]
        [SerializeField] private Sprite doubleCheckDialogYesNormalSprite = null;
        [SerializeField] private Sprite doubleCheckDialogYesSelectSprite = null;
        [SerializeField] private Sprite doubleCheckDialogNoNormalSprite = null;
        [SerializeField] private Sprite doubleCheckDialogNoSelectSprite = null;
        //ボタン選択時にGetAxisを使用するので回数制限に使用します
        private int countNum = 0;
        //ボタン選択番号
        private int nextStageButtonSelectNum = 0;
        private int nextStageButtonSelectNumMax = 3;
        private int retryButtonSelectNum = 0;
        private int retryButtonSelectNumMax = 2;
        private int exitButtonSelectNum = 0;
        private int exitButtonSelectNumMax = 2;
        //選択ステージ番号を格納
        private int stageNum;
        private float resultAnimationTime;
        private float resultAnimationTimeMax = 10.0f;
        //総ダメージの表示
        static public int all_damage = 0;
        static public int allStar = 0;
        //ダメージ表記スコアUIを取得
        [SerializeField] private GameObject[] scoreObj = null;
        //ダメージ表記スコアUIを取得
        [SerializeField] private Image[] scoreUI = null;
        //ダメージ表示用の数値画像0～9
        [SerializeField] private Sprite[] numSprite = null;
        //ステージ数の表記UI
        [SerializeField] private Image stageNumUI = null;
        //ステージ数表示画像
        [SerializeField] private Sprite[] stageNumSprite = null;
        //クリアランク表示用UI
        [SerializeField] private Image rankUI = null;
        //クリアランク表示画像
        [SerializeField] Sprite[] rankSprite = null;
        private const int MaxScore = 99999999;
        [Header("ランク振り分けスコア")]
        [SerializeField] private int rankAScore;
        [SerializeField] private int rankBScore;
        [SerializeField] private int rankCScore;
        //全ステージ数
        private const int ollStageNum = 4;
        [SerializeField] private bool debug;
        [SerializeField] private int debugStage = 3;
        // Start is called before the first frame update
        IEnumerator Start()
        {
            fadeImage = fadeImageObj.GetComponent<Image>();
            FadeImageDysplay(false);
            ExitDiaLogDysplay(false);
            RetryDiaLogDysplay(false);
            nextStageButtonSelectNum = 0;
            NextStageButtonSelect(nextStageButtonSelectNum);
            NextStageDiaLogDisplay(false);
            ClearRankDisplay(all_damage);
            stageNum = GameSceneController.stageNum;
            StageNumDisplay(stageNum);
            ScoreUIDysplay(scoreObj.Length, false);
            yield return null;
            ResultScoreDisplay(all_damage);
            if (allStar / 10 != 0) resultAnimator.SetInteger("ResultStars", allStar / 10);
            yield return null;
            resultState = ResultState.ResultAnimation;
            resultRetryState = ResultRetryState.ResultSelect;
            if (debug)
            {
                GameSceneController.stageNum = debugStage;
            }
            if (SoundManager.audioVolume != 0) Singleton.Instance.soundManager.AudioVolume();
            else Singleton.Instance.soundManager.AllAudioVolume();
            Singleton.Instance.soundManager.PlayBgm("NormalBGM");
        }

        // Update is called once per frame
        void Update()
        {
            switch (resultState)
            {
                case ResultState.ResultAnimation:
                    ResultAnimation();
                    break;
                case ResultState.ResultSerect:
                    NextStageSelect();
                    break;
                case ResultState.GameEnd:
                    GameEndUpdate();
                    break;
            }
        }
        /// <summary>
        /// 評価演出
        /// </summary>
        void ResultAnimation()
        {
            AnimatorStateInfo animInfo = resultAnimator.GetCurrentAnimatorStateInfo(0);
            if (animInfo.normalizedTime < 1.0f)
            {
                //アニメーション早送り
                if (Input.GetKey(KeyCode.Return) || Input.GetButton("SelectOk"))
                {
                    Time.timeScale = 2.5f;
                }
                else
                {
                    Time.timeScale = 1.0f;
                }
            }
            else
            {   //アニメーション終了後クリックしてダイアログ表示
                if (Input.GetKey(KeyCode.Return) || Input.GetButtonDown("SelectOk"))
                {
                    Time.timeScale = 1.0f;
                    if (GameSceneController.stageNum != ollStageNum)
                    {
                        NextStageDiaLogDisplay(true);
                        resultState = ResultState.ResultSerect;
                    }
                    else
                    {
                        resultState = ResultState.GameEnd;
                    }
                }
            }
        }

        /// <summary>
        /// 次のステージに挑戦するかどうかを選択する関数
        /// </summary>
        void NextStageSelect()
        {
            float dx = Input.GetAxis("Horizontal");
            float dy = Input.GetAxis("Vertical");
            switch (resultRetryState)
            {
                case ResultRetryState.ResultSelect:
                    //左
                    if (dx < 0 && countNum == 0)
                    {
                        countNum++;
                        if (nextStageButtonSelectNum > 0) nextStageButtonSelectNum--;
                        NextStageButtonSelect(nextStageButtonSelectNum);
                    }//右
                    else if (dx > 0 && countNum == 0)
                    {
                        countNum++;
                        if (nextStageButtonSelectNum < nextStageButtonSelectNumMax) nextStageButtonSelectNum++;
                        NextStageButtonSelect(nextStageButtonSelectNum);
                    }
                    else if (dx == 0 && countNum != 0)
                    {
                        countNum = 0;
                    }

                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Joystick1Button0))
                    {
                        switch (nextStageButtonSelectNum)
                        {
                            case 0://次のステージ
                                GameSceneController.stageNum++;
                                stageNum = GameSceneController.stageNum;
                                SceneManager.LoadScene(string.Format("Main0{0}", stageNum));
                                resultRetryState = ResultRetryState.None;
                                break;
                            case 1://リトライ
                                retryButtonSelectNum = 0;
                                RetryButtonSelect(retryButtonSelectNum);
                                RetryDiaLogDysplay(true);
                                resultRetryState = ResultRetryState.Retry;
                                break;
                            case 2://やめる
                                exitButtonSelectNum = 0;
                                ExitButtonSelect(exitButtonSelectNum);
                                ExitDiaLogDysplay(true);
                                resultRetryState = ResultRetryState.Exit;
                                break;
                        }
                    }
                    break;
                case ResultRetryState.Retry:
                    RetrySelect(dx);
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("SelectOk"))
                    {
                        switch (retryButtonSelectNum)
                        {
                            case 0://いいえ
                                RetryDiaLogDysplay(false);
                                resultRetryState = ResultRetryState.ResultSelect;
                                break;
                            case 1://はい
                                SceneManager.LoadScene(string.Format("Main0{0}", stageNum));
                                resultRetryState = ResultRetryState.None;
                                break;
                        }
                    }
                    break;
                case ResultRetryState.Exit:
                    ExitSelect(dx);
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("SelectOk"))
                    {
                        switch (exitButtonSelectNum)
                        {
                            case 0://いいえ
                                ExitDiaLogDysplay(false);
                                resultRetryState = ResultRetryState.ResultSelect;
                                break;
                            case 1://はい
                                SceneManager.LoadScene("TitleScene");
                                resultRetryState = ResultRetryState.None;
                                break;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// クリア時のボタン選択
        /// 選択したボタンの色を変更します
        /// </summary>
        /// <param name="buttonSelectNum"><選択したボタンの番号/param>
        private void NextStageButtonSelect(int buttonSelectNum)
        {
            switch (buttonSelectNum)
            {
                case 0://次のステージ
                    nextStageButton.sprite = nextStageSelectSprite;
                    exitTitleButton.sprite = exitTitleNormalSprite;
                    retryButton.sprite = retryNormalSprite;
                    return;
                case 1://リトライ
                    nextStageButton.sprite = nextStageNormalSprite;
                    exitTitleButton.sprite = exitTitleNormalSprite;
                    retryButton.sprite = retrySelectSprite;
                    return;
                case 2://やめる
                    nextStageButton.sprite = nextStageNormalSprite;
                    exitTitleButton.sprite = exitTitleSelectSprite;
                    retryButton.sprite = retryNormalSprite;
                    break;
            }
        }
        /// <summary>
        /// リトライ選択時のYesNoボタンセレクト
        /// </summary>
        /// <param name="dx">左右選択</param>
        private void RetrySelect(float dx)
        {
            //右
            if (dx > 0 && countNum == 0)
            {
                countNum++;
                if (retryButtonSelectNum < retryButtonSelectNumMax) retryButtonSelectNum++;
                RetryButtonSelect(retryButtonSelectNum);
            }//左
            else if (dx < 0 && countNum == 0)
            {
                countNum++;
                if (retryButtonSelectNum > 0) retryButtonSelectNum--;
                RetryButtonSelect(retryButtonSelectNum);
            }
            else if (dx == 0 && countNum != 0)
            {
                countNum = 0;
            }
        }
        /// <summary>
        /// 終了選択時のYesNoボタンセレクト
        /// </summary>
        /// <param name="dx"></param>
        private void ExitSelect(float dx)
        {
            //右
            if (dx > 0 && countNum == 0)
            {
                countNum++;
                if (exitButtonSelectNum < exitButtonSelectNumMax) exitButtonSelectNum++;
                ExitButtonSelect(exitButtonSelectNum);
            }//左
            else if (dx < 0 && countNum == 0)
            {
                countNum++;
                if (exitButtonSelectNum > 0) exitButtonSelectNum--;
                ExitButtonSelect(exitButtonSelectNum);
            }
            else if (dx == 0 && countNum != 0)
            {
                countNum = 0;
            }
        }
        /// <summary>
        /// Exitを選択した後のYesNoボタン選択
        /// 選択したボタンの色を変更します
        /// </summary>
        /// <param name="buttonSelectNum">選択したボタンの番号</param>
        private void ExitButtonSelect(int buttonSelectNum)
        {
            switch (buttonSelectNum)
            {
                case 0:
                    exitDoubleCheckDialogYesButton.sprite = doubleCheckDialogYesNormalSprite;
                    exitDoubleCheckDialogNoButton.sprite = doubleCheckDialogNoSelectSprite;
                    return;
                case 1:
                    exitDoubleCheckDialogYesButton.sprite = doubleCheckDialogYesSelectSprite;
                    exitDoubleCheckDialogNoButton.sprite = doubleCheckDialogNoNormalSprite;
                    return;
            }
        }
        /// <summary>
        /// リトライ時YesNoのボタン選択
        /// 選択したボタンの色を変更します
        /// </summary>
        /// <param name="buttonSelectNum">選択したボタンの番号</param>
        private void RetryButtonSelect(int buttonSelectNum)
        {
            switch (buttonSelectNum)
            {
                case 0:
                    retryDoubleCheckDialogYesButton.sprite = doubleCheckDialogYesNormalSprite;
                    retryDoubleCheckDialogNoButton.sprite = doubleCheckDialogNoSelectSprite;
                    return;
                case 1:
                    retryDoubleCheckDialogYesButton.sprite = doubleCheckDialogYesSelectSprite;
                    retryDoubleCheckDialogNoButton.sprite = doubleCheckDialogNoNormalSprite;
                    return;
            }
        }
        /// <summary>
        /// 全てのステージがクリアしたときに呼び出されます
        /// </summary>
        private void GameEndUpdate()
        {
            StartCoroutine(GameEndEnumerator());
        }
        /// <summary>
        /// エンディングシーン遷移時にフェードを入れてから遷移
        /// </summary>
        /// <returns></returns>
        private IEnumerator GameEndEnumerator()
        {
            ForceColor(Color.clear);
            FadeImageDisplay(true);
            yield return FadeOutEnumerator(fadeOutColor, fadeOutTime);
            FadeImageDisplay(false);
            SceneManager.LoadScene("EndingScene");
        }
        /// <summary>
        /// フェードアウト
        /// </summary>
        /// <param name="color">最終カラー</param>
        /// <param name="period">フェード時間</param>
        /// <returns></returns>
        private IEnumerator FadeOutEnumerator(Color color, float period)
        {
            fadeImageObj.transform.SetAsLastSibling();
            yield return FadeEnumerator(Color.clear, color, period);
        }
        /// <summary>
        /// フェード実体
        /// </summary>
        /// <param name="startColor">初期カラー</param>
        /// <param name="targetColor">最終カラー</param>
        /// <param name="period">フェード時間</param>
        /// <returns></returns>
        private IEnumerator FadeEnumerator(Color startColor, Color targetColor, float period)
        {
            float t = 0;
            while (t < period)
            {

                t += Time.deltaTime;
                Color color = Color.Lerp(startColor, targetColor, t / period);
                fadeImage.color = color;
                yield return null;
            }
            fadeImage.color = targetColor;
        }
        /// <summary>
        /// フェードカラー初期化用
        /// </summary>
        /// <param name="color">初期化カラー</param>
        private void ForceColor(Color color)
        {
            FadeImageDysplay(true);
            fadeImageObj.transform.SetAsLastSibling();
            fadeImage.color = color;
        }
        /// <summary>
        /// フェード時フェードキャラクターを表示非表示します
        /// </summary>
        /// <param name="isFade">表示するかどうか</param>
        private void FadeImageDisplay(bool isFade)
        {
            fadeText.SetActive(isFade);
            fadeChara.SetActive(isFade);
        }
        /// <summary>
        /// フェードImageの表示非表示
        /// </summary>
        /// <param name="isDysplay">表示非表示</param>
        private void FadeImageDysplay(bool isDysplay)
        {
            fadeImageObj.SetActive(isDysplay);
        }
        /// <summary>
        /// 次のステージに挑戦するかダイアログを表示非表示
        /// </summary>
        /// <param name="isDisplay">表示非表示</param>
        private void NextStageDiaLogDisplay(bool isDisplay)
        {
            nextStageDiaLog.SetActive(isDisplay);
        }
        /// <summary>
        /// リトライボタンを押したときのYesNo確認ダイアログ
        /// </summary>
        /// <param name="isDysplay">表示非表示</param>
        private void RetryDiaLogDysplay(bool isDysplay)
        {
            retryDoubleCheckDialog.SetActive(isDysplay);
        }
        /// <summary>
        /// 終了ボタンを押したときのYesNo確認ダイアログ
        /// </summary>
        /// <param name="isDysplay">表示非表示</param>
        private void ExitDiaLogDysplay(bool isDysplay)
        {
            exitDoubleCheckDialog.SetActive(isDysplay);
        }
        /// <summary>
        /// クリアステージ数を表示します
        /// </summary>
        /// <param name="stageNum">クリアステージ数</param>
        private void StageNumDisplay(int stageNum)
        {
            if (debug) return;
            else stageNumUI.sprite = stageNumSprite[stageNum - 1];
        }
        /// <summary>
        /// ダメージを表示します
        /// 変数の作りすぎかもなので修正できるといいです
        /// </summary>
        /// <param name="ollDamage">総ダメージ値</param>
        private void ResultScoreDisplay(int allDamage)
        {
            var damage = allDamage;
            if (damage > MaxScore) damage = MaxScore;
            //1の桁
            var score1 = damage % 10;
            //10の桁
            var score10 = damage / 10 % 10;
            //100の桁
            var score100 = damage / 100 % 10;
            //1000の桁
            var score1000 = damage / 1000 % 10;
            //10000の桁
            var score10000 = damage / 10000 % 10;
            //100000の桁
            var score100000 = damage / 100000 % 10;
            //1000000の桁
            var score1000000 = damage / 1000000 % 10;
            //10000000の桁
            var score10000000 = damage / 10000000 % 10;
            scoreUI[0].sprite = numSprite[score1];
            scoreUI[1].sprite = numSprite[score10];
            scoreUI[2].sprite = numSprite[score100];
            scoreUI[3].sprite = numSprite[score1000];
            scoreUI[4].sprite = numSprite[score10000];
            scoreUI[5].sprite = numSprite[score100000];
            scoreUI[6].sprite = numSprite[score1000000];
            scoreUI[7].sprite = numSprite[score10000000];
            if (allDamage < 1000) ScoreUIDysplay(3, true);
            else if (allDamage < 10000) ScoreUIDysplay(4, true);
            else if (allDamage < 100000) ScoreUIDysplay(5, true);
            else if (allDamage < 1000000) ScoreUIDysplay(6, true);
            else if (allDamage < 10000000) ScoreUIDysplay(7, true);
            else ScoreUIDysplay(scoreObj.Length, true);
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
                scoreObj[i].SetActive(isDysPlay);
            }
        }
        /// <summary>
        /// クリアランクを表示します
        /// </summary>
        /// <param name="allDamage">総ダメージ値</param>
        private void ClearRankDisplay(int allDamage)
        {
            //Aランク
            if (allDamage > rankAScore) rankUI.sprite = rankSprite[0];
            //Bランク
            else if (allDamage > rankBScore) rankUI.sprite = rankSprite[1];
            //Cランク
            else
                rankUI.sprite = rankSprite[2];
        }
    }
}