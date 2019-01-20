using System.Linq;
using FixMath.NET;
using Lockstep.Framework.Commands; 
using UnityEngine;
using UnityEngine.Experimental.Input;

public class UnityInput : MonoBehaviour
{
    public InputMaster controls;

    void Awake()
    {
        controls.Player.SpawnUnit.performed += SpawnUnitOnPerformed;
        controls.Player.MoveUnits.performed += NavigateUnitsOnPerformed;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    private void SpawnUnitOnPerformed(InputAction.CallbackContext obj)
    {                                    
        var pos = GetWorldPos(Mouse.current.position.ReadValue());  
        FindObjectOfType<EntitySpawner>().Spawn(pos);
    }

    private void NavigateUnitsOnPerformed(InputAction.CallbackContext obj)
    {
        var pos = GetWorldPos(Mouse.current.position.ReadValue());
        LockstepNetwork.Instance.SendInput(new NavigateCommand
        {
            Destination = new BEPUutilities.Vector2(pos.X, pos.Y),
            EntityIds = Contexts.sharedInstance.game.GetEntities().Select(entity => entity.id.value).ToArray()
        });
    }

    public static BEPUutilities.Vector2 GetWorldPos(Vector2 screenPos)
    {
        var ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit))
        {
            return new BEPUutilities.Vector2((Fix64)hit.point.x, (Fix64)hit.point.z);
        }
        var hitPoint = ray.origin - ray.direction * (ray.origin.y / ray.direction.y);
        return new BEPUutilities.Vector2((Fix64)hitPoint.x, (Fix64)hitPoint.z);
    } 
}
