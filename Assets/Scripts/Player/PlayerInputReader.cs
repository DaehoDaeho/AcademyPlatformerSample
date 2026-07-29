using UnityEngine;

    /// <summary>키보드의 이동과 점프 입력을 읽어 다른 플레이어 컴포넌트에 제공합니다.</summary>
    public sealed class PlayerInputReader : MonoBehaviour
    {
        /// <summary>현재 수평 이동 입력값을 제공합니다.</summary>
        public float Move { get; private set; }
        /// <summary>현재 프레임에 점프 키를 눌렀는지 제공합니다.</summary>
        public bool JumpPressed { get; private set; }
        /// <summary>점프 키를 계속 누르고 있는지 제공합니다.</summary>
        public bool JumpHeld { get; private set; }

        /// <summary>매 프레임 이동 및 점프 키 입력을 읽습니다.</summary>
        private void Update()
        {
            Move = 0f;
            bool movingLeft = Input.GetKey(KeyCode.A) == true ||
                Input.GetKey(KeyCode.LeftArrow) == true; // 왼쪽 이동 키 중 하나가 눌려 있는지 여부입니다.
            bool movingRight = Input.GetKey(KeyCode.D) == true ||
                Input.GetKey(KeyCode.RightArrow) == true; // 오른쪽 이동 키 중 하나가 눌려 있는지 여부입니다.
            if (movingLeft == true)
            {
                Move -= 1f;
            }
            if (movingRight == true)
            {
                Move += 1f;
            }

            // GetKeyDown은 이번 프레임에 처음 눌린 순간, GetKey는 계속 누르는 상태를 의미합니다.
            JumpPressed = Input.GetKeyDown(KeyCode.Space);
            JumpHeld = Input.GetKey(KeyCode.Space);
        }
    }
