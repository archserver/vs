using UnityEngine;
using UnityEngine.EventSystems;

// On-screen virtual joystick — visible for mobile, hidden on PC
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static VirtualJoystick JoystickInstance;     // pointerd so PlayerController can read input

    [SerializeField] private RectTransform handle;      
    [SerializeField] private float handleRange = 50f;   // max pixels the handle can travel from center

    public Vector2 InputDirection { get; private set; } // direction read by PlayerController

    private RectTransform _background;                  

    private void Awake()
    {
        JoystickInstance = this;
        _background = GetComponent<RectTransform>();

        // hide the joystick entirely on PC 
        gameObject.SetActive(Application.isMobilePlatform);
        //gameObject.SetActive(true); // for testing on pc
    }

    // finger touched down — tracking
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    // finger moved — move the handle and update the input direction
    public void OnDrag(PointerEventData eventData)
    {
        // convert the touch position into space of the background 
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        // limit the handle so it can't move outside the allowed range
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
        handle.localPosition = clamped;

        // normalize to 0-1 range so movement speed is constant
        InputDirection = clamped / handleRange;
    }

    // finger lifted — move handle back to center and stop movement
    public void OnPointerUp(PointerEventData eventData)
    {
        handle.localPosition = Vector2.zero;
        InputDirection = Vector2.zero;
    }
}
