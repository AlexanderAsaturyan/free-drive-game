using Configs;
using System.Collections.Generic;
using UnityEngine;

namespace Car
{
    public class CarController : MonoBehaviour
    {
        [SerializeField] private WheelCollider leftFrontCollider;
        [SerializeField] private WheelCollider rightFrontCollider;
        [SerializeField] private WheelCollider rightBackCollider;
        [SerializeField] private WheelCollider leftBackCollider;

        [SerializeField] private Transform leftFrontTransform;
        [SerializeField] private Transform rightFrontTransform;
        [SerializeField] private Transform rightBackTransform;
        [SerializeField] private Transform leftBackTransform;

        [SerializeField] private TrailRenderer leftFrontTrail;
        [SerializeField] private TrailRenderer rightFrontTrail;
        [SerializeField] private TrailRenderer rightBackTrail;
        [SerializeField] private TrailRenderer leftBackTrail;

        [SerializeField] private Rigidbody car;
        [SerializeField] private CarConfig carConfig;

        [SerializeField] private AudioSource engineStartSound;
        [SerializeField] private AudioSource engineOffSound;
        [SerializeField] private AudioSource driftSound;

        private const float KmhCoeff = 3.6f;

        private bool _isCarRunning;
        public bool IsCarRunning
        {
            get { return _isCarRunning; }
            set { _isCarRunning = value; }
        }

        public Rigidbody Car
        {
            get { return car; }
            set { car = value; }
        }


        private float _currentBrakeForce;
        public float CurrentBrakeForce 
        { 
            get { return _currentBrakeForce; }
            set { _currentBrakeForce = value; }
        }
        private float _currentTurnAngle;

        private int _gear;

        private float _wheelTorque;
        private float _gasInput;

        private float _maxSpeedMs;
        private float _currentSpeedKmh;

        private Dictionary<int, (float speedLimit, float gearRatio)> gearData;

        private float targetTurnAngle = 0f;
        private float turnLerpTime = 0.1f;

        private void Start()
        {
            CreateGearData();
           // Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            ResetCarRotation();
            ControlInputs();
            if (car.velocity.magnitude > _maxSpeedMs)
            {
                car.velocity = Vector3.ClampMagnitude(car.velocity, _maxSpeedMs);
            }
        }

        private void FixedUpdate()
        {
            if (_isCarRunning && _gasInput >= 0)
            {
                Accelerate();
            }

            Brake();
            Turn();
            ControlDrift();
        }

        private void LateUpdate()
        {
            UpdateAllWheels();
        }

        private void CreateGearData()
        {
            gearData = new Dictionary<int, (float, float)>()
          {
             { 1, (carConfig.GearSpeedLimits[0], carConfig.GearRatios[4]) },
             { 2, (carConfig.GearSpeedLimits[1], carConfig.GearRatios[3]) },
             { 3, (carConfig.GearSpeedLimits[2], carConfig.GearRatios[2]) },
             { 4, (carConfig.GearSpeedLimits[3], carConfig.GearRatios[1]) },
             { 5, (carConfig.GearSpeedLimits[4], carConfig.GearRatios[0]) },
             { 0, (car.velocity.magnitude, 0) },
             { -1, (carConfig.GearSpeedLimits[0], carConfig.GearRatios[4] * -1) }
          };
        }

        private void ResetCarRotation()
        {
            if (Input.GetKey(KeyCode.R))
            {
                car.transform.eulerAngles = Vector3.zero;
            }
        }

        private void ControlInputs()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                _isCarRunning = !_isCarRunning;

                switch (_isCarRunning)
                {
                    case true:
                        engineStartSound.Play();
                        break;
                    case false:
                        engineOffSound.Play();
                        break;
                }

                Debug.LogError($"_isCarRunning: {_isCarRunning}");
            }

           // _gasInput = Input.GetAxis("Vertical");

            if (Input.GetKeyDown(KeyCode.N)) _gear = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha1)) _gear = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) _gear = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) _gear = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) _gear = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) _gear = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6)) _gear = -1;

            if (Input.GetKeyDown(KeyCode.LeftShift) && _gear < 5) _gear++;
            else if (Input.GetKeyDown(KeyCode.LeftControl) && _gear > -1) _gear--;

            if (_gear == 0)
            {
                gearData[0] = (car.velocity.magnitude, 0);
            }

            _maxSpeedMs = gearData[_gear].speedLimit;
            _wheelTorque = carConfig.EngineTorque * gearData[_gear].gearRatio / carConfig.TransmissionEfficiency;
            
            _currentSpeedKmh = Mathf.Round(car.velocity.magnitude) * KmhCoeff;
            Debug.LogError($"Speed: {_currentSpeedKmh} km/h");
            Debug.LogError($"Gear: {_gear}");

            // _currentTurnAngle = carConfig.MaxTurnAngle * Input.GetAxis("Horizontal");
            _currentTurnAngle = Mathf.Lerp(_currentTurnAngle, targetTurnAngle, turnLerpTime);



            if (Input.GetKey(KeyCode.DownArrow))
            {
                _currentBrakeForce = carConfig.BrakingForce;
            }            
        }

        private void Brake()
        {
            leftFrontCollider.brakeTorque =  _currentBrakeForce;
            rightFrontCollider.brakeTorque = _currentBrakeForce;
            leftBackCollider.brakeTorque = _currentBrakeForce;
            rightBackCollider.brakeTorque = _currentBrakeForce;
        }

        private void ControlDrift()
        {
            if (leftFrontCollider.GetGroundHit(out WheelHit hit))
            {
                leftFrontTrail.emitting = false;
                if (hit.sidewaysSlip > carConfig.FrontSideSlipLimit || hit.sidewaysSlip < -carConfig.FrontSideSlipLimit)
                {
                    //driftSound.Play();
                    leftFrontTrail.emitting = true;
                }
            }

            if (rightFrontCollider.GetGroundHit(out hit))
            {
                rightFrontTrail.emitting = false;
                if (hit.sidewaysSlip > carConfig.FrontSideSlipLimit || hit.sidewaysSlip < -carConfig.FrontSideSlipLimit)
                {
                    //driftSound.Play();
                    rightFrontTrail.emitting = true;
                }
            }

            if (rightBackCollider.GetGroundHit(out hit))
            {
                rightBackTrail.emitting = false;
                if (hit.sidewaysSlip > carConfig.BackSideSlipLimit || hit.sidewaysSlip < -carConfig.BackSideSlipLimit)
                {
                    //driftSound.Play();
                    rightBackTrail.emitting = true;
                }
            }

            if (leftBackCollider.GetGroundHit(out hit))
            {
                leftBackTrail.emitting = false;
                if (hit.sidewaysSlip > carConfig.BackSideSlipLimit || hit.sidewaysSlip < -carConfig.BackSideSlipLimit)
                {
                    //driftSound.Play();
                    leftBackTrail.emitting = true;
                }

                if (hit.forwardSlip > 0.15)
                {
                    // Debug.LogError(hit.forwardSlip);
                    // Debug.LogError("Burnout");
                }
            }
        }

        private void Accelerate()
        {
            leftBackCollider.motorTorque = _wheelTorque * _gasInput;
            rightBackCollider.motorTorque = _wheelTorque * _gasInput;
            leftFrontCollider.motorTorque = _wheelTorque * _gasInput;
            rightFrontCollider.motorTorque = _wheelTorque * _gasInput;
        }

        public void SetGasInput(float gas)
        {
            _gasInput = gas;
        }

        public void SetTurnInput(float turn)
        {            
            targetTurnAngle = carConfig.MaxTurnAngle * turn;
        }

        private void Turn()
        {
            leftFrontCollider.steerAngle = _currentTurnAngle;
            rightFrontCollider.steerAngle = _currentTurnAngle;
        }

        private void UpdateWheelState(WheelCollider col, Transform trans)
        {
            col.GetWorldPose(out Vector3 position, out Quaternion rotation);
            trans.position = position;
            trans.rotation = rotation;
        }

        private void UpdateAllWheels()
        {
            UpdateWheelState(leftFrontCollider, leftFrontTransform);
            UpdateWheelState(rightFrontCollider, rightFrontTransform);
            UpdateWheelState(rightBackCollider, rightBackTransform);
            UpdateWheelState(leftBackCollider, leftBackTransform);
        }
    }
}