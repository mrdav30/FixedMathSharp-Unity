using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Demo component that captures Unity transform data into FixedMathSharp matrices and optionally
    /// round-trips those matrices back onto preview transforms for visual validation.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("FixedMathSharp/Demo/Fixed Transform Interop Demo")]
    public class FixedTransformInteropDemo : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private Transform sourceTransform = null!;
        [SerializeField] private bool autoRefresh = true;
        [SerializeField] private bool applyToTargets = true;

        [Header("Preview Targets")]
        [SerializeField] private Transform localMatrixTarget = null!;
        [SerializeField] private Transform worldMatrixTarget = null!;
        [SerializeField] private Transform localFixedTransformTarget = null!;
        [SerializeField] private Transform localRotationTarget = null!;
        [SerializeField] private Transform worldRotationTarget = null!;

        [Header("Application Status")]
        [SerializeField] private bool localMatrixApplied;
        [SerializeField] private bool worldMatrixApplied;

        [Header("Captured Fixed4x4")]
        [SerializeField] private Fixed4x4 localFixed4x4;
        [SerializeField] private Fixed4x4 worldFixed4x4;

        [Header("Captured Fixed3x3")]
        [SerializeField] private Fixed3x3 localRotationMatrix;
        [SerializeField] private Fixed3x3 worldRotationMatrix;

        [Header("Round-Trip Unity Matrices")]
        [SerializeField] private Matrix4x4 localFixedAsUnityMatrix;
        [SerializeField] private Matrix4x4 worldFixedAsUnityMatrix;

        private FixedTransform localFixedTransform;

        private Transform Source => sourceTransform != null ? sourceTransform : transform;

        private void OnEnable()
        {
            RefreshDemoData();
        }

        private void OnValidate()
        {
            if (autoRefresh)
                RefreshDemoData();
        }

        private void Update()
        {
            if (autoRefresh)
                RefreshDemoData();
        }

        [ContextMenu("Refresh Demo Data")]
        public void RefreshDemoData()
        {
            Transform source = Source;
            if (source == null)
                return;

            localFixed4x4 = source.ToFixed4x4LocalMatrix();
            worldFixed4x4 = source.ToFixed4x4WorldMatrix();
            localFixedTransform = source.ToFixedTransformLocal();

            localRotationMatrix = source.ToFixed3x3LocalRotation();
            worldRotationMatrix = source.ToFixed3x3WorldRotation();

            localFixedAsUnityMatrix = localFixed4x4.ToMatrix4x4();
            worldFixedAsUnityMatrix = worldFixed4x4.ToMatrix4x4();

            if (applyToTargets)
                ApplyPreviewTargets(source);
        }

        private void ApplyPreviewTargets(Transform source)
        {
            ApplyLocalMatrixTarget(source);
            ApplyWorldMatrixTarget(source);
            ApplyLocalFixedTransformTarget(source);
            ApplyLocalRotationTarget(source);
            ApplyWorldRotationTarget(source);
        }

        private void ApplyLocalMatrixTarget(Transform source)
        {
            if (localMatrixTarget == null || localMatrixTarget == source)
                return;

            localMatrixApplied = localFixed4x4.TryApplyToTransformLocal(localMatrixTarget);
        }

        private void ApplyWorldMatrixTarget(Transform source)
        {
            if (worldMatrixTarget == null || worldMatrixTarget == source)
                return;

            worldMatrixApplied = worldFixed4x4.TryApplyToTransformWorld(worldMatrixTarget);
        }

        private void ApplyLocalFixedTransformTarget(Transform source)
        {
            if (localFixedTransformTarget == null || localFixedTransformTarget == source)
                return;

            localFixedTransform.ApplyToTransformLocal(localFixedTransformTarget);
        }

        private void ApplyLocalRotationTarget(Transform source)
        {
            if (localRotationTarget == null || localRotationTarget == source)
                return;

            localRotationMatrix.ApplyRotationToTransformLocal(localRotationTarget);
        }

        private void ApplyWorldRotationTarget(Transform source)
        {
            if (worldRotationTarget == null || worldRotationTarget == source)
                return;

            worldRotationMatrix.ApplyRotationToTransformWorld(worldRotationTarget);
        }

    }
}
