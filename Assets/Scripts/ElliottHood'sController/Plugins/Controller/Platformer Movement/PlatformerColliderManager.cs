using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    /// <summary>
    /// A helper script that configures colliders for platformer movement with tilemaps
    /// - Also automatically sizes the GroundCheck
    /// </summary>
    public class PlatformerColliderManager : MonoBehaviour
    {
        /// <summary>
        /// Safe threshold where collisions won't overlap accidentally
        /// </summary>
        public const float COLLIDER_SPACING = 0.02f;
        public const float GROUND_CHECK_THICKNESS = 0.0625f;
        
        [Header("Collider")]
        [Tooltip("Shaped with slopes on the bottom corners to prevent getting stuck on terrain (especially tilemaps)")]
        [SerializeField] private PolygonCollider2D _slopedCollision;
        [SerializeField] private Vector2 _size;
        [SerializeField] private Vector2 _slopeSize = new Vector2(0.0125f, 0.0025f);  // The height of the slope at the bottom corners of the collision
        [Header("Ground Checks")]
        [SerializeField] private BoxCollider2D _groundCheck;
        [SerializeField] private BoxCollider2D _platformGroundCheck;
        [SerializeField] private float _groundCheckBoxThickness = 1 / 16f;

        private Rect GetCollisionBounds()
        {
            Rect bounds = new Rect
            {
                position = new Vector2(-_size.x / 2, 0),
                size = _size
            };
            return bounds;
        }

        private void OnValidate()
        {
            SetSlopedCollision();
            SetGroundChecks();
        }

        private void SetSlopedCollision()
        {
            if (!_slopedCollision)
                return;
            Rect bounds = GetCollisionBounds();

            Vector2[] points = new Vector2[6];

            points[0] = new Vector2(bounds.xMin, bounds.yMax);  // Top-left
            points[1] = new Vector2(bounds.xMax, bounds.yMax);  // Top-right
            points[2] = new Vector2(bounds.xMax, bounds.yMin + _slopeSize.y);  // Bottom-right with slope
            points[3] = new Vector2(bounds.xMax - _slopeSize.x, bounds.yMin);  // Bottom-right with slope
            points[4] = new Vector2(bounds.xMin + _slopeSize.x, bounds.yMin); // Bottom-left with slope
            points[5] = new Vector2(bounds.xMin, bounds.yMin + _slopeSize.y); // Bottom-left with slope

            _slopedCollision.points = points;
        }

        private void SetGroundChecks()
        {
            Rect collisionBounds = GetCollisionBounds();
            
            if (_groundCheck != null)
            {
                _groundCheck.size = new Vector2(collisionBounds.width - COLLIDER_SPACING * 2, _groundCheckBoxThickness);
                _groundCheck.offset = new Vector2(0, collisionBounds.yMin - _groundCheckBoxThickness / 2);
            }
            if (_platformGroundCheck != null)
            {
                _platformGroundCheck.size = new Vector2(collisionBounds.width - COLLIDER_SPACING * 2, _groundCheckBoxThickness);
                _platformGroundCheck.offset = new Vector2(0, collisionBounds.yMin + _groundCheckBoxThickness / 2 + COLLIDER_SPACING);
            }
        }
    }
}