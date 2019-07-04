using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverLineController : MonoBehaviour
{

    public enum GameOverLineState
    {
        None,
        Sealed,//封印状態
        Awakening,//覚醒状態
    }
    public GameOverLineState gameOverLineState = GameOverLineState.None;

    private Animator gameOverLineAnimator;

    [SerializeField] private　int awakeningSeNum;

    public void Init()
    {
        gameOverLineAnimator = GetComponent<Animator>();
        gameOverLineState = GameOverLineState.Sealed;
    }
    public void GameOverLineAnimation()
    {
        switch (gameOverLineState)
        {
            case GameOverLineState.Sealed:
                break;
            case GameOverLineState.Awakening:
                gameOverLineAnimator.SetTrigger("OnAwakening");
                break;
        }
    }

    public void PlayMoaiAwakeningSE()
    {
        Singleton.Instance.soundManager.StopPlayerSe();
        Singleton.Instance.soundManager.PlayPlayerSe(awakeningSeNum);
    }
}
