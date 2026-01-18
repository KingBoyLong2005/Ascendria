using StarterAssets;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemExtraJump", menuName = "Item/ExtraJump")]
public class ItemExtraJump : Item
{
    [Header("Stats")]
    [Tooltip("Số lần jump thêm khi đã unlock air jump")]
    public int extraJumpCount = 1;

    public override void Apply(int multiplier = 1)
    {
        // Lần đầu: chỉ unlock air jump
        if (!PlayerMoveManager.Instance.airJumpActive)
        {
            PlayerMoveManager.Instance.airJumpActive = true;
            PlayerMoveManager.Instance.maxAirJump = 1; // mốc cơ bản sau khi unlock

            Debug.Log("<color=green>[Item]</color> Air Jump unlocked");
            return;
        }

        // Những lần sau: tăng số lần nhảy
        int totalJumps = extraJumpCount * multiplier;
        PlayerMoveManager.Instance.maxAirJump += totalJumps;

        Debug.Log($"<color=green>[Item]</color> +{totalJumps} Extra Air Jump (x{multiplier})");
    }

    public override void Remove(int multiplier = 1)
    {
        var move = PlayerMoveManager.Instance;

        if (!move.airJumpActive)
            return;

        int totalJumps = extraJumpCount * multiplier;
        move.maxAirJump -= totalJumps;

        // Không cho tụt xuống dưới 1 nếu đã unlock
        move.maxAirJump = Mathf.Max(1, move.maxAirJump);

        Debug.Log($"<color=red>[Item]</color> Removed {totalJumps} Air Jump");
    }
}
