using UnityEngine;
using System.Collections.Generic;

public class BloodSplat : MonoBehaviour
{
    // magic numbers begone!
    public const float BLOOD_SPLAT_SPAWN_CHANCE = .25F;
    private const float SPREAD = .5F;
    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] private List<Sprite> sprites; 

    public void Activate(Vector3 position, int sortingOrder)
    {
        if (transform.childCount <= 0)
        {
            Debug.LogWarning("Blood splat doesn't have a child!");
            gameObject.SetActive(false);    
        }

        transform.position = position;
        transform.Translate(Random.Range(-SPREAD, SPREAD), Random.Range(-SPREAD, SPREAD), 0f);
        GetComponentInChildren<SpriteRenderer>().sortingOrder = sortingOrder;
        
        // just flip sometimes to make it more diverse
        float rotationChance = .5f;
        if (Random.Range(0f,1f) > rotationChance)
        {
            transform.GetChild(0).Rotate(180, 0, 0);
        }

        gameObject.SetActive(true);
    }
}
