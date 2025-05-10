using _project.Character;
using UnityEngine;

public class CharacterMover : MonoBehaviour
{
    public Vector3 target;

    CharacterSc _character;

    private void Start()
    {
        _character = GetComponent<CharacterSc>();
    }

    void LateUpdate()
    {
        if (target == Vector3.zero)
            return;
            
        transform.position = Vector3.MoveTowards(transform.position, target, 2f * Time.deltaTime);

        float dist = Vector3.Distance(transform.position, target);
        if (dist < 0.025f)
            _character.StopMove();
    }
}
