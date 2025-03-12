using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;




public class CameraMove : MonoBehaviour
{
    public CameraState cameraState;
    private ControlPanel _controlPanel;
    private CameraScale CameraScale;
    private Vector3 originPos;
    private Quaternion originQua;
    public List<Transform> Pos;

    void Start()
    {
        originPos = transform.position;
        originQua = transform.rotation;
        _controlPanel = GameObject.Find("Scripts").GetComponent<ControlPanel>();
        CameraScale=transform.GetComponent<CameraScale>();


    }


    // �� Toggle ״̬�����仯ʱ����
    public void OnToggleValueChanged(Toggle toggle, bool isOn)
    {
        if (isOn)
        {
            print(1);
            cameraState = CameraState.����ϵͳ;

            // ʹ�ð�ť����ƥ��λ��
            string toggleName = toggle.transform.Find("Name").GetComponent<TextMeshProUGUI>().text;
            Transform targetPos = Pos.FirstOrDefault(p => p.name == toggleName);

            if (targetPos != null)
            {
                CameraToMove(toggle, targetPos);
            }
        }

            // ������а�ť��δ�����ص�ȫ��ģʽ
            if (!_controlPanel.cloneToggles.Any(t => t.isOn))
            {
                
                cameraState = CameraState.ȫ��;
                CameraScale.�˳�����();
            }
        
    }

    public void CameraToMove(Toggle toggle, Transform targetPos)
    {
        if (targetPos == null) return;

        // ���֮ǰ�Ķ���
        transform.DOKill();

        // ǿ��������ƶ���Ŀ��λ��
        transform.DOMove(targetPos.position, 0.8f).SetEase(Ease.OutQuad);
        transform.DORotateQuaternion(targetPos.rotation, 0.8f).SetEase(Ease.OutQuad);
    }

    public void MoveToCamera()
    {
        transform.DOKill(); // ���֮ǰ�Ķ���
        transform.DOMove(originPos, 0.8f).SetEase(Ease.OutQuad);
        transform.DORotateQuaternion(originQua, 0.8f).SetEase(Ease.OutQuad);
    }
}



