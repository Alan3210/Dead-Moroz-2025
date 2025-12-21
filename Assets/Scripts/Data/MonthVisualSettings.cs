using UnityEngine;

[CreateAssetMenu(fileName = "MonthVisualSettings", menuName = "Dead Moroz/Month Visual Settings")]
public class MonthVisualSettings : ScriptableObject
{
    [Header("Month Info")]
    public string monthName;
    public int monthNumber;

    [Header("Skybox")]
    public Material skyboxMaterial;

    [Header("Ambient Lighting")]
    public Color ambientColor = new Color(0.5f, 0.5f, 0.5f);
    [Range(0f, 2f)]
    public float ambientIntensity = 1f;

    [Header("Fog Settings")]
    public bool useFog = true;
    public Color fogColor = Color.white;
    [Range(0f, 0.1f)]
    public float fogDensity = 0.01f;

    [Header("Directional Light")]
    public Color lightColor = Color.white;
    [Range(0f, 2f)]
    public float lightIntensity = 1f;

    [Header("Visual Theme")]
    [TextArea(2, 4)]
    public string themeDescription;
}
