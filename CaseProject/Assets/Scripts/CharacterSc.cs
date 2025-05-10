using _project.Settings;
using _project.Enums;
using UnityEngine;
using DG.Tweening;

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

            _mover.enabled = false;
        }

        public void ChangeColor(ColorEnum color)
        {
            _settingSO = SettingSO.Instance;
            _childMeshRendererTr = transform.GetChild(0);
            _childMeshRendererTr.GetComponent<SkinnedMeshRenderer>().material = _settingSO.PickColor(color);
            MyColor = color;
        }

        public void MovePos(Vector3 target)
        {
            _mover.enabled = true;
            _mover.target = target;
            _animator.SetBool("Run", true);
        }

        public void Jump(Transform JumpPos)
        {
            if (JumpPos != null)
            {
                transform.DOJump(JumpPos.position, 2f, 0, 0.65f).OnComplete(() => { _animator.Play("SitIdle"); transform.SetParent(JumpPos); });
                transform.DORotate(JumpPos.eulerAngles, 0.5f);
                _animator.Play("Jump");
            }
        }

        public void StopMove()
        {
            _animator.SetBool("Run", false);
            _mover.target = Vector3.zero;
            _mover.enabled = false;
        }
    }
}