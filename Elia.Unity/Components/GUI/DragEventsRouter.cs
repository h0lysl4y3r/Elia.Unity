using UnityEngine;
using UnityEngine.EventSystems;

namespace Elia.Unity.Components.GUI
{
    /// <summary>
    /// Routes drag events to <see cref="Target"/> <see cref="GameObject"/>.
    /// </summary>
	[AddComponentMenu("ELIA/Components/DragEventsRouter")]
	public sealed class DragEventsRouter : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler, IDragHandler
	{
		#region Members

        /// <summary>
        /// Target <see cref="GameObject"/> to route event data to.
        /// </summary>
		public GameObject Target;

		private IBeginDragHandler[] _beginDragObjects;
		private IEndDragHandler[] _endDragObjects;
		private IInitializePotentialDragHandler[] _initializePotentialDragObjects;
		private IDragHandler[] _dragObjects;

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			_beginDragObjects = Target.GetComponents<IBeginDragHandler>();
			_endDragObjects = Target.GetComponents<IEndDragHandler>();
			_initializePotentialDragObjects = Target.GetComponents<IInitializePotentialDragHandler>();
			_dragObjects = Target.GetComponents<IDragHandler>();
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (_beginDragObjects != null)
			{
				foreach (var beginDragHandler in _beginDragObjects)
				{
					beginDragHandler.OnBeginDrag(eventData);
				}
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (_endDragObjects != null)
			{
				foreach (var endDragHandler in _endDragObjects)
				{
					endDragHandler.OnEndDrag(eventData);
				}
			}
		}

		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			if (_initializePotentialDragObjects != null)
			{
				foreach (var initializePotentialDragHandler in _initializePotentialDragObjects)
				{
					initializePotentialDragHandler.OnInitializePotentialDrag(eventData);
				}
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (_dragObjects != null)
			{
				foreach (var dragHandler in _dragObjects)
				{
					dragHandler.OnDrag(eventData);
				}
			}
		}

		#endregion
	}
}
