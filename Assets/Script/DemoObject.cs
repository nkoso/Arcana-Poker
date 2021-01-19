using UnityEngine;

public class DemoObject : MonoBehaviour
{
    [SerializeField]
    private float m_speed = 3.0f;

    private PhotonView m_photonView = null;
    private Renderer m_render = null;

    private readonly Color[] MATERIAL_COLORS = new Color[]
    {
        Color.white, Color.red, Color.green, Color.blue, Color.green,
    };

    void Awake()
    {
        m_photonView = GetComponent<PhotonView>();
        m_render = GetComponent<Renderer>();
    }

    void Start()
    {
        int ownerID = m_photonView.ownerId;
        m_render.material.color = MATERIAL_COLORS[ownerID];
    }

    void Update()
    {
        // 持ち主でないのなら制御させない
        if (!m_photonView.isMine)
        {
            return;
        }

        Vector3 pos = transform.position;

        pos.x += Input.GetAxis("Horizontal") * m_speed * Time.deltaTime;
        pos.y += Input.GetAxis("Vertical") * m_speed * Time.deltaTime;

        transform.position = pos;
    }
}
