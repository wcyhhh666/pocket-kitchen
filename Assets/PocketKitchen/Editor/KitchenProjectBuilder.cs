using System;
using System.IO;
using PocketKitchen;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class KitchenProjectBuilder
{
    public const string ScenePath = "Assets/PocketKitchen/Scenes/PocketKitchen.unity";
    private static string Workspace => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    [MenuItem("Tools/PocketKitchen/1 - Build Kitchen Scene")]
    public static void BuildScene()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before rebuilding the scene.");
        Directory.CreateDirectory("Assets/PocketKitchen/Scenes");
        Directory.CreateDirectory("Assets/PocketKitchen/Materials");
        Directory.CreateDirectory("Assets/PocketKitchen/Prefabs");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("PocketKitchen");
        var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; camGo.transform.SetParent(root.transform);
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 6.8f;
        cam.transform.position = new Vector3(0, 13.8f, -12.8f); cam.transform.LookAt(new Vector3(0, 0, 0.35f));
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.16f, 0.29f, 0.29f); cam.nearClipPlane = 0.1f; cam.farClipPlane = 80;
        camGo.AddComponent<AudioListener>();
        var sunGo = new GameObject("Warm Sun"); sunGo.transform.SetParent(root.transform); sunGo.transform.rotation = Quaternion.Euler(48, -32, 0);
        Light sun = sunGo.AddComponent<Light>(); sun.type = LightType.Directional; sun.color = new Color(1, 0.94f, 0.85f); sun.intensity = 0.75f; sun.shadows = LightShadows.Soft; sun.shadowStrength = 0.65f; sun.shadowBias = 0.04f;
        var fillGo = new GameObject("Soft Fill"); fillGo.transform.SetParent(root.transform); fillGo.transform.rotation = Quaternion.Euler(65, 150, 0);
        Light fill = fillGo.AddComponent<Light>(); fill.type = LightType.Directional; fill.color = new Color(0.80f, 0.94f, 1); fill.intensity = 0.16f;
        RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.38f, 0.43f, 0.40f); RenderSettings.skybox = null; RenderSettings.fog = false; RenderSettings.sun = sun;
        QualitySettings.shadows = ShadowQuality.All; QualitySettings.shadowDistance = 40; QualitySettings.shadowResolution = ShadowResolution.High; QualitySettings.antiAliasing = 4;
        var room = new GameObject("Kitchen Environment"); room.transform.SetParent(root.transform);
        KitchenVisuals.BuildRoom(room.transform);
        var stationRoot = new GameObject("Workstations"); stationRoot.transform.SetParent(root.transform);
        var stations = new[]
        {
            KitchenVisuals.BuildStation(StationKind.Tomatoes, new Vector3(-4.3f, 0, 3.15f), stationRoot.transform),
            KitchenVisuals.BuildStation(StationKind.Lettuce, new Vector3(-1.5f, 0, 3.15f), stationRoot.transform),
            KitchenVisuals.BuildStation(StationKind.Plates, new Vector3(1.3f, 0, 3.15f), stationRoot.transform),
            KitchenVisuals.BuildStation(StationKind.Delivery, new Vector3(4.1f, 0, 3.15f), stationRoot.transform),
            KitchenVisuals.BuildStation(StationKind.Chopping, new Vector3(-5.7f, 0, 0), stationRoot.transform),
            KitchenVisuals.BuildStation(StationKind.Stove, new Vector3(5.7f, 0, 0), stationRoot.transform),
            KitchenVisuals.BuildStation(StationKind.Trash, new Vector3(-4.8f, 0, -3.6f), stationRoot.transform)
        };
        ChefController chef = KitchenVisuals.BuildChef(root.transform); chef.transform.rotation = Quaternion.Euler(0, 180, 0);
        GameObject manager = new GameObject("Game Director"); manager.transform.SetParent(root.transform);
        KitchenGame game = manager.AddComponent<KitchenGame>(); game.chef = chef; game.Stations = stations;
        manager.AddComponent<KitchenHUD>();
        foreach (Material mat in KitchenVisuals.AllMaterials)
        {
            if (AssetDatabase.Contains(mat)) continue;
            string assetPath = "Assets/PocketKitchen/Materials/" + mat.name + ".mat";
            Material saved = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (saved == null) AssetDatabase.CreateAsset(mat, assetPath);
            else
            {
                EditorUtility.CopySerialized(mat, saved);
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
                    if (renderer.sharedMaterial == mat) renderer.sharedMaterial = saved;
            }
        }
        PrefabUtility.SaveAsPrefabAsset(chef.gameObject, "Assets/PocketKitchen/Prefabs/Chef.prefab");
        foreach (var station in stations) PrefabUtility.SaveAsPrefabAsset(station.gameObject, "Assets/PocketKitchen/Prefabs/" + station.kind + ".prefab");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "Pocket Kitchen"; PlayerSettings.companyName = "PocketKitchen";
        PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true; PlayerSettings.runInBackground = true;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = chef.gameObject;
        SceneView.RepaintAll();
        Debug.Log("[PocketKitchen] Scene created: " + ScenePath);
    }

    [MenuItem("Tools/PocketKitchen/2 - Run Gameplay Smoke Test")]
    public static void RunSmokeTest()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before starting smoke test.");
        EditorSceneManager.OpenScene(ScenePath);
        SessionState.SetBool("PocketKitchen.RunSmoke", true);
        EditorApplication.isPlaying = true;
    }

    [Serializable] private class BuildEvidence
    {
        public string result;
        public string executable;
        public string unityVersion;
        public string builtAt;
        public int errors;
        public int warnings;
        public string failure;
    }

    [MenuItem("Tools/PocketKitchen/3 - Build Windows Game")]
    public static void BuildWindows()
    {
        string destination = Path.Combine(Workspace, "Builds/PocketKitchen-v1/PocketKitchen.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        Directory.CreateDirectory(Path.Combine(Workspace, "Outputs"));
        var evidence = new BuildEvidence { executable = destination, unityVersion = Application.unityVersion, builtAt = DateTimeOffset.Now.ToString("o") };
        try
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before building.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = destination,
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
            });
            evidence.result = report.summary.result.ToString(); evidence.errors = report.summary.totalErrors; evidence.warnings = report.summary.totalWarnings;
        }
        catch (Exception exception) { evidence.result = "Failed"; evidence.failure = exception.ToString(); Debug.LogException(exception); }
        File.WriteAllText(Path.Combine(Workspace, "Outputs/build-report.json"), JsonUtility.ToJson(evidence, true));
        Debug.Log("[PocketKitchen] Windows build: " + evidence.result);
    }
}
