using Configs;
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

        private float _currentBrakeForce;
        private float _currentHandbrakeForce;
        private float _currentTurnAngle;

        private int _gear;

        private float _wheelTorque;
        private float _gasInput;

        private float _maxSpeedMs;
        private float _currentSpeedKmh;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
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
            Handbrake();
            Turn();
            ControlDrift();
        }

        private void LateUpdate()
        {
            UpdateAllWheels();
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

                Debug.LogError($"IsCarRunning: {_isCarRunning}");
            }

            _gasInput = Input.GetAxis("Vertical");


            switch (_gear)
            {
                case 1:
                    _maxSpeedMs = carConfig.GearSpeedLimits[0];
                    _wheelTorque = carConfig.EngineTorque * carConfig.GearRatios[4] /
                                   carConfig.TransmissionEfficiency;
                    break;
                case 2:
                    _maxSpeedMs = carConfig.GearSpeedLimits[1];
                    _wheelTorque = carConfig.EngineTorque * carConfig.GearRatios[3] /
                                   carConfig.TransmissionEfficiency;
                    break;
                case 3:
                    _maxSpeedMs = carConfig.GearSpeedLimits[2];
                    _wheelTorque = carConfig.EngineTorque * carConfig.GearRatios[2] /
                                   carConfig.TransmissionEfficiency;
                    break;
                case 4:
                    _maxSpeedMs = carConfig.GearSpeedLimits[3];
                    _wheelTorque = carConfig.EngineTorque * carConfig.GearRatios[1] /
                                   carConfig.TransmissionEfficiency;
                    break;
                case 5:
                    _maxSpeedMs = carConfig.GearSpeedLimits[4];
                    _wheelTorque = carConfig.EngineTorque * carConfig.GearRatios[0] /
                                   carConfig.TransmissionEfficiency;
                    break;
                case 0:
                    _maxSpeedMs = car.velocity.magnitude;
                    _wheelTorque = 0;
                    break;
                case < 0:
                    _maxSpeedMs = carConfig.GearSpeedLimits[0];
                    _wheelTorque = carConfig.EngineTorque * carConfig.GearRatios[4] /
                        carConfig.TransmissionEfficiency * -1;
                    break;
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                _gear = 0;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _gear = 1;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _gear = 2;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _gear = 3;
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _gear = 4;
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                _gear = 5;
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                _gear = -1;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (_gear < 5)
                {
                    _gear++;
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (_gear > -1)
                {
                    _gear--;
                }
            }

            _currentSpeedKmh = Mathf.Round(car.velocity.magnitude) * KmhCoeff;
            Debug.LogError($"Speed: {_currentSpeedKmh} km/h");
            Debug.LogError($"Gear: {_gear}");

            _currentTurnAngle = carConfig.MaxTurnAngle * Input.GetAxis("Horizontal");
            _currentBrakeForce = 0;

            if (Input.GetKey(KeyCode.DownArrow))
            {
                _currentBrakeForce = carConfig.BrakingForce;
            }

            _currentHandbrakeForce = 0;
            if (Input.GetKey(KeyCode.Space))
            {
                _currentHandbrakeForce = carConfig.HandbrakingForce;
            }
        }

        private void Handbrake()
        {
            leftBackCollider.brakeTorque = _currentHandbrakeForce;
            rightBackCollider.brakeTorque = _currentHandbrakeForce;
        }

        private void ControlDrift()
        {
            if (leftFrontCollider.GetGroundHit(out WheelHit hit))
            {
                leftFrontTrail.emitting = false;
                if (hit.sidewaysSlip > carConfig.FrontSideSlipLimit || hit.sidewaysSlip < - carConfig.FrontSideSlipLimit)
                {
                    //driftSound.Play();
                    leftFrontTrail.emitting = true;
                }
            }

            if (rightFrontCollider.GetGroundHit(out hit))
            {
                rightFrontTrail.emitting = false;
                if (hit.sidewaysSlip > carConfig.FrontSideSlipLimit || hit.sidewaysSlip < - carConfig.FrontSideSlipLimit)
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

        private void Brake()
        {
            leftFrontCollider.brakeTorque = _currentBrakeForce;
            rightFrontCollider.brakeTorque = _currentBrakeForce;
            rightBackCollider.brakeTorque = _currentBrakeForce;
            leftBackCollider.brakeTorque = _currentBrakeForce;
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