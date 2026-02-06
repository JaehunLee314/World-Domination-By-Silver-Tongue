using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using SilverTongue.Data;

public class StartAndInfoSceneComposer
{
    [MenuItem("Tools/Compose Start Scene")]
    public static void ComposeStartScene()
    {
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Debug.Log($"[StartAndInfoSceneComposer] Composing Start Scene in: {activeScene.name}");

        // === 1. Create GameManager ===
        var gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
            gmObj = new GameObject("GameManager");

        var gm = gmObj.GetComponent<SilverTongue.GameManager>();
        if (gm == null)
            gm = gmObj.AddComponent<SilverTongue.GameManager>();

        // Load character SOs
        var kai = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/Protagonist_Kai.asset");
        var reina = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/Rival_Reina.asset");
        var zarvoth = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/Boss_DemonKing.asset");

        var gmSo = new SerializedObject(gm);
        var charProp = gmSo.FindProperty("availableCharacters");
        charProp.arraySize = 3;
        charProp.GetArrayElementAtIndex(0).objectReferenceValue = kai;
        charProp.GetArrayElementAtIndex(1).objectReferenceValue = reina;
        charProp.GetArrayElementAtIndex(2).objectReferenceValue = zarvoth;
        gmSo.ApplyModifiedProperties();

        // === 2. Instantiate StartCanvas Prefab ===
        DestroyIfExists("StartCanvas");
        DestroyIfExists("StartSceneManager");

        var startCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/StartScene/StartCanvas.prefab");
        if (startCanvasPrefab == null)
        {
            Debug.LogError("[StartAndInfoSceneComposer] StartCanvas prefab not found! Run 'Tools/Build Start & Info UI Prefabs' first.");
            return;
        }
        var startCanvasObj = (GameObject)PrefabUtility.InstantiatePrefab(startCanvasPrefab);

        // === 3. Create StartSceneManager ===
        var mgrObj = new GameObject("StartSceneManager");
        var mgr = mgrObj.AddComponent<SilverTongue.StartScene.StartSceneManager>();

        // === 4. Wire SerializedFields ===
        var mgrSo = new SerializedObject(mgr);
        mgrSo.FindProperty("startView").objectReferenceValue = startCanvasObj.GetComponent<SilverTongue.StartScene.StartView>();
        mgrSo.ApplyModifiedProperties();

        // Wire StartView fields
        var startView = startCanvasObj.GetComponent<SilverTongue.StartScene.StartView>();
        if (startView != null)
        {
            var viewSo = new SerializedObject(startView);
            viewSo.FindProperty("titleText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(startCanvasObj, "Canvas/TitleText");
            viewSo.FindProperty("startButton").objectReferenceValue = FindInChildren<Button>(startCanvasObj, "Canvas/StartButton");
            viewSo.ApplyModifiedProperties();
        }

        // === 5. Save Scene ===
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);

        Debug.Log("[StartAndInfoSceneComposer] Start Scene composed successfully!");
    }

    [MenuItem("Tools/Compose DummyInfoGathering Scene")]
    public static void ComposeDummyInfoGatheringScene()
    {
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Debug.Log($"[StartAndInfoSceneComposer] Composing DummyInfoGathering Scene in: {activeScene.name}");

        // === 1. Instantiate Canvas Prefab ===
        DestroyIfExists("DummyInfoGatheringCanvas");
        DestroyIfExists("DummyInfoGatheringSceneManager");

        var canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DummyInfoGatheringScene/DummyInfoGatheringCanvas.prefab");
        if (canvasPrefab == null)
        {
            Debug.LogError("[StartAndInfoSceneComposer] DummyInfoGatheringCanvas prefab not found! Run 'Tools/Build Start & Info UI Prefabs' first.");
            return;
        }
        var canvasObj = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab);

        // === 2. Create DummyInfoGatheringSceneManager ===
        var mgrObj = new GameObject("DummyInfoGatheringSceneManager");
        var mgr = mgrObj.AddComponent<SilverTongue.DummyInfoGatheringScene.DummyInfoGatheringSceneManager>();

        // === 3. Load and wire items + opponent ===
        var diary = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Evidence_SecretDiary.asset");
        var photo = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Evidence_PhotoAlbum.asset");
        var ramen = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Evidence_RamenRecipe.asset");
        var tsundere = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Skill_TsundereLogic.asset");
        var logic = AssetDatabase.LoadAssetAtPath<ItemSO>("Assets/ScriptableObjects/Items/Skill_LogicalDeduction.asset");
        var opponent = AssetDatabase.LoadAssetAtPath<CharacterSO>("Assets/ScriptableObjects/Characters/Boss_DemonKing.asset");

        var mgrSo = new SerializedObject(mgr);

        var itemsProp = mgrSo.FindProperty("itemsToProvide");
        itemsProp.arraySize = 5;
        itemsProp.GetArrayElementAtIndex(0).objectReferenceValue = diary;
        itemsProp.GetArrayElementAtIndex(1).objectReferenceValue = photo;
        itemsProp.GetArrayElementAtIndex(2).objectReferenceValue = ramen;
        itemsProp.GetArrayElementAtIndex(3).objectReferenceValue = tsundere;
        itemsProp.GetArrayElementAtIndex(4).objectReferenceValue = logic;

        mgrSo.FindProperty("opponent").objectReferenceValue = opponent;
        mgrSo.FindProperty("view").objectReferenceValue = canvasObj.GetComponent<SilverTongue.DummyInfoGatheringScene.DummyInfoGatheringView>();
        mgrSo.ApplyModifiedProperties();

        // === 4. Wire DummyInfoGatheringView fields ===
        var view = canvasObj.GetComponent<SilverTongue.DummyInfoGatheringScene.DummyInfoGatheringView>();
        if (view != null)
        {
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("titleText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(canvasObj, "Canvas/TitleText");
            viewSo.FindProperty("itemListText").objectReferenceValue = FindInChildren<TMPro.TextMeshProUGUI>(canvasObj, "Canvas/ItemListText");
            viewSo.FindProperty("proceedButton").objectReferenceValue = FindInChildren<Button>(canvasObj, "Canvas/ProceedButton");
            viewSo.ApplyModifiedProperties();
        }

        // === 5. Save Scene ===
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);

        Debug.Log("[StartAndInfoSceneComposer] DummyInfoGathering Scene composed successfully!");
    }

    [MenuItem("Tools/Setup Build Settings")]
    public static void SetupBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/StartScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/InfoGatheringScenes/DummyInfoGatheringScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/BattleScene.unity", true),
        };
        Debug.Log("[StartAndInfoSceneComposer] Build Settings configured: StartScene(0), DummyInfoGatheringScene(1), BattleScene(2)");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    static void DestroyIfExists(string name)
    {
        var obj = GameObject.Find(name);
        if (obj != null) Object.DestroyImmediate(obj);
    }

    static T FindInChildren<T>(GameObject root, string path) where T : Component
    {
        var tr = root.transform.Find(path);
        if (tr == null)
        {
            Debug.LogWarning($"[StartAndInfoSceneComposer] Could not find: {path} in {root.name}");
            return null;
        }
        return tr.GetComponent<T>();
    }
}
