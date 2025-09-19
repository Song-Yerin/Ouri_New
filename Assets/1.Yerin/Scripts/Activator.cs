using UnityEngine;

public class Activator : MonoBehaviour
{
    [Header("활성화할 대상 (비워두면 이 오브젝트 자신)")]
    [SerializeField] private GameObject target;

    /// <summary>
    /// target(또는 자신)을 즉시 활성화한다.
    /// </summary>
    public void Activate()
    {
        var go = target ? target : gameObject;
        if (!go.activeSelf) go.SetActive(true);
    }

    /// <summary>
    /// 지정한 오브젝트를 즉시 활성화한다.
    /// </summary>
    public void Activate(GameObject toActivate)
    {
        if (toActivate && !toActivate.activeSelf)
            toActivate.SetActive(true);
    }

    /// <summary>
    /// 지정한 초 뒤에 활성화한다.
    /// </summary>
    public void ActivateAfter(float delaySeconds)
    {
        StartCoroutine(CoActivateAfter(delaySeconds));
    }

    private System.Collections.IEnumerator CoActivateAfter(float t)
    {
        yield return new WaitForSeconds(t);
        Activate();
    }
}
