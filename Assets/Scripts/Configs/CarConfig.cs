using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "CarConfig", menuName = "Configs")]
    public class CarConfig : ScriptableObject
    {
        [SerializeField] private float transmissionEfficiency = 0.95f;
        [SerializeField] private float brakingForce = 300000f;
        [SerializeField] private float maxTurnAngle = 35f;
        [SerializeField] private float engineTorque = 196f;
        [SerializeField] private float[] gearRatios = { 0.8f, 1.0f, 1.3f, 1.9f, 3.5f };
        [SerializeField] private float[] gearSpeedLimits = { 11.1f, 22.2f, 33.3f, 44.4f, 55.5f };
        [SerializeField] private float frontSideSlipLimit = 0.3f;
        [SerializeField] private float backSideSlipLimit = 0.1f;
        
        public float TransmissionEfficiency => transmissionEfficiency;
        public float BrakingForce => brakingForce;
        public float MaxTurnAngle => maxTurnAngle;
        public float EngineTorque => engineTorque;

        public float[] GearRatios => gearRatios;
        public float[] GearSpeedLimits => gearSpeedLimits;

        public float FrontSideSlipLimit => frontSideSlipLimit;
        public float BackSideSlipLimit => backSideSlipLimit;
    }
}