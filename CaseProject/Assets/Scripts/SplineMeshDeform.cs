using _project.Car;
using _project.Grid;
using SplineMesh;
using UnityEngine;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(Spline))]
public class SplineMeshDeform : MonoBehaviour
{
    public bool Selected;

    [Header("Uç Objeler")]
    [Tooltip("Spline’ın arka ucu olarak kullanılacak parça.")]
    [SerializeField] private Transform ReplaceObj;
    [Tooltip("Spline’ın ön ucu olarak kullanılacak parça.")]
    [SerializeField] private Transform FollowObj;
    [Tooltip("Spline’ın ön ucu olarak kullanılacak parça degisken.")]
    [SerializeField] private Transform HeadOrTail;

    [Header("Offset Miktarları")]
    [Tooltip("ReplaceObj üzerine eklenen offset (metre).")]
    [SerializeField] private float ReplaceOffset = 0.5f;
    [Tooltip("FollowObj üzerine eklenen offset (metre).")]
    [SerializeField] private float FollowOffset = 0.5f;
    [Tooltip("FollowObj Head ise.")]
    [SerializeField] private bool Ishead;
    [Tooltip("FollowObj Tail ise.")]
    [SerializeField] private bool IsTail;

    [Header("Handle Uzunluğu")]
    [Tooltip("Her node’dan sapma uzunluğu (metre).")]
    [SerializeField] private float handleLength = 1f;

    [SerializeField] private CarCountainer _carCountainer;
    [SerializeField] private bool HeadOrTailFollow;
    [SerializeField] private Transform _followObj;

    private Spline _spline;
    private SplineNode[] _nodes;
    private GridManager _gridManager;
    private SplineMeshTiling _tilling;

    void Start()
    {
        _gridManager = GridManager.GridManagerScript;
        _carCountainer = ReplaceObj.transform.parent.GetComponent<CarCountainer>();
        _tilling = GetComponent<SplineMeshTiling>();
        _spline = GetComponent<Spline>();

        _carCountainer.DeformMesh.Add(this);
        _gridManager.CurrentMouseSelectedCarPart.Subscribe(CheckSelect);

        // 1) Mevcut node'ları temizle, iki yeni placeholder ekle
        _spline.nodes.Clear();
        _spline.AddNode(new SplineNode(Vector3.zero, Vector3.forward));
        _spline.AddNode(new SplineNode(Vector3.one, Vector3.one + Vector3.forward));

        // 2) Cache
        _nodes = _spline.nodes.ToArray();

        _tilling.material = ReplaceObj.GetComponent<MeshRenderer>().materials[0];

        // 3) İlk güncelleme
        UpdateElement();
    }

    void CheckSelect(CarPart selectedpart)
    {
        Selected = false;

        foreach (var item in _carCountainer.AllPart)
        {
            if (item == selectedpart)
            {
                Selected = true;
                break;
            }
        }
    }

    public void BecameNull()
    {
        _tilling.enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
        this.enabled = false;
    }

    void LateUpdate()
    {
        if(Selected)
            UpdateElement();
    }

    void UpdateElement()
    {
        if(_gridManager.CurrentMouseSelectedCarPart.Value != null)
        {
            if (_gridManager.CurrentMouseSelectedCarPart.Value.IsHead)
            {
                if(Ishead || IsTail)
                    _followObj = HeadOrTail;

                HeadOrTailFollow = _gridManager.CurrentMouseSelectedCarPart.Value.IsTail;
            }
        }

        if (_followObj == null || !HeadOrTailFollow)
            _followObj = FollowObj;

        if (!HeadOrTailFollow)
        {
            // A) ReplaceObj (arka uç) ve FollowObj (ön uç) pozisyonları + offset
            Vector3 tailPos = ReplaceObj.position - (ReplaceObj.forward * ReplaceOffset);
            Vector3 headPos = _followObj.position + (_followObj.forward * FollowOffset);

            // B) Node pozisyonlarını ata
            _nodes[0].Position = tailPos;
            _nodes[1].Position = headPos;

            // D) Handle’ları ileri/geri yönde ata
            _nodes[0].Direction = tailPos - (ReplaceObj.forward / 5f);
            _nodes[1].Direction = headPos + (_followObj.forward / 2f);
        }
        else
        {
            // 1) Pozisyonlar (aynı forward±offset değil, ikisi de geriye çekiliyor)
            Vector3 tailPos = ReplaceObj.position - ReplaceObj.forward * FollowOffset;
            Vector3 headPos = _followObj.position + _followObj.forward * ReplaceOffset;

            _nodes[0].Position = headPos;
            _nodes[1].Position = tailPos;

            // D) Handle’ları ileri/geri yönde ata
            _nodes[0].Direction = tailPos - (ReplaceObj.forward / 2f);
            _nodes[1].Direction = headPos + (_followObj.forward / 5f);
        }
    }
}