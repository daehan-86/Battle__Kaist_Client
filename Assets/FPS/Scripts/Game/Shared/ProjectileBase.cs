using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        public GameObject Owner { get; private set; }
        public Vector3 InitialPosition { get; private set; }
        public Vector3 InitialDirection { get; private set; }
        public Vector3 InheritedMuzzleVelocity { get; private set; }
        public float InitialCharge { get; private set; }

        public UnityAction OnShoot;

        public void Shoot(WeaponController controller)
        {
            Owner = controller.Owner;
            InitialPosition = transform.position;
            InitialDirection = transform.forward;
            InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;
            InitialCharge = controller.CurrentCharge;

            OnShoot?.Invoke();
        }
        public void Shoot(GameObject customOwner, Vector3 pos, Vector3 dir)
        {
            Owner = customOwner;

            // 원하는 위치로 설정
            transform.position = pos;

            // 원하는 방향으로 회전
            transform.rotation = Quaternion.LookRotation(dir);

            // 추가적으로 내부 변수도 세팅
            InitialPosition = pos;
            InitialDirection = dir.normalized;
            InheritedMuzzleVelocity = Vector3.zero;
            InitialCharge = 0f;

            OnShoot?.Invoke();
        }
    }
}