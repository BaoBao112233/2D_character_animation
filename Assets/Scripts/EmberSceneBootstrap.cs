using UnityEngine;

/// <summary>
/// Place this on an empty GameObject in an empty scene.
/// Press Play — it builds the entire Ember prototype level from code.
/// No scene setup required.
/// </summary>
public class EmberSceneBootstrap : MonoBehaviour
{
    void Awake()
    {
        BuildScene();
    }

    void BuildScene()
    {
        // Camera
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam      = camGo.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor  = new Color(0.12f, 0.16f, 0.22f);
        camGo.AddComponent<AudioListener>();

        // Ground platform (long)
        BuildPlatform(new Vector2(0f,  -2.5f), new Vector2(30f, 0.5f), new Color(0.28f, 0.22f, 0.16f));
        // Extra platforms
        BuildPlatform(new Vector2(-5f,  0.0f), new Vector2(4f,  0.3f), new Color(0.35f, 0.27f, 0.18f));
        BuildPlatform(new Vector2( 4f,  1.2f), new Vector2(4f,  0.3f), new Color(0.35f, 0.27f, 0.18f));
        BuildPlatform(new Vector2( 0f,  2.5f), new Vector2(3f,  0.3f), new Color(0.35f, 0.27f, 0.18f));

        // Background gradient (simple colored quads)
        BuildBackground();

        // Ember character
        BuildEmber(new Vector2(0f, -2.0f));
    }

    void BuildEmber(Vector2 pos)
    {
        var go = new GameObject("Ember");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one;

        // Visual rig
        go.AddComponent<EmberVisualPlaceholder>();

        // Physics
        var rb            = go.AddComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col           = go.AddComponent<CapsuleCollider2D>();
        col.size          = new Vector2(0.40f, 1.10f);
        col.offset        = new Vector2(0f, 0.55f);

        // Prototype controller (no external assets needed)
        go.AddComponent<EmberProtoController>();
    }

    void BuildPlatform(Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject("Platform");
        go.transform.position = pos;
        go.layer = LayerMask.NameToLayer("Default");

        // Visual
        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sprite       = CreateRectSprite(size);
        sr.color        = color;
        sr.sortingOrder = -5;

        // Collider
        var bc      = go.AddComponent<BoxCollider2D>();
        bc.size     = size;
    }

    void BuildBackground()
    {
        // Sky
        var sky = new GameObject("Sky");
        sky.transform.position = new Vector3(0, 1, 10);
        var sr = sky.AddComponent<SpriteRenderer>();
        sr.sprite       = CreateRectSprite(new Vector2(60, 20));
        sr.color        = new Color(0.08f, 0.12f, 0.22f);
        sr.sortingOrder = -10;

        // Ground fog strip
        var fog = new GameObject("FogStrip");
        fog.transform.position = new Vector3(0, -2.0f, 5);
        var sr2 = fog.AddComponent<SpriteRenderer>();
        sr2.sprite      = CreateRectSprite(new Vector2(60, 1.5f));
        sr2.color       = new Color(0.18f, 0.22f, 0.30f, 0.5f);
        sr2.sortingOrder = -6;
    }

    static Sprite CreateRectSprite(Vector2 size)
    {
        int w = Mathf.Max(1, Mathf.RoundToInt(size.x * 20));
        int h = Mathf.Max(1, Mathf.RoundToInt(size.y * 20));
        var tex = new Texture2D(w, h);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 20f);
    }
}
