using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StarProject.Gamemain;

namespace StarProject.Title
{
    public class SceneController : MonoBehaviour
    {
        private enum TitleTyp
        {
            None,
            TitleSelect,
            ExitSelect,
            Debug,
        }
        private TitleTyp titleTyp = TitleTyp.None;
        private enum DebugTyp
        {
            None,
            Audio,
            BGM,
            SE,
        }
        private DebugTyp debugTyp = DebugTyp.None;
        //フェード関係
        [Header("フェード関係")]
        [SerializeField] private GameObject fadeImageObj = null;
        [SerializeField] private GameObject fadeText = null;
        [SerializeField] private GameObject fadeChara = null;
        private Image fadeImage;
        [SerializeField] private Color fadeOutColor;
        [SerializeField] private float fadeOutTime;
        //ボタン選択用ナンバー
        [Header("ボタン")]
        [SerializeField] private int stageMax = 0;
        [SerializeField] private Image selectButton = null;
        [SerializeField] private Image exitButton = null;
        [SerializeField] private Sprite selectButtonNormalSprite = null;
        [SerializeField] private Sprite selectButtonSelectSprite = null;
        [SerializeField] private Sprite exitButtonNormalSprite = null;
        [SerializeField] private Sprite exitButtonSelectSprite = null;
        //タイトル終了ボタン選択ナンバー
        private int buttonNum;
        //EXITダイアログUI
        [Header("EXIT用テクスチャー")]
        [SerializeField] private GameObject exitDialogUI = null;
        [SerializeField] private Image yesButton = null;
        [SerializeField] private Image noButton = null;
        [SerializeField] private Sprite exitButtonYesNormalSprite = null;
        [SerializeField] private Sprite exitButtonYesSelectSprite = null;
        [SerializeField] private Sprite exitButtonNoNormalSprite = null;
        [SerializeField] private Sprite exitButtonNoSelectSprite = null;
        //EXITボタン選択用ナンバー
        private int exitSelectNum = 0;
        //ゲームパッドjoyコン制御用
        private int countNum;
        private bool debugFlag = false;

        // Start is called before the first frame update
        void Init()
        {
            buttonNum = 0;
            countNum = 0;
            exitSelectNum = 0;
            exitDialogUI.SetActive(false);
            fadeImage = fadeImageObj.GetComponent<Image>();
            TitleSelectButton(buttonNum);
        }

        //スタート
        IEnumerator Start()
        {
            Init();
            yield return null;
            if (SoundManager.audioVolume != 0) Singleton.Instance.soundManager.AudioVolume();
            else Singleton.Instance.soundManager.AllAudioVolume();
            Singleton.Instance.soundManager.PlayBgm("NormalBGM");
            titleTyp = TitleTyp.TitleSelect;
        }
        private void Awake()
        {
            FadeImageDysplay(false);
        }
        // Update is called once per frame
        void Update()
        {
            switch (titleTyp)
            {
                case TitleTyp.TitleSelect:
                    TitleSeletUpdate();
                    break;
                case TitleTyp.ExitSelect:
                    ExitSelectUpdate();
                    break;
                case TitleTyp.Debug:
                    DebugUpdate();
                    break;
            }
        }
        int debugSelectIndex = 0;
        int debugJoyCount = 0;
        private const float debugSelectHeightGUISize = 250;
        private const float debugSelectWidhtGUISize = 500;
        private const float debugNormalHeightGUISize = 125;
        private const float debugNormalWidhtGUISize = 250;
        void OnGUI()
        {
            if (debugFlag)
            {
                GUIStyle myLabelStyle = new GUIStyle(GUI.skin.label);
                myLabelStyle.fontSize = 50;

                float dx = Input.GetAxis("Horizontal");
                float dy = Input.GetAxis("Vertical");
                if (dy < 0 && debugJoyCount == 0)
                {
                    debugSelectIndex++;
                    debugJoyCount++;
                    if (debugSelectIndex < 0)
                    {
                        debugSelectIndex = 0;
                    }
                }
                else if (dy > 0 && debugJoyCount == 0)
                {
                    debugSelectIndex--;
                    debugJoyCount++;
                    if (debugSelectIndex > 3)
                    {
                        debugSelectIndex = 3;
                    }
                }
                else if (dy == 0)
                {
                    debugJoyCount = 0;
                }
                if (debugSelectIndex == 1) debugTyp = DebugTyp.Audio;
                else if (debugSelectIndex == 2) debugTyp = DebugTyp.BGM;
                else if (debugSelectIndex == 3) debugTyp = DebugTyp.SE;
                switch (debugTyp)
                {
                    case DebugTyp.Audio:
                        // 全てのオーディオに影響します
                        if (dx < 0)
                        {
                            SoundManager.audioVolume -= 0.01f;
                            if (SoundManager.audioVolume < 0) SoundManager.audioVolume = 0;
                        }
                        else if (dx > 0)
                        {
                            SoundManager.audioVolume += 0.01f;
                            if (SoundManager.audioVolume > 1) SoundManager.audioVolume = 1;
                        }
                        GUI.Label(new Rect(100, 150, 500, 500), "AudioVolume:", myLabelStyle);
                        SoundManager.audioVolume = GUI.HorizontalSlider(new Rect(100, 200, debugSelectWidhtGUISize, debugSelectHeightGUISize), SoundManager.audioVolume, 0.0F, 1.0F);
                        GUI.Label(new Rect(100, 250, 250, 250), "BGMVolume:", myLabelStyle);
                        SoundManager.bgmVolume = GUI.HorizontalSlider(new Rect(100, 300, debugNormalWidhtGUISize, debugNormalHeightGUISize), SoundManager.bgmVolume, 0.0F, 1.0F);
                        GUI.Label(new Rect(100, 350, 250, 250), "SeVolume:", myLabelStyle);
                        SoundManager.seVolume = GUI.HorizontalSlider(new Rect(100, 400, debugNormalWidhtGUISize, debugNormalHeightGUISize), SoundManager.seVolume, 0.0F, 1.0F);
                        break;
                    case DebugTyp.BGM:
                        // BGMオーディオに影響します
                        if (dx < 0)
                        {
                            SoundManager.bgmVolume -= 0.01f;
                            if (SoundManager.bgmVolume < 0) SoundManager.bgmVolume = 0;
                        }
                        else if (dx > 0)
                        {
                            SoundManager.bgmVolume += 0.01f;
                            if (SoundManager.bgmVolume > 1) SoundManager.bgmVolume = 1;
                        }
                        GUI.Label(new Rect(100, 150, 250, 250), "AudioVolume:", myLabelStyle);
                        SoundManager.audioVolume = GUI.HorizontalSlider(new Rect(100, 200, debugNormalWidhtGUISize, debugNormalHeightGUISize), SoundManager.audioVolume, 0.0F, 1.0F);
                        GUI.Label(new Rect(100, 250, 500, 500), "BGMVolume:", myLabelStyle);
                        SoundManager.bgmVolume = GUI.HorizontalSlider(new Rect(100, 300, debugSelectWidhtGUISize, debugSelectHeightGUISize), SoundManager.bgmVolume, 0.0F, 1.0F);
                        GUI.Label(new Rect(100, 350, 250, 250), "SeVolume:", myLabelStyle);
                        SoundManager.seVolume = GUI.HorizontalSlider(new Rect(100, 400, debugNormalWidhtGUISize, debugNormalHeightGUISize), SoundManager.seVolume, 0.0F, 1.0F);
                        if (SoundManager.bgmVolume != 0) SoundManager.audioVolume = 1.0f;
                        break;
                    case DebugTyp.SE:
                        // SEオーディオに影響します
                        if (dx < 0)
                        {
                            SoundManager.seVolume -= 0.01f;
                            if (SoundManager.seVolume < 0) SoundManager.seVolume = 0;
                        }
                        else if (dx > 0)
                        {
                            SoundManager.seVolume += 0.01f;
                            if (SoundManager.seVolume > 1) SoundManager.seVolume = 1;
                        }
                        GUI.Label(new Rect(100, 150, 250, 250), "AudioVolume:", myLabelStyle);
                        SoundManager.audioVolume = GUI.HorizontalSlider(new Rect(100, 200, debugNormalWidhtGUISize, debugNormalHeightGUISize), SoundManager.audioVolume, 0.0F, 1.0F);
                        GUI.Label(new Rect(100, 250, 250, 250), "BGMVolume:", myLabelStyle);
                        SoundManager.bgmVolume = GUI.HorizontalSlider(new Rect(100, 300, debugNormalWidhtGUISize, debugNormalHeightGUISize), SoundManager.bgmVolume, 0.0F, 1.0F);
                        GUI.Label(new Rect(100, 350, 500, 500), "SeVolume:", myLabelStyle);
                        SoundManager.seVolume = GUI.HorizontalSlider(new Rect(100, 400, debugSelectWidhtGUISize, debugSelectHeightGUISize), SoundManager.seVolume, 0.0F, 1.0F);
                        if (SoundManager.seVolume != 0) SoundManager.audioVolume = 1.0f;
                        break;
                }
            }
        }

        private void DebugUpdate()
        {
            if (SoundManager.audioVolume != 0) Singleton.Instance.soundManager.AudioVolume();
            else Singleton.Instance.soundManager.AllAudioVolume();
            //debugupdateへ
            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.W))
            {
                debugFlag = false;
                titleTyp = TitleTyp.TitleSelect;
            }
        }
        //アプリ終了関数
        private void OnExit()
        {
            Application.Quit();
            Debug.Log("アプリ終了");
        }
        private void TitleSeletUpdate()
        {
            float dx = Input.GetAxis("Horizontal");
            //左
            if (dx < 0 && countNum == 0)
            {
                countNum++;
                if (buttonNum > 0) buttonNum--;
                TitleSelectButton(buttonNum);
            }//右
            else if (dx > 0 && countNum == 0)
            {
                countNum++;
                if (buttonNum < stageMax) buttonNum++;
                TitleSelectButton(buttonNum);
            }
            else if (dx == 0 && countNum != 0)
            {
                countNum = 0;
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("SelectOk"))
            {
                switch (buttonNum)
                {
                    case 0:
                        StartCoroutine(GameStartEnumerator());
                        titleTyp = TitleTyp.None;
                        break;
                    case 1:
                        exitDialogUI.SetActive(true);
                        exitSelectNum = 0;
                        ExitSelectButton(exitSelectNum);
                        titleTyp = TitleTyp.ExitSelect;
                        break;
                }
            }
            //debugupdateへ
            if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.E))
            {
                debugFlag = true;
                titleTyp = TitleTyp.Debug;
                debugTyp = DebugTyp.Audio;
            }
        }
        private void ExitSelectUpdate()
        {
            float dx = Input.GetAxis("Horizontal");
            //左
            if (dx < 0 && countNum == 0)
            {
                countNum++;
                if (exitSelectNum > 0) exitSelectNum--;
                ExitSelectButton(exitSelectNum);
            }//右
            else if (dx > 0 && countNum == 0)
            {
                countNum++;
                if (exitSelectNum < 1) exitSelectNum++;
                ExitSelectButton(exitSelectNum);
            }
            else if (dx == 0 && countNum != 0)
            {
                countNum = 0;
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("SelectOk"))
            {
                switch (exitSelectNum)
                {
                    case 0:
                        exitDialogUI.SetActive(false);
                        titleTyp = TitleTyp.TitleSelect;
                        break;
                    case 1:
                        OnExit();
                        titleTyp = TitleTyp.None;
                        break;
                }
            }
        }
        /// <summary>
        /// タイトルボタンセレクト時に画像を切り替えます
        /// </summary>
        /// <param name="selectNum"></param>
        private void TitleSelectButton(int selectNum)
        {
            switch (selectNum)
            {
                case 0:
                    selectButton.sprite = selectButtonSelectSprite;
                    exitButton.sprite = exitButtonNormalSprite;
                    break;
                case 1:
                    selectButton.sprite = selectButtonNormalSprite;
                    exitButton.sprite = exitButtonSelectSprite;
                    break;
            }
        }
        /// <summary>
        /// 終了ボタン選択時YesNoボタンセレクトの画像を切り替えます
        /// </summary>
        /// <param name="selectNum"></param>
        private void ExitSelectButton(int selectNum)
        {
            switch (selectNum)
            {
                case 0:
                    yesButton.sprite = exitButtonYesNormalSprite;
                    noButton.sprite = exitButtonNoSelectSprite;
                    break;
                case 1:
                    yesButton.sprite = exitButtonYesSelectSprite;
                    noButton.sprite = exitButtonNoNormalSprite;
                    break;
            }
        }
        /// <summary>
        /// ゲームスタート時に実行します
        /// </summary>
        /// <returns></returns>
        private IEnumerator GameStartEnumerator()
        {
            ForceColor(Color.clear);
            yield return FadeEnumerator(Color.clear, fadeOutColor, fadeOutTime);
            GameSceneController.stageNum = 1;
            SceneManager.LoadScene("main01");
        }
        /// <summary>
        /// フェードアウト
        /// </summary>
        /// <param name="color">最終カラー</param>
        /// <param name="period">フェード時間</param>
        /// <returns></returns>
        //private IEnumerator FadeOutEnumerator(Color color, float period)
        //{
        //    yield return 
        //}
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
        /// フェード用Imageの表示非表示
        /// </summary>
        /// <param name="isDysplay">表示非表示</param>
        private void FadeImageDysplay(bool isDysplay)
        {
            fadeImageObj.SetActive(isDysplay);
        }
    }
}