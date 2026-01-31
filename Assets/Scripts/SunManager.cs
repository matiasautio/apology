using UnityEngine;

public class SunManager : MonoBehaviour
{

    [SerializeField] Player player;
    [SerializeField] GameObject sun;
    [SerializeField] float sunOffset = 1f;
    [SerializeField] float minSunY = -3f;
    [SerializeField] float maxSunY = 6f;
    [SerializeField] float sunLerpSpeed = 2f;
    // Background
    [SerializeField] Camera mainCamera;
    [SerializeField] Color nightColor = new Color(0.05f, 0.05f, 0.15f);
    [SerializeField] Color dayColor   = new Color(0.4f, 0.7f, 1f);
    [SerializeField] float bgLerpSpeed = 1.5f;

    private float targetMental;

    void OnEnable()
    {
        player.OnMentalStateChanged += HandleMentalChanged;
        HandleMentalChanged(player.MentalState);
    }

    void OnDisable()
    {
        player.OnMentalStateChanged -= HandleMentalChanged;
    }

    void HandleMentalChanged(float value)
    {
        Debug.Log("Environment updating");
        targetMental = value;
    }
    void Update()
    {
        UpdateEnvironment(targetMental);
    }
    void UpdateEnvironment(float mentalState)
    {
        UpdateSun(mentalState);
        UpdateBackground(mentalState);
    }
    void UpdateSun(float mentalState)
    {
        float targetY = Remap(
            mentalState,
            0f, 10f,
            minSunY, maxSunY
        );
        Vector3 pos = sun.transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * sunLerpSpeed);
        //sun.transform.position = pos;
        sun.transform.position = new Vector2(mainCamera.transform.position.x + sunOffset, pos.y);
    }
    void UpdateBackground(float mentalState)
    {
        float t = Mathf.InverseLerp(0f, 10f, mentalState);

        Color target = Color.Lerp(nightColor, dayColor, t);
        mainCamera.backgroundColor =
            Color.Lerp(mainCamera.backgroundColor, target, Time.deltaTime * bgLerpSpeed);
    }

    public static float Remap(
        float value,
        float inMin,
        float inMax,
        float outMin,
        float outMax)
    {
        float t = Mathf.InverseLerp(inMin, inMax, value);
        return Mathf.Lerp(outMin, outMax, t);
    }
}
