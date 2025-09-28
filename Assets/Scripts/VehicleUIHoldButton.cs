using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VehicleUIHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Kind { Throttle, Reverse, SteerLeft, SteerRight }
    public Kind kind;

    public VehicleUIInputsHub hub;     // drag the Hub here
    [Header("Optional visuals")]
    public Graphic highlight;          // (Image/Text) to tint while held
    public Color pressedColor = Color.white;
    public Color idleColor = new Color(1, 1, 1, 0.6f);

    bool held;

    void Awake()
    {
        if (highlight) highlight.color = idleColor;
    }

    public void OnPointerDown(PointerEventData e)
    {
        held = true;
        SetState(true);
    }

    public void OnPointerUp(PointerEventData e)
    {
        held = false;
        SetState(false);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (held) OnPointerUp(e);
    }

    void SetState(bool value)
    {
        if (!hub) return;

        switch (kind)
        {
            case Kind.Throttle: hub.throttleHeld = value; break;
            case Kind.Reverse: hub.reverseHeld = value; break;
            case Kind.SteerLeft: hub.steerLeftHeld = value; break;
            case Kind.SteerRight: hub.steerRightHeld = value; break;
        }

        if (highlight) highlight.color = value ? pressedColor : idleColor;
    }
}
