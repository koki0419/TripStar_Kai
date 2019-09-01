using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using StarProject.Result;

namespace StarProject.Gamemain
{
    public class GameSceneController : MonoBehaviour
    {
        /// <summary>
        /// ゲームのシーン状態
        /// </summary>
        public enum GameMainState
        {
            None,
            Opening,// オープニング
            Play,// ゲーム中
            Pause,// 中断
            Tutorial,// チュートリアル
            SpecialProduction,// スペシャル演出
            BossMoaiAttack,// ボスモアイの攻撃
            GameClear,// ゲームクリア
            GameOver,// ゲームオーバー
        }
        public GameMainState gameMainState
        {
            private set; get;
        }
        // ---------Unityコンポーネント宣言--------------
        [SerializeField] private GameObject playerObj = null;
        [SerializeField] private GameObject safeHitGigMoaiObj = null;
        // エネミーのスクリプト取得用
        [SerializeField] private GameObject enemysObj = null;
        private EnemyController[] enemyController = null;
        private GameObject[] enemyChilledObj = null;
        private ObstacleManager[] obstacleManager = null;
        [SerializeField] private GameObject gameOverLineObj = null;
        [SerializeField] private GameObject mainCamera = null;
        [SerializeField] private GameObject openingCamera = null;
        private GameObject fastTargetObj = null;
        // ------------クラスの宣言----------------------
        [SerializeField] private PlayerMove playerMove = null;
        public PlayerMove PlayerMove
        {
            get { return playerMove; }
        }
        [SerializeField] private Boss[] boss = null;
        private CameraController cameraController = null;
        [SerializeField] private StarChargeController starChargeController = null;
        public StarChargeController StarChargeController
        {
            get { return starChargeController; }
        }
        // 変数を直接参照させないでプロパティ文法でアクセサを経由させる
        public ChargePointManager ChargePointManager
        {
            get { return chargePointManager; }
        }
        // 変数本体でInspectorにはこちらが表示される
        [SerializeField] private ChargePointManager chargePointManager = null;
        [SerializeField] private UiManager uiManager = null;
        private GameOverLineController gameOverLineController = null;
        [SerializeField] private StarGenerator starGenerator = null;
        [SerializeField] private StarSpawn starSpawn = null;
        [SerializeField] private DamageTextSpawn damageTextSpawn = null;
        [SerializeField] private ObstacleSpawn obstacleSpawn = null;

        [SerializeField] private Light light = null;
        [SerializeField] private List<TutorialTask> tutorialTasks = new List<TutorialTask>();
        private int checkTutorialTasksNum = 0;
        // ------------数値変数の宣言--------------------
        // 現在のステージ番号 // リザルトでリトライやNextステージで使用します
        // タイトルでstageNumを1に設定します。その後はリザルトシーンのみでしか使用しません
        static public int stageNum;
        // エネミーを何体倒したか
        // ステージ１のカメラ起動用に使用します
        private int destroyCount = 0;
        // モアイの攻撃範囲
        private const float fastAttackBossMoaiScreenProgressRange = 5;
        private const float secondAttackBossMoaiScreenProgressRange = 5;
        // ------------フラグ変数の宣言------------------
        // ゲームクリア
        public bool isGameClear
        {
            set; private get;
        }
        private bool gameClearMovie;
        // ゲームオーバー
        public bool isGameOver
        {
            set; get;
        }
        // カメラを振動出せるかどうか
        private bool canCameraShake;
        // カメラを動かすかどうか
        private bool isMoveCamera;
        // ゲームオーバー時にボタンを操作できるかどうか
        private bool isOperation;
        // 登場演出が終了したかどうか
        private bool exitOpning;
        // モアイのアタックコルーチン用
        private bool bigMoaiAttack = false;
        // debug状態かどうか
        [SerializeField] private bool debug;
        // ボス攻撃を解放する
        [SerializeField] private bool release_BossAttack = false;
        // チュートリアルを解放する
        [SerializeField] private bool release_Tutorial = false;
        // カメラのズームアップ/ダウン
        [SerializeField] private bool release_CameraZoomUp = false;
        [SerializeField] private bool release_CameraZoomDown = false;
        // プレイ中かどうか // 基本gameMainStateで管理しているが管理できない部分用
        public static bool isPlaying;
        // ビックモアイの攻撃
        bool isBigMoaiAttack = false;
        [SerializeField] private bool oldSystem;
        [SerializeField] private bool newSystem;
        // 複数個☆を獲得した
        public bool isMultipleAcquisition
        {
            get; set;
        }
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            // gaugeDroportion = (float)PlayerMove.PlayerBeastModeState.StarCost / 100;//StarCostを『0.01』にする
            for (int i = 0; i < boss.Length; i++)
            {
                boss[i].Init();
            }
            cameraController = Singleton.Instance.cameraController;
            // エネミー子供オブジェクトを取得
            enemyChilledObj = new GameObject[obstacleSpawn.SpawnMax];
            enemyController = new EnemyController[obstacleSpawn.SpawnMax];
            obstacleManager = new ObstacleManager[obstacleSpawn.SpawnMax];
            fastTargetObj = enemysObj.transform.GetChild(0).gameObject;
            gameOverLineController = gameOverLineObj.GetComponent<GameOverLineController>();
            gameOverLineObj.SetActive(false);
            isGameClear = false;
            isGameOver = false;
            isOperation = false;
            canCameraShake = false;
            gameClearMovie = false;
            isMultipleAcquisition = false;
            isPlaying = false;
            if(!newSystem && !oldSystem)
            {
                oldSystem = true;
            }
            ResultScreenController.all_damage = 0;
        }
        // Start()より早く処理する
        private void Awake()
        {
            uiManager.Init();
            CameraSelect(false, true);
        }
        // スタート
        IEnumerator Start()
        {
            exitOpning = false;
            playerMove.Init();
            yield return null;
            obstacleSpawn.Init();
            yield return null;
            Init();
            starChargeController.Init();
            cameraController.Init();
            chargePointManager.Init();
            gameOverLineController.Init();
            starSpawn.Init();
            damageTextSpawn.Init();
            yield return null;
            yield return uiManager.FadeInEnumerator();
            gameMainState = GameMainState.Opening;
            if (SoundManager.audioVolume != 0) Singleton.Instance.soundManager.AudioVolume();
            else Singleton.Instance.soundManager.AllAudioVolume();
            Singleton.Instance.soundManager.PlayBgm("NormalBGM");
        }
        // 本体のアップデート
        void Update()
        {
            switch (gameMainState)
            {
                case GameMainState.Opening:
                    OpeningUpdate();
                    break;
                case GameMainState.Play:
                    GamePlay();
                    break;
                case GameMainState.Pause:
                    GamePause();
                    break;
                case GameMainState.Tutorial:
                    TutorialUpdate();
                    break;
                case GameMainState.SpecialProduction:
                    SpecialProductionUpdate();
                    break;
                case GameMainState.BossMoaiAttack:
                    BossMoaiAttack();
                    break;
                case GameMainState.GameClear:
                    GameClear();
                    break;
                case GameMainState.GameOver:
                    GameOver();
                    break;
            }
            // カメラShake
            if (canCameraShake)
            {
                cameraController.Shake(0.25f, 0.1f);
            }
        }

        private void LateUpdate()
        {
            if (gameMainState == GameMainState.Play)// ゲームスタート
            {
                chargePointManager.OnUpdate();
            }
        }
        /// <summary>
        /// チュートリアルの各フラグの確認
        /// </summary>
        private bool TutorialCheck()
        {
            var starCount = chargePointManager.StarChildCount;
            // 星20個溜まった時
            if (starCount >= 20 && !tutorialTasks[0].CheckTask)
            {
                // uiManager.GetTutorialUiSprite(tutorialTasks[0].tutorialUiSprite);
                return true;
            }// 星50個溜まった時
            else if (starCount == 50 && !tutorialTasks[2].CheckTask)
            {
                // uiManager.GetTutorialUiSprite(tutorialTasks[2].tutorialUiSprite);
                return true;
            }
            else
            {
                return false;
            }
            //isPlaying = false;
        }
        private bool checkTutorial = false;
        private bool checkRunAgain = false; // もう一度実行するか
        /// <summary>
        /// ステージ1のチュートリアル
        /// </summary>
        private void TutorialUpdate()
        {
            if (oldSystem)
            {
                uiManager.ViewTutorialUI(true);
                if (Input.GetKeyDown(KeyCode.L))
                {
                    tutorialTasks[checkTutorialTasksNum].CheckTask = true;
                    checkTutorialTasksNum++;
                    if (checkTutorialTasksNum == 1)
                    {

                        uiManager.GetTutorialUiSprite(tutorialTasks[1].tutorialUiSprite);
                    }
                    else
                    {
                        uiManager.ViewTutorialUI(false);
                        isPlaying = true;
                        gameMainState = GameMainState.Play;
                    }
                }
            }
            if (newSystem)
            {
                if (!checkTutorial)
                {
                    checkTutorial = true;
                    StartCoroutine(TutorialIEnumerator());
                }
            }
        }
        private IEnumerator TutorialIEnumerator()
        {
            tutorialTasks[checkTutorialTasksNum].CheckTask = true;
            uiManager.GetTutorialUiSprite(tutorialTasks[checkTutorialTasksNum].tutorialUiSprite);
            if (checkTutorialTasksNum == 0)
                checkRunAgain = true;
            else
                checkRunAgain = false;
            checkTutorialTasksNum++;
            uiManager.ViewTutorialUI(true);
            uiManager.TutorialStartAnimation();
            yield return new WaitForSeconds(3.0f);
            uiManager.TutorialComeBackAnimation();
            yield return new WaitForSeconds(3.0f);
            uiManager.ViewTutorialUI(false);
            checkTutorial = false;
            // ここから下必要か検討
            //isPlaying = true;
            if (!checkRunAgain)
                gameMainState = GameMainState.Play;
        }
        /// <summary>
        /// モアイ動きスタート
        /// </summary>
        IEnumerator BigMoaiMoveStart()
        {
            gameOverLineController.PlayMoaiAwakeningSE();
            gameOverLineController.gameOverLineState = GameOverLineController.GameOverLineState.Awakening;
            gameOverLineController.GameOverLineAnimation();
            yield return new WaitForSeconds(1.0f);
            canCameraShake = true;
            yield return new WaitForSeconds(3.5f);
            canCameraShake = false;
            Destroy(safeHitGigMoaiObj);
            isMoveCamera = true;
            if (gameMainState != GameMainState.Pause) gameMainState = GameMainState.Play;
        }
        /// <summary>
        /// ゲーム開始時登場演出
        /// </summary>
        void OpeningUpdate()
        {
            if (!exitOpning)
            {
                AnimatorStateInfo animInfo = playerMove.PlayerAnimator.GetCurrentAnimatorStateInfo(0);
                if (animInfo.normalizedTime < 1.0f)
                {
                    return;
                }
                else
                {
                    exitOpning = true;
                    StartCoroutine(OpeningEnumerator());
                }
            }
        }
        /// <summary>
        /// オープニングが終了した際に残りのオブジェクトを設定する
        /// </summary>
        /// <returns></returns>
        IEnumerator OpeningEnumerator()
        {
            yield return uiManager.FadeOutEnumerator();
            gameOverLineObj.SetActive(true);
            CameraChange();
            uiManager.StarUICanvasDisplay(true);
            starGenerator.Init();
            playerMove.CharacterAnimation("gameStart");
            yield return uiManager.FadeInEnumerator();
            if (!debug)
            {
                if (stageNum == 1)
                {
                    isMoveCamera = false;
                }
                else
                {
                    StartCoroutine(BigMoaiMoveStart());
                }
            }
            else
            {
                isMoveCamera = false;
            }
            isPlaying = true;
            gameMainState = GameMainState.Play;
        }
        /// <summary>
        /// ゲーム本体のメインアップデート
        /// </summary>
        void GamePlay()
        {
            starGenerator.StarSponUpdate();
            obstacleSpawn.ObstaclesSponUpdate();
            float deltaTime = Time.deltaTime;
            cameraController.MoveUpdate(deltaTime, isMoveCamera);
            gameOverLineController.MoveUpdate(deltaTime, isMoveCamera);
            playerMove.OnUpdate(deltaTime);// PlayerのUpdate
                                           // ☆エネミー子供オブジェクト初期化
                                           // debugモードOnOff
            if (!debug)
            {
                if (fastTargetObj != null && fastTargetObj.GetComponent<ObstacleManager>().IsDestroyed && stageNum == 1 && destroyCount == 0)
                {
                    destroyCount++;
                    StartCoroutine(BigMoaiMoveStart());
                }
            }
            else
            {
                if (fastTargetObj != null && fastTargetObj.GetComponent<ObstacleManager>().IsDestroyed && destroyCount == 0)
                {
                    destroyCount++;
                    StartCoroutine(BigMoaiMoveStart());
                }
            }
            // デカいモアイ攻撃
            if (release_BossAttack)
            {
                // 追ってモアイが攻撃をしてくるかどうかを確認する
                var CheckCameraMoaiAttack = CheckThePositionOfPlayerAndCameraMoai(Camera.main.transform.position);
                if (CheckCameraMoaiAttack != -1)
                {
                    if (light.intensity <= 0.7f)
                    {
                        if (!bigMoaiAttack)
                        {
                            StartCoroutine(BigMoaiAttackIEnumerator());
                            bigMoaiAttack = true;
                        }
                    }
                    else
                    {
                        light.intensity -= 0.01f;
                    }
                }
                if (isBigMoaiAttack)
                {
                    BossMoaiAttack();
                }
            }
            // チュートリアル
            if (release_Tutorial)
            {
                // ステージ１の時チュートリアルを挟みます
                if (stageNum == 1 || debug)
                {
                    if (TutorialCheck() || checkRunAgain)
                    {
                        TutorialUpdate();
                        //gameMainState = GameMainState.Tutorial;
                    }
                }
            }
            // カメラ演出
            if (release_CameraZoomUp)
            {
                cameraController.CameraZoomUp();
            }
            if (release_CameraZoomDown)
            {
                cameraController.CameraZoomDown();
            }
            // チャージ5の時のスペシャル演出
            if (playerMove.IsSpecialProduction)
            {
                gameMainState = GameMainState.SpecialProduction;
            }
            // ゲームオーバー
            if (isGameOver)
            {
                Singleton.Instance.soundManager.PlayJingle("GameOver");
                gameMainState = GameMainState.GameOver;
            }
            // ゲームクリア
            if (isGameClear)
            {
                Singleton.Instance.soundManager.PlayJingle("GameClear");
                gameClearMovie = true;
                gameMainState = GameMainState.GameClear;
            }
            // ポーズ
            if (Input.GetButtonDown("Pause") || Input.GetKeyDown(KeyCode.Escape))
            {
                uiManager.PauseDiaLogDisplay(true);
                isPlaying = false;
                gameMainState = GameMainState.Pause;
            }
        }
        /// <summary>
        /// ポイントライトを光らせて攻撃開始
        /// </summary>
        /// <returns></returns>
        IEnumerator BigMoaiAttackIEnumerator()
        {
            yield return gameOverLineController.SpotLightOnOff();
            isBigMoaiAttack = true;
            // TODO: 攻撃時にカメラを止めず、且つプレイヤーも止めないようにする
            //isPlaying = false;
            //gameMainState = GameMainState.BossMoaiAttack;

        }
        /// <summary>
        /// Pause時の処理
        /// この時にもう一度Pauseボタンを押すとPlayモードに戻る
        /// </summary>
        void GamePause()
        {
            uiManager.PauseButtonSelectUpdate();
            if (Input.GetButtonDown("Pause") || Input.GetKeyDown(KeyCode.Escape))
            {
                uiManager.PauseDiaLogDisplay(false);
                isPlaying = true;
                gameMainState = GameMainState.Play;
            }
        }
        /// <summary>
        /// メインキャラクターチャージ5の時の
        /// スペシャル演出アップデート
        /// </summary>
        float specialProductionTime = 2.0f;
        void SpecialProductionUpdate()
        {
            specialProductionTime -= Time.deltaTime;
            if (specialProductionTime <= 0)
            {
                specialProductionTime = 2.0f;
                gameMainState = GameMainState.Play;
            }
        }
        bool returnPosition = false;
        /// <summary>
        /// 追ってくるモアイの攻撃update
        /// </summary>
        void BossMoaiAttack()
        {
            var checkAttack = true;
            canCameraShake = true;
            if (!fastBossAttack && !returnPosition)
            {
                checkAttack = gameOverLineController.BigMoaiAttack(fastAttackBossMoaiScreenProgressRange, 0);
                if (!checkAttack)
                {
                    fastBossAttack = true;
                    returnPosition = true;
                }
            }
            else if (!secondBossAttack && !returnPosition)
            {
                checkAttack = gameOverLineController.BigMoaiAttack(secondAttackBossMoaiScreenProgressRange, 1);
                if (!checkAttack)
                {
                    secondBossAttack = true;
                    returnPosition = true;
                }
            }
            // 攻撃終了時元のポジションまで帰る
            if (returnPosition)
            {
                var checkReturn = gameOverLineController.ReturnPosipoin();
                if (!checkReturn)
                {
                    // TODO: 書くコメントアウトしたものが必要不必要を確認
                    canCameraShake = false;
                    // isMoveCamera = true;
                    returnPosition = false;
                    bigMoaiAttack = false;
                    light.intensity = 1.0f;
                    // isPlaying = true;
                    isBigMoaiAttack = false;
                    // gameMainState = GameMainState.Play;
                }
            }
        }
        /// <summary>
        /// /ゲームクリア時の処理
        /// </summary>
        void GameClear()
        {
            playerMove.GameClear();
            if (gameClearMovie)
            {
                gameClearMovie = false;
                StartCoroutine(OnClear());
            }
        }
        /// <summary>
        /// ゲームオーバー時の処理
        /// コルーチンで一定時間後にゲームオーバーダイアログを操作可能にできる
        /// isOperationはゲームオーバー時に適当にボタンを押されるとダイアログ表示前に【リトライorやめる】に遷移してしまう
        /// ため設定しました
        /// </summary>
        void GameOver()
        {
            StartCoroutine(OnGameOver());
            if (isOperation)
            {
                uiManager.GameOverButtonSelectUpdate();
            }
        }
        // クリア
        IEnumerator OnClear()
        {
            canCameraShake = true;
            ResultScreenController.allStar = chargePointManager.StarChildCount;
            yield return new WaitForSeconds(0.5f);
            uiManager.GameClearUIDisplay(true);
            yield return new WaitForSeconds(3.0f);
            canCameraShake = false;
            SceneManager.LoadScene("ResultScene");
        }
        // ゲームオーバー
        IEnumerator OnGameOver()
        {
            yield return new WaitForSeconds(0.5f);
            uiManager.GameOvreUIDisplay(true);
            yield return new WaitForSeconds(2.5f);
            uiManager.GameOverDiaLogDisplay(true);
            isOperation = true;
        }
        /// <summary>
        /// カメラの種類を選択します
        /// </summary>
        /// <param name="mainCameraActive">メインカメラ</param>
        /// <param name="openingCameraActive">オープニング用カメラ</param>
        void CameraSelect(bool mainCameraActive, bool openingCameraActive)
        {
            mainCamera.SetActive(mainCameraActive);
            openingCamera.SetActive(openingCameraActive);
        }
        /// <summary>
        /// カメラを入れ替えます
        /// </summary>
        void CameraChange()
        {
            if (mainCamera.activeSelf)
            {
                mainCamera.SetActive(false);
                openingCamera.SetActive(true);
            }
            else
            {
                mainCamera.SetActive(true);
                openingCamera.SetActive(false);
            }
        }
        // 1度目の攻撃フラグ
        bool fastBossAttack = false;
        // 2度目の攻撃フラグ
        bool secondBossAttack = false;
        // 追ってモアイの攻撃範囲に入ったかどうか
        private int CheckThePositionOfPlayerAndCameraMoai(Vector3 cameraPosition)
        {
            if (gameOverLineController.FastAttackPosition.x <= cameraPosition.x && !fastBossAttack)
            {
                // isMoveCamera = false;
                return 1;
            }
            else if (gameOverLineController.SecondAttackPosition.x <= cameraPosition.x && !secondBossAttack)
            {
                // isMoveCamera = false;
                return 2;
            }
            else return -1;
        }
    }
}
