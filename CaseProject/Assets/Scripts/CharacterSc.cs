using _project.Settings;
using _project.Enums;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace _project.Character
{
    public class CharacterSc : MonoBehaviour
    {
        public ColorEnum MyColor;

        Transform _childMeshRendererTr;
        CharacterMover _mover;
        SettingSO _settingSO;
        Animator _animator;

        private void Start()
        {
            _mover = GetComponent<CharacterMover>();
            _animator = GetComponent<Animator>();
        }

        public void ChangeColor(ColorEnum color)
        {
            _settingSO = SettingSO.Instance;
            _childMeshRendererTr = transform.GetChild(0);
            _childMeshRendererTr.GetComponent<SkinnedMeshRenderer>().material = _settingSO.PickColor(color);
            MyColor = color;
        }

        public void MovePos(Transform target)
        {
            _mover.enabled = true;
            _mover.target = target;
            _animator.SetBool("Run", true);
        }

        public void StopMove()
        {
            _mover.target = null;
            _mover.enabled = false;
            _animator.SetBool("Run", false);
        }
    }
}