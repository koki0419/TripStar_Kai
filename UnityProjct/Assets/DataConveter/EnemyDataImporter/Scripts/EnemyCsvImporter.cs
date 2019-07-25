using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriprable/Create Enemy CSV Importer")]
public class EnemyCsvImporter : ScriptableObject
{
    public TextAsset csvFile;
    public string StageName;
}
