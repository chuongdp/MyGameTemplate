namespace MyGame.Script.Systems
{
    using UnityEngine;
    using GameFoundation.DI;
    using GameFoundation.Signals;

    public class UserInputSystem : ITickable
    {
        private readonly SignalBus signalBus;

        public UserInputSystem(SignalBus signalBus) { this.signalBus = signalBus; }

        private Vector3 touchStartPosition;

        public void Tick()
        {
            switch (Input.touchCount)
            {
                case 1:
                    var touch = Input.GetTouch(0);
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            this.touchStartPosition = touch.position;
                            // Begin touch

                            break;
                        case TouchPhase.Moved:
                            // Dragging

                            break;
                        case TouchPhase.Stationary:
                            break;
                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            // End touch

                            break;
                    }

                    break;
                case 2: // Zoom with two fingers

                    break;
            }
        }
    }
}