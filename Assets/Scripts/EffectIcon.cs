using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectIcon : MonoBehaviour
{
    public EffectData Effect => _effect;
    private EffectData _effect;

    public void SetData(EffectData newEffect)
    {
        _effect = newEffect;
    }
}
