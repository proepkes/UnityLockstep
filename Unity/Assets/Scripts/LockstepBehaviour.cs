using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LockstepBehaviour : MonoBehaviour
{
    public virtual void OnStart() { }
    public virtual void OnSimulate(){ }
}
