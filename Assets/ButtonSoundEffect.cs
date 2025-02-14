using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundEffect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlaySoundEffect);
    }

    private void PlaySoundEffect()
    {
        GameManager.Instance.PlayClickEffect();
    }

    void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(PlaySoundEffect);
    }
}
