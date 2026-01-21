using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundEffect : MonoBehaviour
{
    [SerializeField] private AudioClip _clickSound;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlaySoundEffect);
    }

    private void PlaySoundEffect()
    {
        GameObject pooledAudioSourceGO = ObjectPool.Instance.GetAudioSource();
        pooledAudioSourceGO.SetActive(true);
        AudioSource audioSource = pooledAudioSourceGO.GetComponent<AudioSource>();
        audioSource.clip = _clickSound;
        audioSource.Play();
    }

    void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(PlaySoundEffect);
    }
}
