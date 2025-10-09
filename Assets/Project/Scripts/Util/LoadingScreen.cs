using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using OneGear.UI.Utility;

public class LoadingScreen : MonoBehaviour
{
    public Slider progressBar; 
    public string intiGame;
    public Animator animator;
    
    private void Start()
    {
        if (!GameManager.Instance.initGame)
        {
            GameManager.Instance.initGame = true;
            StartCoroutine(LoadAsyncScene(intiGame));
        }
        else
        {
            StartCoroutine(LoadAsyncScene(GameManager.Instance.sceneToLoad));
        }
    }

    private IEnumerator LoadAsyncScene(string sceneName)
    {
        UITransition.Instance.CallTransition(TRANSITIONS.FULL_TO_NULL);
        yield return new WaitForSeconds(2);
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            progressBar.value = progress;
            
            if (asyncLoad.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1f);
                animator.Play("exitlogo");
                UITransition.Instance.CallTransition(TRANSITIONS.NULL_TO_FULL);
                yield return new WaitForSeconds(1.5f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}