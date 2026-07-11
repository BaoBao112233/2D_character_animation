using UnityEngine;

/// <summary>
/// Builds a visible placeholder body for Ember using Unity primitives.
/// Attach to an empty GameObject "Ember_Root".
/// Remove this component once you import real sprites.
/// </summary>
public class EmberVisualPlaceholder : MonoBehaviour
{
    [Header("Colors — Ember palette")]
    [SerializeField] Color _hairColor    = new Color(0.85f, 0.35f, 0.05f); // burnt orange
    [SerializeField] Color _skinColor    = new Color(0.95f, 0.75f, 0.60f);
    [SerializeField] Color _coatColor    = new Color(0.35f, 0.22f, 0.10f); // dark leather
    [SerializeField] Color _shirtColor   = new Color(0.18f, 0.42f, 0.18f); // dark green
    [SerializeField] Color _saberColor   = new Color(0.20f, 0.80f, 1.00f); // cyan glow
    [SerializeField] Color _blasterColor = new Color(0.70f, 0.50f, 0.20f); // brass

    // Exposed bones so animator/other scripts can reference them
    [HideInInspector] public Transform BoneHips;
    [HideInInspector] public Transform BoneTorso;
    [HideInInspector] public Transform BoneHead;
    [HideInInspector] public Transform BoneHair;
    [HideInInspector] public Transform BoneUpperArmR;
    [HideInInspector] public Transform BoneLowerArmR;
    [HideInInspector] public Transform BoneUpperArmL;
    [HideInInspector] public Transform BoneLowerArmL;
    [HideInInspector] public Transform BoneUpperLegR;
    [HideInInspector] public Transform BoneLowerLegR;
    [HideInInspector] public Transform BoneUpperLegL;
    [HideInInspector] public Transform BoneLowerLegL;
    [HideInInspector] public Transform BoneWeaponR;  // Steam-Blaster
    [HideInInspector] public Transform BoneWeaponL;  // Mana-Saber
    [HideInInspector] public Transform BoneCoatTail;

    // Saber glow renderer for pulsing
    SpriteRenderer _saberRenderer;
    float _glowTimer;

    void Awake()
    {
        Build();
    }

    void Update()
    {
        // Mana-Saber cyan glow pulse
        if (_saberRenderer != null)
        {
            _glowTimer += Time.deltaTime * 2.5f;
            float alpha = 0.6f + 0.4f * Mathf.Sin(_glowTimer);
            _saberRenderer.color = new Color(_saberColor.r, _saberColor.g, _saberColor.b, alpha);
        }
    }

    // ─────────────────────────────────────────────
    //  BUILD HIERARCHY
    // ─────────────────────────────────────────────
    void Build()
    {
        // Root offset so character stands above ground
        transform.localPosition = Vector3.zero;

        // ── HIPS (pivot)
        BoneHips = MakeBone("Hips", transform,         Vector2.zero);

        // ── TORSO
        BoneTorso = MakeBone("Torso", BoneHips,        new Vector2(0,  0.45f));
        MakeBox("Shirt",    BoneTorso, new Vector2(0, 0.10f), new Vector2(0.30f, 0.40f), _shirtColor, order:0);
        MakeBox("Coat",     BoneTorso, new Vector2(0, 0.05f), new Vector2(0.38f, 0.52f), _coatColor,  order:-1);

        // ── HEAD
        BoneHead = MakeBone("Head", BoneTorso,         new Vector2(0,  0.48f));
        MakeBox("Face",     BoneHead, Vector2.zero,     new Vector2(0.24f, 0.28f), _skinColor,  order:2);

        // ── HAIR (fluffy top)
        BoneHair = MakeBone("Hair", BoneHead,          new Vector2(0,  0.17f));
        MakeBox("Hair_L",   BoneHair, new Vector2(-0.05f, 0), new Vector2(0.18f, 0.18f), _hairColor, order:3);
        MakeBox("Hair_R",   BoneHair, new Vector2(0.05f, 0),  new Vector2(0.18f, 0.18f), _hairColor, order:3);

        // ── COAT TAIL (flows behind hips)
        BoneCoatTail = MakeBone("CoatTail", BoneHips, new Vector2(0, -0.10f));
        MakeBox("CoatTailBody", BoneCoatTail, new Vector2(0, -0.15f), new Vector2(0.28f, 0.30f), _coatColor, order:-1);

        // ── RIGHT ARM (Steam-Blaster side)
        BoneUpperArmR = MakeBone("UpperArm_R", BoneTorso,  new Vector2( 0.24f, 0.28f));
        MakeBox("UpperArmR", BoneUpperArmR, new Vector2(0.12f, 0), new Vector2(0.24f, 0.12f), _coatColor, order:1);
        BoneLowerArmR = MakeBone("LowerArm_R", BoneUpperArmR, new Vector2(0.24f, 0));
        MakeBox("LowerArmR", BoneLowerArmR, new Vector2(0.10f, 0), new Vector2(0.20f, 0.11f), _skinColor, order:1);
        BoneWeaponR   = MakeBone("SteamBlaster", BoneLowerArmR, new Vector2(0.22f, 0));
        MakeBox("Blaster", BoneWeaponR, new Vector2(0.08f, 0), new Vector2(0.20f, 0.09f), _blasterColor, order:2);

        // ── LEFT ARM (Mana-Saber side)
        BoneUpperArmL = MakeBone("UpperArm_L", BoneTorso, new Vector2(-0.24f, 0.28f));
        MakeBox("UpperArmL", BoneUpperArmL, new Vector2(-0.12f, 0), new Vector2(0.24f, 0.12f), _coatColor, order:1);
        BoneLowerArmL = MakeBone("LowerArm_L", BoneUpperArmL, new Vector2(-0.24f, 0));
        MakeBox("LowerArmL", BoneLowerArmL, new Vector2(-0.10f, 0), new Vector2(0.20f, 0.11f), _skinColor, order:1);
        BoneWeaponL   = MakeBone("ManaSaber", BoneLowerArmL, new Vector2(-0.22f, 0));
        var saberGo   = MakeBox("Saber", BoneWeaponL, new Vector2(-0.14f, 0), new Vector2(0.28f, 0.05f), _saberColor, order:2);
        _saberRenderer = saberGo.GetComponent<SpriteRenderer>();

        // ── RIGHT LEG
        BoneUpperLegR = MakeBone("UpperLeg_R", BoneHips, new Vector2( 0.12f, -0.10f));
        MakeBox("UpperLegR", BoneUpperLegR, new Vector2(0, -0.15f), new Vector2(0.13f, 0.30f), _coatColor, order:0);
        BoneLowerLegR = MakeBone("LowerLeg_R", BoneUpperLegR, new Vector2(0, -0.30f));
        MakeBox("BootR", BoneLowerLegR, new Vector2(0, -0.13f), new Vector2(0.13f, 0.28f), new Color(0.20f,0.14f,0.08f), order:0);

        // ── LEFT LEG
        BoneUpperLegL = MakeBone("UpperLeg_L", BoneHips, new Vector2(-0.12f, -0.10f));
        MakeBox("UpperLegL", BoneUpperLegL, new Vector2(0, -0.15f), new Vector2(0.13f, 0.30f), _coatColor, order:0);
        BoneLowerLegL = MakeBone("LowerLeg_L", BoneUpperLegL, new Vector2(0, -0.30f));
        MakeBox("BootL", BoneLowerLegL, new Vector2(0, -0.13f), new Vector2(0.13f, 0.28f), new Color(0.20f,0.14f,0.08f), order:0);
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────
    static Transform MakeBone(string name, Transform parent, Vector2 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        return go.transform;
    }

    static GameObject MakeBox(string name, Transform parent, Vector2 localPos,
                               Vector2 size, Color color, int order = 0)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite         = CreateRectSprite(size);
        sr.color          = color;
        sr.sortingOrder   = order;
        return go;
    }

    static Sprite CreateRectSprite(Vector2 size)
    {
        int w = Mathf.Max(1, Mathf.RoundToInt(size.x * 100));
        int h = Mathf.Max(1, Mathf.RoundToInt(size.y * 100));
        var tex = new Texture2D(w, h);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }
}
