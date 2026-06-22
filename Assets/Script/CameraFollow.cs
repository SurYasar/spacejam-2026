using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float damping;

    public Transform target;

    [Header("Camera Bounds (Batas Arena)")]
    [Tooltip("Batas koordinat X paling kiri")] public float minX;
    [Tooltip("Batas koordinat X paling kanan")] public float maxX;
    [Tooltip("Batas koordinat Y paling bawah")] public float minY;
    [Tooltip("Batas koordinat Y paling atas")] public float maxY;

    private Vector3 vel = Vector3.zero;

    private void FixedUpdate()
    {
        if (target)
        {
            Vector3 targetPosition = target.position + offset;
            targetPosition.z = transform.position.z;

            // 1. Hitung pergerakan smooth seperti biasa
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref vel, damping, Mathf.Infinity, Time.unscaledDeltaTime);

            // 2. Batasi posisi X dan Y hasil smooth tadi agar tidak melewati batas
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);

            // 3. Aplikasikan posisi yang sudah aman ke kamera
            transform.position = smoothedPosition;
        }
        
    }
}
