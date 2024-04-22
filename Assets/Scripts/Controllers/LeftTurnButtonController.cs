using Car;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers
{
    public class LeftTurnButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private CarController carController;
        [SerializeField] private Button leftButton;

        private float leftTurnValue;

        public void OnPointerDown(PointerEventData eventData)
        {
            leftTurnValue = -1;
            carController.SetTurnInput(leftTurnValue); 
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            leftTurnValue = 0;
            carController.SetTurnInput(leftTurnValue);
        }
    }
}