using UnityEngine;
using System;
using System.Collections;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    /*
        singletonにしたいスクリプト(Controller,Managerとか)のMonoBehaviourの部分をSingletonMonoBehaviour<スクリプト名>にする
        あとは他スクリプトからスクリプト名.Instance.呼び出したい変数,関数名(Public)で呼び出せる
    */

    protected static readonly string[] findTags =
    {
        "GameController",
    };

    protected static T instance;
    public static T Instance
    {
        get
        {
            if ( instance == null )
            {

                Type type = typeof(T);

                foreach ( var tag in findTags )
                {
                    GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);

                    for ( int j = 0 ; j < objs.Length ; j++ )
                    {
                        instance = (T)objs[j].GetComponent(type);
                        if ( instance != null )
                            return instance;
                    }
                }

                Debug.LogWarning(string.Format("{0} is not found", type.Name));
            }

            return instance;
        }
    }

    virtual protected void Awake()
    {
        CheckInstance();
    }

    protected bool CheckInstance()
    {
        if ( instance == null )
        {
            instance = (T)this;
            return true;
        }
        else if ( Instance == this )
        {
            return true;
        }

        Destroy(this);
        return false;
    }
}