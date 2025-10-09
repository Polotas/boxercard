using UnityEngine;

 public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
 {
        private static T _instance;

        protected Singleton()
        {
        } //Prevents other classes from creating

        public static T Instance
        {
            get
            {
                //Debug.Log ("Requesting instance: " + typeof(T).ToString());

                if (_instance == null)
                {
                    _instance = (T) FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " + " - there should never be more than 1 singleton!" + " Reopening the scene might fix it. Requested type: " + typeof(T));
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        var typeName = typeof(T).ToString();
                        var split = typeName.Split('.');
                        var scriptName = split[split.Length - 1];

                        // Debug.Log("Script name: " + scriptName);
                        Object prefab = Resources.Load<GameObject>($"Singletons/{scriptName}");
                        GameObject singleton;

                        //Prefab strategy
                        if (prefab != null)
                        {
                            // Debug.Log("[Singleton] An instance was created with a prefab found for it");
                            singleton = Instantiate(prefab) as GameObject;
                            _instance = singleton.GetComponent<T>();
                        }
                        else
                        {
                            //If we don't have a prefab for it, just create a new GO with the component attached
                            // Debug.Log("[Singleton] An instance was created with NO prefab found for it");
                            singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                        }

                        singleton.name = typeof(T).ToString();

//						Debug.Log("[Singleton] An instance of " + typeof(T) +
//						          " is needed in the scene, so '" + singleton +
//						          "' was created with DontDestroyOnLoad.");

                        if (_instance == null)
                            Debug.LogError("Singleton instance is null");
                        else if (_instance.gameObject == null)
                            Debug.LogError("Singleton game objects is null!");

                        if (_instance != null && _instance.gameObject != null)
                            DontDestroyOnLoad(singleton);
                    }
                }

                return _instance;
            }
        }

        public virtual void Start()
        {
            HuntForDuplicates();
        }

        // public void OnLevelWasLoaded(int level)
        // {
        //     HuntForDuplicates();
        // }

        private static void HuntForDuplicates()
        {
            if (!(FindObjectsOfType(typeof(T)) is T[] singletons))
                return;

            foreach (T singleton in singletons)
            {
                if (Instance == singleton)
                    continue;

                Destroy(singleton.gameObject);
            }
        }
}