using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProject.Gamemain;

public class Singleton : SingletonMonoBehaviour<Singleton>
{
    //『StarGenerator』を取得します
    public StarGenerator starGenerator;
    public GameSceneController gameSceneController;
    public CameraController cameraController;
    public SoundManager soundManager;
    public StarSpawn starSpawn;
    public DamageTextSpawn damageTextSpawn;

}
