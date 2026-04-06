using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name + " (Auto)");
                    _instance = go.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
}

// Now you can just do:
public class SoundManager : Singleton<SoundManager> { /* Your Code */ }