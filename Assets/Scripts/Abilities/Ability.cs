using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// TIL you can use scriptable objects in such a way. I always just used them for
// data storage alone - but you can actually just use them the same way you use
// other scripts. Super cool. Cold brew baby. Let's go.

public class Ability : ScriptableObject
{
    // scriptable info - define the data for the ability
    [Header("Descriptive")]
    public string abilityID;
    public string abilityName;
    public string description;
    public int motCost;
    public int range;
    public Sprite sprite;

    /// <summary>
    /// The current ability a pawn is about to use.
    /// </summary>
    /// <remarks>
    /// I'm not sure about this, but I think it's a better place than the BattleManager.
    /// 
    /// Bad static! Bad!
    /// </remarks>
    public static Ability SelectedAbility;

    // this class should probably also have access to a sound system. Can do
    // an object pool for audio components. Each pawn does not need one.

    /// <summary>
    /// Activate the ability. Should be overridden by subclass for the
    /// actual behavior.
    /// </summary>
    /// <param name="currentPawn"></param>
    /// <param name="targetPawn"></param>
    public virtual bool Activate(Pawn currentPawn, Pawn targetPawn)
    {
        Debug.Log("Ability not implemented!");
        return false;
    }
}
