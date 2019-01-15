using FixMath.NET;
using Lockstep.Framework.Services;
using UnityEngine;

public class MouseInput : MonoBehaviour
{              
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
                                           
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var pos = GetWorldPos(Input.mousePosition);
            //LockstepNetwork.Instance.SendCommand(CommandTag.Navigate, new MovePacket{ AgentIDs = new ushort[0], PosX = pos.X, PosY = pos.Y});
        }
    }
}
