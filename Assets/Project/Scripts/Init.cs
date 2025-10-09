using System.Collections;
using OneGear.UI.Utility;
using UnityEngine;

public class Init : MonoBehaviour
{
    public Animator animator;
    
    private void Start()
    {
        StartCoroutine(LoadScene("01_Loading"));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        yield return new WaitForSeconds(2);
        UITransition.Instance.BackToLoading(sceneName,TRANSITIONS.NULL_TO_FULL,HideLogo,1);
    }
    
    private void HideLogo() => animator.Play("exitlogo");
}
