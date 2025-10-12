using UnityEngine;
public class FadeKiller : MonoBehaviour
{
    [SerializeField] GameObject[] fadeObjects; // FADEONLY, BlackPanel µî µî·Ï
    public void TurnOff()
    {
        foreach (var go in fadeObjects) if (go) go.SetActive(false);
    }
}
