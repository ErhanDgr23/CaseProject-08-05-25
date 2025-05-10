using _project.Character;
using UnityEngine;

public class CharacterMover : MonoBehaviour
{
    public Transform target;

    CharacterSc _character;

    private void Start()
    {
        _character = GetComponent<CharacterSc>();
        enabled = false;
    }

    void LateUpdate()
    {
        if (target == null)
            return;
            
        transform.position = Vector3.MoveTowards(transform.position, target.position, 2f * Time.deltaTime);

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist < 1f)
            _character.StopMove();
    }
}
