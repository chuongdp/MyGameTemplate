namespace Game.Script.Utilities
{
    using System;
    using VContainer;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    public class UIRaycaster : MonoBehaviour
    {
        [SerializeField] private GraphicRaycaster graphicRaycaster;

        public GraphicRaycaster GetGraphicRaycaster => this.graphicRaycaster;

        #region Inject

        private EventSystem eventSystem;

        [Inject]
        public void Init(EventSystem eventSys)
        {
            this.eventSystem = eventSys;
        }
        #endregion

        /// <summary>
        /// Checks if a screen touch intersects with a GameObject containing the specified component type.
        /// </summary>
        /// <param name="screenPoint">The screen position to perform the check at.</param>
        /// <param name="type">The type of component to look for on the GameObject.</param>
        /// <returns>True if a GameObject with the specified component is hit, otherwise False.</returns>
        public bool CheckRaycast(Vector2 screenPoint, Type type)
        {
            // Create PointerEventData with the touch position
            var pointerEventData = new PointerEventData(this.eventSystem)
            {
                position = screenPoint,
            };

            // Perform raycast and store the results
            var results = new System.Collections.Generic.List<RaycastResult>();
            this.graphicRaycaster.Raycast(pointerEventData, results);

            // Check if any GameObject in the results contains the specified component type
            return results.Any(result => result.gameObject.GetComponent(type) != null);
        }
    }
}