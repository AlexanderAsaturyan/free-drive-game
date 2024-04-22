using Car;
using Configs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers
{
    public class BrakeButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Button brakeButton;
        [SerializeField] private CarConfig carConfig;
        [SerializeField] private CarController carController;

        [SerializeField] private Light leftTailLight;
        [SerializeField] private Light midTailLight;
        [SerializeField] private Light rightTailLight;

        public void OnPointerDown(PointerEventData eventData)
        {
            carController.CurrentBrakeForce = carConfig.BrakingForce;
            leftTailLight.enabled = true;
            midTailLight.enabled = true;
            rightTailLight.enabled = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            carController.CurrentBrakeForce = 0;
            leftTailLight.enabled = false;
            midTailLight.enabled = false;
            rightTailLight.enabled = false;
        }
    }
}
