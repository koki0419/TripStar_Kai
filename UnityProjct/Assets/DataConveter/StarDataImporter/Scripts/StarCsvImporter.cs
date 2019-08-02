using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriprable/Create Star CSV Importer")]
public class StarCsvImporter : ScriptableObject
{
    // CSVファイルを設定
    public TextAsset csvFile;
    // 生成するScriptableObjectの名前
    public string StageName;
}


