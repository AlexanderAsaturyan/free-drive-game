using Car;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controller
{
    class InputController : MonoBehaviour
    {
        [SerializeField] private CarController carController;
        [SerializeField] private Button startStopButton;
        [SerializeField] private Button recoverButton;

        [SerializeField] private AudioSource engineStartSound;
        [SerializeField] private AudioSource engineOffSound;        

        private void OnEnable()
        {
            startStopButton.onClick.AddListener(StartStopEngine);
            recoverButton.onClick.AddListener(Recover);
        }

        private void OnDisable()
        {
            startStopButton.onClick.RemoveListener(StartStopEngine);
            recoverButton.onClick.RemoveListener(Recover);
        }


        private void StartStopEngine()
        {
            carController.IsCarRunning = !carController.IsCarRunning;

            switch (carController.IsCarRunning)
            {
                case true:
                    engineStartSound.Play();
                    break;
                case false:
                    engineOffSound.Play();
                    break;
            }
            Debug.LogError($"carController.IsCarRunning: {carController.IsCarRunning}");
        }

        private void Recover()
        {
            carController.Car.transform.eulerAngles = Vector3.zero;
        }
    }
}
