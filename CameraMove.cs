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


    // 当 Toggle 状态发生变化时触发
    public void OnToggleValueChanged(Toggle toggle, bool isOn)
    {
        if (isOn)
        {
            print(1);
            cameraState = CameraState.进入系统;

            // 使用按钮名称匹配位置
            string toggleName = toggle.transform.Find("Name").GetComponent<TextMeshProUGUI>().text;
            Transform targetPos = Pos.FirstOrDefault(p => p.name == toggleName);

            if (targetPos != null)
            {
                CameraToMove(toggle, targetPos);
            }
        }

            // 如果所有按钮都未激活，则回到全景模式
            if (!_controlPanel.cloneToggles.Any(t => t.isOn))
            {
                
                cameraState = CameraState.全景;
                CameraScale.退出缩放();
            }
        
    }

    public void CameraToMove(Toggle toggle, Transform targetPos)
    {
        if (targetPos == null) return;

        // 清除之前的动画
        transform.DOKill();

        // 强制摄像机移动到目标位置
        transform.DOMove(targetPos.position, 0.8f).SetEase(Ease.OutQuad);
        transform.DORotateQuaternion(targetPos.rotation, 0.8f).SetEase(Ease.OutQuad);
    }

    public void MoveToCamera()
    {
        transform.DOKill(); // 清除之前的动画
        transform.DOMove(originPos, 0.8f).SetEase(Ease.OutQuad);
        transform.DORotateQuaternion(originQua, 0.8f).SetEase(Ease.OutQuad);
    }
}



