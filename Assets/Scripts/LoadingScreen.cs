using UnityEngine;
using WorldGenerator;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private TerrainGenerator terrainGenerator;

    [SerializeField]
    private Image Background;

    [SerializeField]
    private Text Text;

    private bool loaded = false;
    private Color textFirst;
    private Color textSecond;

    private void Awake()
    {
        textFirst = Text.color;
        textSecond = new Color(0.5f, 0.7f, 0.9f);
        Text.color = new Color(textFirst.r, textFirst.g, textFirst.b, 0);
    }

    private void Start()
    {
        terrainGenerator.OnInitialTerrainLoaded += TerrainGenerator_OnInitialTerrainLoaded;
        float seed = terrainGenerator.worldSettings.globalBiomeSettings.seed;

        Text.text = "Loading terrain seed " + seed + "\n\nPlease wait...";
        Background.gameObject.SetActive(true);
        Text.gameObject.SetActive(true);
        startFade = Time.time + initialWait;
    }

    private void TerrainGenerator_OnInitialTerrainLoaded()
    {
        loaded = true;
        startFade = Time.time;
    }

    private float startFade;

    private bool fadeIn = true;

    [SerializeField]
    private float fadeSpeed = 1f;

    [SerializeField]
    private float initialWait = 5f;

    private void Update()
    {
        if (!loaded)
        {
            if (fadeIn)
            {
                float t = Mathf.Clamp01((Time.time - startFade) / (1 / fadeSpeed));
                float a = Mathf.Lerp(0f, 1f, t);
                Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, a);
                if (t >= 1f)
                {
                    textFirst = Text.color;
                    fadeIn = false;
                }
            }
            else
            {
                Text.color = Color.Lerp(textFirst, textSecond, Mathf.PingPong(Time.time, 1));
            }
        }
        else
        {
            float t = Mathf.Clamp01((Time.time - startFade) / (1 / fadeSpeed));
            float a = Mathf.Lerp(1f, 0f, t);
            Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, a);
            if (t >= 1f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
