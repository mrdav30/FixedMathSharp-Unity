using UnityEngine;

namespace FixedMathSharp
{
    /// <summary>
    /// Demo component that captures Unity Bounds into FixedMathSharp bounds types and exposes
    /// the round-tripped results for inspector and gizmo validation.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("FixedMathSharp/Demo/Fixed Bounds Interop Demo")]
    public class FixedBoundsInteropDemo : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private Renderer sourceRenderer = null!;
        [SerializeField] private Collider sourceCollider = null!;
        [SerializeField] private bool autoRefresh = true;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color sourceBoundsColor = Color.white;
        [SerializeField] private Color boundingBoxColor = Color.green;
        [SerializeField] private Color boundingAreaColor = Color.cyan;

        [Header("Captured Bounds")]
        [SerializeField] private bool hasValidBounds;
        [SerializeField] private Bounds unityBounds;
        [SerializeField] private BoundingBox fixedBoundingBox;
        [SerializeField] private BoundingArea fixedBoundingArea;
        [SerializeField] private Bounds roundTripBoundingBoxBounds;
        [SerializeField] private Bounds roundTripBoundingAreaBounds;

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
            if (!TryGetSourceBounds(out Bounds bounds))
            {
                hasValidBounds = false;
                return;
            }

            hasValidBounds = true;
            unityBounds = bounds;
            fixedBoundingBox = bounds.ToBoundingBox();
            fixedBoundingArea = bounds.ToBoundingArea();
            roundTripBoundingBoxBounds = fixedBoundingBox.ToBounds();
            roundTripBoundingAreaBounds = fixedBoundingArea.ToBounds();
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || !hasValidBounds)
                return;

            DrawBounds(unityBounds, sourceBoundsColor);
            DrawBounds(roundTripBoundingBoxBounds, boundingBoxColor);
            DrawBounds(roundTripBoundingAreaBounds, boundingAreaColor);
        }

        private bool TryGetSourceBounds(out Bounds bounds)
        {
            if (sourceRenderer != null)
            {
                bounds = sourceRenderer.bounds;
                return true;
            }

            if (sourceCollider != null)
            {
                bounds = sourceCollider.bounds;
                return true;
            }

            if (TryGetComponent(out Renderer fallbackRenderer))
            {
                bounds = fallbackRenderer.bounds;
                return true;
            }

            if (TryGetComponent(out Collider fallbackCollider))
            {
                bounds = fallbackCollider.bounds;
                return true;
            }

            bounds = default;
            return false;
        }

        private static void DrawBounds(Bounds bounds, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
