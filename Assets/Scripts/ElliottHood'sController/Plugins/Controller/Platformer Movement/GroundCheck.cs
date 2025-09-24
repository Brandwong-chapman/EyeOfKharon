using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    /// <summary>
    /// 
    /// The ground detection will not work accurately if their parents are ever rotated
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class GroundCheck : MonoBehaviour
    {
        public static int GroundLayerMask => 1 << Layers.Default;
        [SerializeField] private BoxCollider2D _groundBox;
        [SerializeField] private bool _detectsPlatforms = false;
        [SerializeField] private LayerMask _platformMask;
        [Tooltip("The bottom edge of this box should be a smidge above the top edge of the groundBox, and should be inside the collision of the player.")]
        [SerializeField] private BoxCollider2D _noPlatformZone;
        public bool Grounded { get; private set; }
        public Collider2D LastCollision { get; protected set; }
        public float LastCollisionLeftTime { get; protected set; } = Mathf.NegativeInfinity;

        private bool _groundFoundThisUpdate;

        private void OnValidate()
        {
            if (_groundBox == null)
            {
                _groundBox = GetComponent<BoxCollider2D>();
                _groundBox.isTrigger = true;
            }
        }

        private void FixedUpdate()
        {
            bool groundFoundLastUpdate = _groundFoundThisUpdate;
            _groundFoundThisUpdate = false;

            GroundDetection();
            if (!_groundFoundThisUpdate && _detectsPlatforms) 
                PlatformDetection();

            if (!_groundFoundThisUpdate && groundFoundLastUpdate)
                GroundLeft();
        }

        /// <summary>
        /// Returns true if theres a platform in the PlatformBox AND no platform in the NoPlatBox
        /// </summary>
        /// <returns></returns>
        private void PlatformDetection()
        {
            Collider2D platHit = Physics2DOverlapBoxCollider(_groundBox, _platformMask);
            Collider2D noPlatHit = Physics2DOverlapBoxCollider(_noPlatformZone, _platformMask);

            if (platHit != null && noPlatHit == null)
                GroundHit(platHit);
        }

        private void GroundDetection()
        {
            Collider2D collisionResults = Physics2DOverlapBoxCollider(_groundBox, GroundLayerMask);

            if (collisionResults != null)
                GroundHit(collisionResults);
        }

        private void GroundHit(Collider2D hit)
        {
            Grounded = true;
            LastCollision = hit;
            _groundFoundThisUpdate = true;
        }

        private void GroundLeft()
        {
            Grounded = false;
            LastCollisionLeftTime = Time.time;
        }

        public Vector2 GetLastCollisionPoint(Vector2 referencePoint, bool lockXAxis = false, bool lockYAxis = false)
        {
            var point = LastCollision.ClosestPoint(referencePoint);
            if (lockXAxis) point.x = referencePoint.x;
            if (lockYAxis) point.y = referencePoint.y;
            return point;
        }
        
        public static Collider2D Physics2DOverlapBoxCollider(BoxCollider2D boxCollider2D, LayerMask layermask)
        {
            return Physics2D.OverlapBox((Vector2)boxCollider2D.transform.position + boxCollider2D.offset, boxCollider2D.size * boxCollider2D.transform.lossyScale, 0, layermask);
        }
    }

}
