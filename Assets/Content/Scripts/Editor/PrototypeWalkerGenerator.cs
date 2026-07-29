#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// Builds a "Prototype"-style primitive walker prefab (generic mechanical hexapod,
// not a copy of any game's art): a body + 6 multi-jointed legs, each with a hip and
// 4 knees (5 segments), correctly arched. Every segment gets a ProceduralBone so the
// whole thing animates with the same rig as Human. Run from the menu:
//   Tools > Prototype Walker > Build Prefab
public static class PrototypeWalkerGenerator
{
    private const string Dir = "Assets/Content/PrototypeWalker";
    private const string PrefabPath = Dir + "/PrototypeWalker.prefab";

    // Leg shape (see the tuned arch). rest angles are LOCAL, in degrees; a segment's
    // default direction is "down" and restRotation rotates it. This makes the thigh
    // lift up-and-out and each knee curl the leg back down to the foot.
    private static readonly float[] LegRest = { 135f, -30f, -45f, -52f, -50f };
    private static readonly float[] LegLen  = { 1.10f, 0.95f, 0.85f, 0.75f, 0.55f };

    private static Sprite _square;
    private static Sprite _circle;

    [MenuItem("Tools/Prototype Walker/Build Prefab")]
    public static void Build()
    {
        Directory.CreateDirectory(Dir);
        _square = MakeSprite("square", circle: false);
        _circle = MakeSprite("circle", circle: true);

        var root = new GameObject("PrototypeWalker");
        root.AddComponent<ProceduralRig>();
        root.AddComponent<Animator>(); // empty; drop an AnimatorController here to animate

        // ---- Body (a ProceduralBone so it can bob/tilt) --------------------------
        var body = Bone("Body", root.transform, Vector2.zero, 0f);
        Rect(body.transform, new Vector2(0f, 0f), new Vector2(3.6f, 1.4f), 0f,
             new Color(0.72f, 0.55f, 0.85f), order: 0);           // torso
        Rect(body.transform, new Vector2(1.7f, 0.15f), new Vector2(1.0f, 0.9f), -0.02f,
             new Color(0.50f, 0.13f, 0.13f), order: 1);           // "saddle" block

        // ---- Head / neck on top --------------------------------------------------
        var neck = Bone("Neck", body.transform, new Vector2(1.6f, 0.7f), 25f);
        Rect(neck.transform, new Vector2(0f, 0.55f), new Vector2(0.28f, 1.1f), 0f,
             new Color(0.80f, 0.65f, 0.20f), order: 2);
        var head = Bone("Head", neck.transform, new Vector2(0f, 1.1f), 20f);
        Circle(head.transform, Vector2.zero, 0.9f, 0f, new Color(0.50f, 0.13f, 0.13f), order: 3);

        // ---- 6 legs: interleaved near / far along the body underside -------------
        float hipY = -0.55f;
        float[] hipX = { 1.5f, 0.9f, 0.3f, -0.3f, -0.9f, -1.5f };
        for (int i = 0; i < hipX.Length; i++)
        {
            bool near = (i % 2 == 0);
            float x = hipX[i];
            float lean = -x * 8f;                       // front legs lean forward, back legs back
            Color seg  = near ? new Color(0.56f, 0.56f, 0.60f) : new Color(0.32f, 0.32f, 0.36f);
            Color knee = near ? new Color(0.82f, 0.82f, 0.88f) : new Color(0.50f, 0.50f, 0.56f);
            int order  = near ? 2 : -2;                 // near in front of body, far behind
            BuildLeg(body.transform, $"Leg{i}_{(near ? "N" : "F")}",
                     new Vector2(x, hipY), lean, seg, knee, order);
        }

        // ---- Save prefab, keep an instance in the open scene ---------------------
        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.UserAction);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log($"[PrototypeWalker] Built prefab at {PrefabPath}");
    }

    // One leg = 5 chained bones (hip + 4 knees).
    private static void BuildLeg(Transform body, string name, Vector2 hip, float lean,
                                 Color segColor, Color kneeColor, int order)
    {
        Transform parent = body;
        Vector2 pos = hip;
        for (int i = 0; i < LegRest.Length; i++)
        {
            float rot = LegRest[i] + (i == 0 ? lean : 0f);
            var bone = Bone($"{name}_Seg{i}", parent, pos, rot);

            float L = LegLen[i];
            float w = Mathf.Lerp(0.28f, 0.15f, i / 4f);
            Rect(bone.transform, new Vector2(0f, -L * 0.5f), new Vector2(w, L), 0.01f, segColor, order);
            Circle(bone.transform, Vector2.zero, w * 1.6f, -0.01f, kneeColor, order + 1); // knee disc

            parent = bone.transform;
            pos = new Vector2(0f, -L);            // next joint sits at this segment's far end
        }
        // small foot
        Circle(parent, new Vector2(0f, -LegLen[LegLen.Length - 1]), 0.22f, 0f,
               new Color(0.20f, 0.20f, 0.23f), order);
    }

    // ---- helpers ----------------------------------------------------------------

    private static GameObject Bone(string name, Transform parent, Vector2 localPos, float restRot)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, restRot); // visible rest pose in edit mode
        var pb = go.AddComponent<ProceduralBone>();
        pb.restRotation = restRot;                                      // and driven the same way at play
        return go;
    }

    private static void Rect(Transform parent, Vector2 offset, Vector2 size, float z, Color color, int order)
        => Visual(parent, offset, size, z, _square, color, order);

    private static void Circle(Transform parent, Vector2 offset, float diameter, float z, Color color, int order)
        => Visual(parent, offset, new Vector2(diameter, diameter), z, _circle, color, order);

    private static void Visual(Transform parent, Vector2 offset, Vector2 size, float z, Sprite sprite, Color color, int order)
    {
        var go = new GameObject("Visual");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(offset.x, offset.y, z);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
    }

    private static Sprite MakeSprite(string name, bool circle, int size = 64)
    {
        string path = $"{Dir}/{name}.png";
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size * 0.5f;
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool on = true;
                if (circle)
                {
                    float dx = x + 0.5f - r, dy = y + 0.5f - r;
                    on = dx * dx + dy * dy <= r * r;
                }
                px[y * size + x] = on ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
        tex.SetPixels32(px);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = size;   // 1 unit per sprite -> scale via transform
        imp.filterMode = FilterMode.Point;
        imp.mipmapEnabled = false;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
#endif
