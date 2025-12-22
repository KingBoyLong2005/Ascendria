
using System;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxManager : MonoBehaviour
{
    public static HitBoxManager Instance { get; private set; }

    // =========================================================
    // CONFIG
    // =========================================================
    [Header("Buffer")]
    [SerializeField] private int colliderBufferSize = 64;
    public Collider[] ColliderBuffer { get; private set; }

    [Header("Debug")]
    public bool drawDebug = true;
    private readonly List<DebugBox> debugBoxes = new();
    private readonly List<DebugSphere> debugSpheres = new();
    private readonly List<DebugRay> debugRays = new();


    // =========================================================
    // REQUEST STRUCTS
    // =========================================================
    private struct BoxRequest
    {
        public Vector3 center;
        public Vector3 halfExtents;
        public Quaternion rotation;
        public LayerMask mask;
        public Action<Collider> callback;
        public float duration;
    }

    private struct SphereRequest
    {
        public Vector3 center;
        public float radius;
        public LayerMask mask;
        public Action<Collider> callback;
        public float duration;
    }

    private struct RayRequest
    {
        public Vector3 origin;
        public Vector3 direction;
        public float distance;
        public LayerMask mask;
        public Action<RaycastHit> callback;
        public float duration;
    }


    // =========================================================
    // REQUEST QUEUES
    // =========================================================
    private readonly List<BoxRequest> boxRequests = new();
    private readonly List<SphereRequest> sphereRequests = new();
    private readonly List<RayRequest> rayRequests = new();


    // =========================================================
    // LIFECYCLE
    // =========================================================
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ColliderBuffer = new Collider[colliderBufferSize];
    }

    private void Update()
    {
        ProcessBoxRequests();
        ProcessSphereRequests();
        ProcessRayRequests();
    }


    // =========================================================
    // PUBLIC API (REQUEST)
    // =========================================================

    /// <summary>
    /// Dùng cho chém, aoe hình hộp, hitbox tạm thời
    /// </summary>
    public void RequestBox(
        Vector3 center,
        Vector3 size,
        Quaternion rotation,
        LayerMask mask,
        Action<Collider> onHit,
        float debugDuration = 0.1f)
    {
        boxRequests.Add(new BoxRequest
        {
            center = center,
            halfExtents = size * 0.5f,
            rotation = rotation,
            mask = mask,
            callback = onHit,
            duration = debugDuration
        });

        if (drawDebug)
            debugBoxes.Add(new DebugBox(center, size, rotation, debugDuration));
    }

    /// <summary>
    /// Dùng cho explosion, aura, damage vòng tròn
    /// </summary>
    public void RequestSphere(
        Vector3 center,
        float radius,
        LayerMask mask,
        Action<Collider> onHit,
        float debugDuration = 0.1f)
    {
        sphereRequests.Add(new SphereRequest
        {
            center = center,
            radius = radius,
            mask = mask,
            callback = onHit,
            duration = debugDuration
        });

        if (drawDebug)
            debugSpheres.Add(new DebugSphere(center, radius, debugDuration));
    }

    /// <summary>
    /// Dùng cho laser, lightning, hitscan
    /// </summary>
    public void RequestRay(
        Vector3 origin,
        Vector3 direction,
        float distance,
        LayerMask mask,
        Action<RaycastHit> onHit,
        float debugDuration = 0.05f)
    {
        rayRequests.Add(new RayRequest
        {
            origin = origin,
            direction = direction.normalized,
            distance = distance,
            mask = mask,
            callback = onHit,
            duration = debugDuration
        });

        if (drawDebug)
            debugRays.Add(new DebugRay(origin, direction, distance, debugDuration));
    }


    // =========================================================
    // PROCESS REQUESTS
    // =========================================================

    private void ProcessBoxRequests()
    {
        foreach (var req in boxRequests)
        {
            int count = Physics.OverlapBoxNonAlloc(
                req.center,
                req.halfExtents,
                ColliderBuffer,
                req.rotation,
                req.mask
            );

            for (int i = 0; i < count; i++)
                req.callback?.Invoke(ColliderBuffer[i]);
        }

        boxRequests.Clear();
    }

    private void ProcessSphereRequests()
    {
        foreach (var req in sphereRequests)
        {
            int count = Physics.OverlapSphereNonAlloc(
                req.center,
                req.radius,
                ColliderBuffer,
                req.mask
            );

            for (int i = 0; i < count; i++)
                req.callback?.Invoke(ColliderBuffer[i]);
        }

        sphereRequests.Clear();
    }

    private void ProcessRayRequests()
    {
        foreach (var req in rayRequests)
        {
            if (Physics.Raycast(
                req.origin,
                req.direction,
                out RaycastHit hit,
                req.distance,
                req.mask))
            {
                req.callback?.Invoke(hit);
            }
        }

        rayRequests.Clear();
    }


    // =========================================================
    // DEBUG GIZMOS
    // =========================================================

    private void LateUpdate()
    {
        UpdateDebugTimers(debugBoxes);
        UpdateDebugTimers(debugSpheres);
        UpdateDebugTimers(debugRays);
    }

    private void UpdateDebugTimers<T>(List<T> list) where T : IDebugShape
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            list[i].Timer -= Time.deltaTime;
            if (list[i].Timer <= 0f)
                list.RemoveAt(i);
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.red;

        foreach (var b in debugBoxes) b.Draw();
        foreach (var s in debugSpheres) s.Draw();
        foreach (var r in debugRays) r.Draw();
    }


    // =========================================================
    // DEBUG SHAPES
    // =========================================================

    private interface IDebugShape
    {
        float Timer { get; set; }
        void Draw();
    }

    private class DebugBox : IDebugShape
    {
        public Vector3 center;
        public Vector3 size;
        public Quaternion rotation;
        public float Timer { get; set; }

        public DebugBox(Vector3 c, Vector3 s, Quaternion r, float t)
        {
            center = c;
            size = s;
            rotation = r;
            Timer = t;
        }

        public void Draw()
        {
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }

    private class DebugSphere : IDebugShape
    {
        public Vector3 center;
        public float radius;
        public float Timer { get; set; }

        public DebugSphere(Vector3 c, float r, float t)
        {
            center = c;
            radius = r;
            Timer = t;
        }

        public void Draw()
        {
            Gizmos.DrawWireSphere(center, radius);
        }
    }

    private class DebugRay : IDebugShape
    {
        public Vector3 origin;
        public Vector3 dir;
        public float dist;
        public float Timer { get; set; }

        public DebugRay(Vector3 o, Vector3 d, float dis, float t)
        {
            origin = o;
            dir = d.normalized;
            dist = dis;
            Timer = t;
        }

        public void Draw()
        {
            Gizmos.DrawLine(origin, origin + dir * dist);
        }
    }
}
