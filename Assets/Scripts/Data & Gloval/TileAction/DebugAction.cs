using UnityEngine;

[CreateAssetMenu(fileName = "New LogAction", menuName = "Game/Actions/Debug Log")]
public class DebugLogAction : TileAction
{
    [TextArea]
    public string message = "이벤트가 발생했습니다.";

    public override void Execute(Node node, GameObject instigator)
    {
        string instigatorName = instigator != null ? instigator.name : "System";
        
        Debug.Log($"[Action] {instigatorName} -> ({node.X}, {node.Y}) 타일: {message}");
    }
}