using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NPCRacingAI : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;
    public float waypointReachDistance = 12f;

    [Header("Movement")]
    public float maxSpeed      = 15f;
    public float acceleration  = 5f;
    public float brakeOnTurn   = 0.6f;
    public float turnSpeed     = 3.5f;

    [Header("Predictive Steering")]
    public float lookAheadDistance = 18f;

    [Header("Ground Check")]
    public float groundCheckDistance = 1.2f;
    public LayerMask groundLayer = ~0; // semua layer
    public float gravityForce  = 25f;

    [Header("Anti Stuck")]
    public float stuckTimeLimit = 2.5f;

    // ── Internal ──
    private Rigidbody rb;
    private int   currentWPIndex;
    private float currentSpeed;
    private float stuckTimer;
    private Vector3 lastPosition;
    private bool  isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass    = new Vector3(0, -0.5f, 0);
        rb.linearDamping   = 0.5f;   // kurangi sliding
        rb.angularDamping  = 5f;     // cegah spin

        lastPosition       = transform.position;
        currentWPIndex     = CariWaypointTerdekat();

        // Spawn: turunkan ke tanah dulu
        SnapKeJalan();
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        GroundCheck();
        TerapkanGravitasi();
        CheckWaypointReached();
        DeteksiStuck();
        Steering();
        Accelerate();
    }

    // ── Snap ke tanah saat spawn ───────────────────
    void SnapKeJalan()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 5f,
            Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            transform.position = hit.point + Vector3.up * 0.5f;
            Debug.Log($"[NPC] {name} di-snap ke tanah: {hit.point}");
        }
        else
        {
            Debug.LogWarning($"[NPC] {name} tidak menemukan tanah! " +
                             "Pastikan terrain/jalan punya collider.");
        }
    }

    // ── Deteksi apakah di tanah ───────────────────
    void GroundCheck()
    {
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.3f,
            Vector3.down,
            groundCheckDistance,
            groundLayer);
    }

    // ── Gravitasi custom agar nempel jalan ────────
    void TerapkanGravitasi()
    {
        if (!isGrounded)
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        else
            rb.AddForce(Vector3.down * 5f, ForceMode.Acceleration);
    }

    // ── Cari waypoint terdekat ────────────────────
    int CariWaypointTerdekat()
    {
        int   nearest = 0;
        float minDist = float.MaxValue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            float d = Vector3.Distance(transform.position, waypoints[i].position);
            if (d < minDist) { minDist = d; nearest = i; }
        }
        return nearest;
    }

    // ── Cek waypoint tercapai ─────────────────────
    void CheckWaypointReached()
    {
        if (waypoints[currentWPIndex] == null) return;

        float dist = Vector3.Distance(
            transform.position, waypoints[currentWPIndex].position);

        if (dist <= waypointReachDistance)
            currentWPIndex = (currentWPIndex + 1) % waypoints.Length;
    }

    // ── Deteksi stuck ─────────────────────────────
    void DeteksiStuck()
    {
        float gerak = Vector3.Distance(transform.position, lastPosition);

        if (gerak < 0.05f)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeLimit)
            {
                currentWPIndex = (currentWPIndex + 1) % waypoints.Length;
                stuckTimer     = 0f;

                // Paksa snap ke tanah lagi jika melayang
                if (!isGrounded) SnapKeJalan();

                Debug.Log($"[NPC] {name} stuck! Skip ke WP {currentWPIndex}");
            }
        }
        else stuckTimer = 0f;

        lastPosition = transform.position;
    }

    // ── Predictive Steering ───────────────────────
    void Steering()
    {
        Vector3 target = GetLookAheadPoint();
        Vector3 dir    = target - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation   = Quaternion.Slerp(
            transform.rotation, targetRot,
            Time.fixedDeltaTime * turnSpeed);
    }

    Vector3 GetLookAheadPoint()
    {
        float   sisa   = lookAheadDistance;
        int     idx    = currentWPIndex;
        Vector3 posNow = transform.position;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[idx] == null) break;
            Vector3 next = waypoints[idx].position;
            float   dist = Vector3.Distance(posNow, next);

            if (dist >= sisa)
                return Vector3.Lerp(posNow, next, sisa / dist);

            sisa  -= dist;
            posNow = next;
            idx    = (idx + 1) % waypoints.Length;
        }
        return waypoints[currentWPIndex].position;
    }

    // ── Akselerasi ────────────────────────────────
    void Accelerate()
    {
        if (!isGrounded) return; // jangan gas jika melayang

        Vector3 dir  = GetLookAheadPoint() - transform.position;
        dir.y        = 0;

        float angle      = Vector3.Angle(transform.forward, dir.normalized);
        float turnFactor = Mathf.Clamp01(angle / 90f);
        float targetSpeed = maxSpeed * (1f - turnFactor * brakeOnTurn);

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed,
                                  Time.fixedDeltaTime * acceleration);

        // Pakai AddForce bukan set velocity langsung
        Vector3 targetVel  = transform.forward * currentSpeed;
        Vector3 velError   = targetVel - rb.linearVelocity;
        velError.y         = 0;
        rb.AddForce(velError * 10f, ForceMode.Acceleration);
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(waypoints[i].position, 0.4f);
            Transform next = waypoints[(i + 1) % waypoints.Length];
            if (next != null)
                Gizmos.DrawLine(waypoints[i].position, next.position);
        }

        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(waypoints[currentWPIndex].position, 1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GetLookAheadPoint(), 0.8f);
    }
}