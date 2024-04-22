using Car;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    class GasButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private CarController carController;
        [SerializeField] private Button gasButton;

        private float gasInputValue;

        public void OnPointerDown(PointerEventData eventData)
        {
            gasInputValue = 1f;
            carController.SetGasInput(gasInputValue);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            gasInputValue = 0f;
            carController.SetGasInput(gasInputValue);
        }
    }
}
