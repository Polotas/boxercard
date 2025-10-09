using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TRANSITIONS
{
    NULL_TO_FULL,
    NULL_TO_MIDLE,
    FULL_TO_NULL,
    FULL_TO_MIDDLE,
    MIDDLE_TO_NULL,
    MIDDLE_TO_FULL
    
}

namespace OneGear.UI.Utility
{
    public class UITransition : MonoBehaviour
    {
        public Animator animator;
        
        public static UITransition Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        
        public void CallTransition(TRANSITIONS transitions)
        {
            switch (transitions)
            {
                case TRANSITIONS.NULL_TO_FULL:
                    animator.Play("null-to-full");
                    break;
                case TRANSITIONS.NULL_TO_MIDLE:
                    animator.Play("null-mid");
                    break;
                case TRANSITIONS.FULL_TO_NULL:
                    animator.Play("full-to-null");
                    break;
                case TRANSITIONS.FULL_TO_MIDDLE:
                    animator.Play("full-mid");
                    break;
                case TRANSITIONS.MIDDLE_TO_NULL:
                    animator.Play("mid-null");
                    break;
                case TRANSITIONS.MIDDLE_TO_FULL:
                    animator.Play("mid-full");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transitions), transitions, null);
            }
        }

        public void BackToLoading(string sceneToLoad, TRANSITIONS transitions, Action afterTransition = null,
            float initDelay = 0)
        {
            GameManager.Instance.sceneToLoad = sceneToLoad;
            StartCoroutine(LoadScene("01_Loading", transitions, afterTransition, initDelay));
        } 
        
        public void ChangeScene(string sceneToLoad, TRANSITIONS transitions, Action afterTransition = null,
            float initDelay = 0) => StartCoroutine(LoadScene(sceneToLoad, transitions, afterTransition, initDelay));
        
        private IEnumerator LoadScene(string sceneName,TRANSITIONS transitions, Action afterTransition,float initDelay)
        {
            CallTransition(transitions);

            if (afterTransition != null)
            {
                yield return new WaitForSeconds(initDelay);
                afterTransition.Invoke();
                yield return new WaitForSeconds(1.2f);
            }
            else
            {
                yield return new WaitForSeconds(1);
            }
            
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}


