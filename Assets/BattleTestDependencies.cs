using UnityEngine;

public class BattleTestDependencies : MonoBehaviour
{
    public static BattleTestDependencies Instance;

    private void Start()
    {
        if (GameObject.Find("GameManager") != null)
        {
            gameObject.SetActive(false);
        }
    }
}
