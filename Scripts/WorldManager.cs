using UnityEngine;
using UnityEngine.UI;
using JaimesUtilities.AStarManual;

// This script is not refactorized.

public class WorldManager : MonoBehaviour
{
    public const float TEXTGLITCHTIME = 5f;

    public static WorldManager Instance { 
        get {
            if (instance == null) instance = FindObjectOfType<WorldManager>();
            return instance;
        } 
    }
    static WorldManager instance;

    public int sceneIndex;
    [HideInInspector] public int objectiveIndex;
    [HideInInspector] public bool isDead;

    public WorldSettings settings;
    public TMPro.TextMeshProUGUI glitchTextMesh;
    public GameObject loadingScreenObj;
    public GameObject deathScreenObj;
    public PathfindingManager pathfindingManager;

    private bool goingToNextLevel;
    private bool restartingLevel;
    private bool restartingGame;

    private void Awake() {
        ResetValues();
        
        GameManager.OnSceneLoad(sceneIndex);
        settings.ghostMaterial.SetColor("_Color", sceneIndex < 3 ? new Color(0f, 0f, 0f, 0.3f) : new Color(0f, 0f, 0f, 1f));

        UpdateScreenMessage();
    }
    private void FixedUpdate() {
        UpdateScreenMessage();

        deathScreenObj.SetActive(isDead);
        if (isDead) {
            deathScreenObj.transform.GetChild(0).GetComponent<RawImage>().color += new Color(0, 0, 0, 0.005f);
            deathScreenObj.transform.GetChild(1).GetComponent<RectTransform>().localPosition += Vector3.down * 1.5f;
            deathScreenObj.transform.GetChild(1).GetComponent<RectTransform>().localPosition += Vector3.down * 1.5f;
            if (deathScreenObj.transform.GetChild(1).GetComponent<RectTransform>().localPosition.y < -50) RestartLevel();
        }
    }

    private void Update() {
        if (goingToNextLevel) {
            ResetValues();
            GameManager.NextScene();
        }
        if (restartingLevel) {
            ResetValues();
            GameManager.RestartScene();
        }
        if (restartingGame) {
            ResetValues();
            GameManager.RestartGame();
        }

        if (!goingToNextLevel) { 
            if (GameManager.Paused) { 
                settings.screenMaterial.SetFloat("_GlitchStrength", glitchScreenStrength / 5f);
                settings.screenMaterial.SetFloat("_TextStrength", 0);
            }
            else {
                settings.screenMaterial.SetFloat("_GlitchStrength", glitchScreenStrength);
                settings.screenMaterial.SetFloat("_TextStrength", glitchTextStrength);
            }

            settings.soldierMaterial.SetFloat("_GlitchStrength", glitchSoldierStrength);
            settings.ghostMaterial.SetFloat("_GlitchStrength", glitchSoldierStrength);
        }
    }
    public void NextObjective() {
        objectiveIndex++;
    }
    public void NextLevel() {
        if (isDead) return;
        ResetValues();
        goingToNextLevel = true;
        loadingScreenObj.SetActive(true);
    }
    public void RestartLevel() {
        ResetValues();
        restartingLevel = true;
        loadingScreenObj.SetActive(true);
    }
    public void RestartGame() {
        ResetValues();
        restartingGame = true;
        loadingScreenObj.SetActive(true);
    }
    private void ResetValues() {
        settings.screenMaterial.SetFloat("_GlitchStrength", 0);
        settings.screenMaterial.SetFloat("_TextStrength", 0);
        settings.soldierMaterial.SetFloat("_GlitchStrength", 0);
        settings.ghostMaterial.SetFloat("_GlitchStrength", 0);
        glitchTextStrength = 0;
        glitchScreenStrength = 0;
        glitchSoldierStrength = 0;
        targetScreenGlitch = 0;
        targetSoldierGlitch = 0;
        textGlitchTimer = 0;
    }

    float glitchScreenStrength, glitchTextStrength, glitchSoldierStrength;
    static float targetScreenGlitch;
    static float targetSoldierGlitch;
    static public float textGlitchTimer;

    private void OnApplicationQuit() {
        ResetValues();
    }

    private void UpdateScreenMessage() {
        glitchSoldierStrength = Mathf.Lerp(glitchSoldierStrength, targetSoldierGlitch, 1f * Time.deltaTime);
        glitchScreenStrength = Mathf.Lerp(glitchScreenStrength, targetScreenGlitch, 1f * Time.deltaTime);
        glitchTextStrength = Mathf.Lerp(glitchTextStrength, textGlitchTimer > Time.time ? 1f : 0, 2f * Time.deltaTime);
    }

    public static void PlayScreenMessage(string message) {
        Instance.glitchTextMesh.text = message;
        textGlitchTimer = Time.time + TEXTGLITCHTIME;
    }
    public static void ExecuteScreenGlitch(float magnitude) {
        targetScreenGlitch = magnitude;
    }
    public static void SetSoldierGlitchStrength(float strength) {
        targetSoldierGlitch = strength;
    }
}
