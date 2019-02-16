using System.Linq;       
using Entitas;
using Entitas.Unity;
using FixMath.NET;
using Lockstep.Game.Commands;          
using UnityEngine;                      

public class UnityInput : MonoBehaviour
{
    private GameEntity selection;

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
        if (Input.GetMouseButtonDown(0))
        {
            LeftClick();
        }

        if (Input.GetMouseButton(1))
        {   
            var pos = GetWorldPos(Input.mousePosition);
            FindObjectOfType<RTSEntitySpawner>().Spawn(pos);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            var e = Contexts.sharedInstance.game
                .GetEntities(GameMatcher.LocalId)
                .Where(entity => entity.actorId.value == RTSNetworkedSimulation.Instance.Simulation.LocalActorId)
                .Select(entity => entity.id.value).ToArray();      

            RTSNetworkedSimulation.Instance.Execute(new NavigateCommand
            {
                Destination = GetWorldPos(Input.mousePosition),
                Selection = e
            });
        }

        if (selection != null)
        {
            foreach (var n in selection.neighbors.neighborsECS.Where(e => e != null))
            {
                var pos = new Vector3((float) selection.position.value.X, 1, (float) selection.position.value.Y);
                var dir = new Vector3((float) n.position.value.X, 1, (float) n.position.value.Y) - pos;

                //first nearest neighbor is the entity itself.
                //also if a neighbor is in the exact same position, the direction is zero too (game logic should probably avoid entities on the exact same position).
                if (dir != Vector3.zero)
                {    
                    DrawArrow.ForDebug(pos, dir, Color.red);
                }
            }
        }
    }

    public void LeftClick()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition); 

        if (Physics.Raycast(ray, out var hit, 1000)) 
        {
            if (hit.transform.gameObject.GetEntityLink() != null) //Did the player click a selectable object?
            {
                if (hit.transform.gameObject.GetEntityLink().entity is GameEntity entity)
                {
                    selection = entity;
                }
            }
        }
    }
}

public static class DrawArrow
{
    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);
        DrawArrowEnd(true, pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle);
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);
        DrawArrowEnd(true, pos, direction, color, arrowHeadLength, arrowHeadAngle);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction);
        DrawArrowEnd(false, pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction, color);
        DrawArrowEnd(false, pos, direction, color, arrowHeadLength, arrowHeadAngle);
    }

    private static void DrawArrowEnd(bool gizmos, Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
        Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
        if (gizmos)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, up * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, down * arrowHeadLength);
        }
        else
        {
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, up * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, down * arrowHeadLength, color);
        }
    }
}