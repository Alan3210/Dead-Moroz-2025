using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public const int TOTAL_LANES = 3;
    public const float LANE_WIDTH = 3f;

    public static Vector3 GetLanePosition(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= TOTAL_LANES)
        {
            Debug.LogWarning($"Invalid lane index: {laneIndex}. Clamping to valid range.");
            laneIndex = Mathf.Clamp(laneIndex, 0, TOTAL_LANES - 1);
        }

        float xPosition = (laneIndex - 1) * LANE_WIDTH;
        return new Vector3(xPosition, 0, 0);
    }

    public static int GetMiddleLaneIndex()
    {
        return 1;
    }
}
