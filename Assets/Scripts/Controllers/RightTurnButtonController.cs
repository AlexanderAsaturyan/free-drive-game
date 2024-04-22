using Car;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers
{
    public class RightTurnButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private CarController carController;
        [SerializeField] private Button rightTurnButton;

        private float rightTurnValue;

        public void OnPointerDown(PointerEventData eventData)
        {
            rightTurnValue = 1;
            carController.SetTurnInput(rightTurnValue);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            rightTurnValue = 0;
            carController.SetTurnInput(rightTurnValue);
        }
    }
}
