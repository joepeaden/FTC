using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalCastle : MonoBehaviour
{
    // the integer represents remaining health
    public UnityEvent<int> OnGetHit = new();

    [SerializeField] private int _remainingHitPoints;

    public void GetHit(int dmg)
    {
        _remainingHitPoints -= dmg;
        OnGetHit.Invoke(_remainingHitPoints);
    } 
}
