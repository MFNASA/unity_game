// Assets/Editor/WaypointPainter.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class WaypointPainter : EditorWindow
{
    // ── Data ───────────────────────────────────────
    GameObject        container;       // RCCP_AI_WaypointsContainer
    List<Vector3>     points    = new List<Vector3>();
    bool              paintMode = false;

    // ── Pengaturan ─────────────────────────────────
    float tinggiOffset   = 0.3f;
    float snapJarak      = 0f;    // 0 = tidak snap ke WP terdekat
    LayerMask groundMask = ~0;

    Vector2 scrollPos;

    // ───────────────────────────────────────────────
    [MenuItem("Tools/Waypoint Painter")]
    public static void ShowWindow()
        => GetWindow<WaypointPainter>("Waypoint Painter");

    void OnEnable()  => SceneView.duringSceneGui += OnSceneGUI;
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        paintMode = false;
    }

    // ───────────────────────────────────────────────
    // SCENE GUI
    // ───────────────────────────────────────────────
    void OnSceneGUI(SceneView sv)
    {
        if (!paintMode) return;

        HandleUtility.AddDefaultControl(
            GUIUtility.GetControlID(FocusType.Passive));

        Event e = Event.current;

        // Klik kiri = tambah titik
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
            {
                Vector3 titik = hit.point + Vector3.up * tinggiOffset;
                points.Add(titik);
                e.Use();
                Repaint();
            }
        }

        // Klik kanan = hapus titik terakhir
        if (e.type == EventType.MouseDown && e.button == 1 && !e.alt)
        {
            if (points.Count > 0)
            {
                points.RemoveAt(points.Count - 1);
                e.Use();
                Repaint();
            }
        }

        DrawPreview(sv);
    }

    // ───────────────────────────────────────────────
    // PREVIEW di Scene View
    // ───────────────────────────────────────────────
    void DrawPreview(SceneView sv)
    {
        if (points.Count == 0) return;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        for (int i = 0; i < points.Count; i++)
        {
            // Sphere titik
            Handles.color = (i == points.Count - 1)
                ? Color.green   // titik terakhir = hijau
                : Color.yellow;
            Handles.SphereHandleCap(0, points[i],
                Quaternion.identity, 0.6f, EventType.Repaint);

            // Label nomor
            Handles.Label(points[i] + Vector3.up * 1f,
                $" WP {i}", EditorStyles.boldLabel);

            // Garis penghubung
            if (i > 0)
            {
                Handles.color = Color.cyan;
                Handles.DrawLine(points[i - 1], points[i], 2f);

                // Gambar panah arah
                Vector3 mid = (points[i - 1] + points[i]) * 0.5f;
                Vector3 dir = (points[i] - points[i - 1]).normalized;
                Handles.ArrowHandleCap(0, mid,
                    Quaternion.LookRotation(dir), 1.2f, EventType.Repaint);
            }
        }

        // Radius jangkau waypoint (preview)
        if (points.Count > 0)
        {
            Handles.color = new Color(1f, 1f, 0f, 0.08f);
            Handles.DrawSolidDisc(points[points.Count - 1],
                Vector3.up, 12f); // radius = waypointReachDistance default
        }

        sv.Repaint();
    }

    // ───────────────────────────────────────────────
    // GUI WINDOW
    // ───────────────────────────────────────────────
    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("=== Waypoint Painter ===", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Container
        container = (GameObject)EditorGUILayout.ObjectField(
            "Waypoint Container", container, typeof(GameObject), true);

        if (container == null)
            EditorGUILayout.HelpBox(
                "Drag RCCP_AI_WaypointsContainer dari Hierarchy ke sini.",
                MessageType.Warning);

        EditorGUILayout.Space(6);

        // Pengaturan
        GUILayout.Label("Pengaturan", EditorStyles.boldLabel);
        tinggiOffset = EditorGUILayout.FloatField("Offset Tinggi (m)", tinggiOffset);
        groundMask   = EditorGUILayout.MaskField("Ground Layer",
            groundMask, UnityEditorInternal.InternalEditorUtility.layers);

        EditorGUILayout.Space(8);

        // Tombol Paint Mode
        GUI.backgroundColor = paintMode
            ? new Color(0.3f, 1f, 0.4f)
            : new Color(0.7f, 0.7f, 0.7f);

        if (GUILayout.Button(
            paintMode
                ? "🖌️  MODE AKTIF — Klik di Scene untuk tambah WP"
                : "🖌️  Aktifkan Mode Melukis",
            GUILayout.Height(36)))
        {
            paintMode = !paintMode;
            if (paintMode)
            {
                SceneView.lastActiveSceneView?.Focus();
                Debug.Log("[WP Painter] ON — Klik kiri: tambah | Klik kanan: hapus terakhir");
            }
        }
        GUI.backgroundColor = Color.white;

        if (paintMode)
            EditorGUILayout.HelpBox(
                "🖱️ Klik KIRI  → tambah waypoint\n" +
                "🖱️ Klik KANAN → hapus waypoint terakhir\n" +
                $"📍 Total: {points.Count} titik",
                MessageType.Info);

        EditorGUILayout.Space(10);

        // Info titik
        EditorGUILayout.LabelField($"Total Titik: {points.Count}",
            EditorStyles.boldLabel);

        EditorGUILayout.Space(6);

        // ── Tombol Generate ──
        GUI.backgroundColor = (container != null && points.Count >= 2)
            ? Color.green : Color.gray;
        if (GUILayout.Button("▶  GENERATE WAYPOINTS ke Scene",
            GUILayout.Height(40)))
            GenerateWaypoints();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(4);

        // ── Tombol hapus titik preview ──
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("✕  Hapus Semua Titik Preview", GUILayout.Height(26)))
        {
            if (EditorUtility.DisplayDialog("Konfirmasi",
                "Hapus semua titik yang digambar?", "Ya", "Batal"))
                points.Clear();
        }

        // ── Tombol hapus waypoint di scene ──
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("🗑  Hapus Semua Waypoint di Container",
            GUILayout.Height(26)))
            HapusWaypointLama();

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(12);

        // ── Tips ──
        GUILayout.Label("Tips", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• Klik mengikuti TENGAH jalan\n" +
            "• Tikungan: klik lebih rapat (tiap 8–10m)\n" +
            "• Lurus: klik lebih jarang (tiap 15–25m)\n" +
            "• Lingkaran kuning = radius jangkau WP\n" +
            "• Pastikan titik tidak terlalu dekat satu sama lain",
            MessageType.None);

        EditorGUILayout.EndScrollView();
    }

    // ───────────────────────────────────────────────
    // GENERATE — buat GameObject waypoint di container
    // ───────────────────────────────────────────────
    void GenerateWaypoints()
    {
        if (container == null)
        { Debug.LogError("[WP Painter] Container belum diisi!"); return; }

        if (points.Count < 2)
        { Debug.LogError("[WP Painter] Minimal 2 titik!"); return; }

        // Hapus waypoint lama di container
        HapusWaypointLama();

        Undo.RegisterFullObjectHierarchyUndo(container, "Generate Waypoints");

        for (int i = 0; i < points.Count; i++)
        {
            GameObject wp = new GameObject($"Waypoint {i}");
            wp.transform.position = points[i];
            wp.transform.SetParent(container.transform);
            wp.transform.SetSiblingIndex(i);
            Undo.RegisterCreatedObjectUndo(wp, "Create Waypoint");
        }

        paintMode = false;
        points.Clear();

        Debug.Log($"[WP Painter] {container.transform.childCount} waypoint berhasil dibuat!");
        Selection.activeGameObject = container;
        EditorGUIUtility.PingObject(container);
    }

    void HapusWaypointLama()
    {
        if (container == null) return;

        // Hapus semua child dari belakang
        int count = container.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(
                container.transform.GetChild(i).gameObject);
        }
        Debug.Log("[WP Painter] Waypoint lama dihapus.");
    }
}