using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public enum CameraState
{
    全景,
    进入系统,
}


public class CameraScale : MonoBehaviour
{

    private Camera mainCamera;
    private Vector3 originTransform;
    private Quaternion originRotation;
    private Vector3 nowRotation;
    private Vector3 originEuA;
    //旋转最大角度
    private int yRotationMinLimit = -30;
    private int yRotationMaxLimit = 30;
    private int xRotationMinLimit = -30;
    private int xRotationMaxLimit = 30;
    //旋转速度
    public float xRotationSpeed = 300f;
    public float yRotationSpeed = 300f;
    //旋转角度
    private float xRotation;
    private float yRotation;

    //fov 最大最小角度
    public int fovMinLimit = 5;
    public int fovMaxLimit = 60;
    //fov 变化速度
    public float fovSpeed = 100.0f;
    //fov 角度
    private float fov = 0.0f;
    private bool isRestoringFov = false;

    private CameraMove cameraMove;
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError(GetType() + "camera Get Error ……");
        }
        cameraMove = GameObject.Find("Main Camera").GetComponent<CameraMove>();
    }
    private void Start()
    {

        fov = mainCamera.fieldOfView;
        originTransform = mainCamera.transform.localPosition;
        originRotation = mainCamera.transform.localRotation;
        originEuA = mainCamera.transform.localEulerAngles;
    }

    private void Update()
    {
        CameraRotateAndFOV();
        if (isRestoringFov)
        {
            fov = Mathf.Lerp(fov, fovMaxLimit, Time.deltaTime * 100);
            mainCamera.fieldOfView = fov;
            // 当fov已经接近目标fov值时，停止插值恢复操作
            if (Mathf.Abs(fovMaxLimit - fov) < 0.01f)
            {
                isRestoringFov = false;
            }
        }
    }
    /// <summary>
    /// 鼠标移动进行旋转
    /// </summary>
    void CameraRotateAndFOV()
    {
        #region Camera Rotation
        if (Input.GetMouseButton(0))
        {

            mainCamera.DOKill();
            cameraMove.enabled = false;
            //Input.GetAxis("MouseX")获取鼠标移动的X轴的距离
            xRotation -= Input.GetAxis("Mouse X") * xRotationSpeed * 0.02f;
            yRotation += Input.GetAxis("Mouse Y") * yRotationSpeed * 0.02f;


            yRotation = ClampValue(yRotation, yRotationMinLimit, yRotationMaxLimit);//这个函数在结尾
            xRotation = ClampValue(xRotation, xRotationMinLimit, xRotationMaxLimit);//这个函数在结尾

            //欧拉角转化为四元数
            //Quaternion rotation = Quaternion.Euler(-yRotation, -xRotation, 0);
            Quaternion rotation = Quaternion.Euler(originEuA.x - yRotation, originEuA.y - xRotation, originEuA.z);

            mainCamera.transform.DOLocalRotateQuaternion(rotation, 0.8f);
            nowRotation = mainCamera.transform.localEulerAngles;
            #endregion

            #region Camera fov
            //获取鼠标滚轮的滑动量
            fov -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 100 * fovSpeed;

            // fov 限制修正
            fov = Mathf.Clamp(fov, fovMinLimit, fovMaxLimit);
            //fov = Mathf.Lerp(fov, targetFov, Time.deltaTime*fovSpeed);
            //改变相机的 fov
            mainCamera.fieldOfView = fov;
        }
        #endregion
        if (Input.GetMouseButtonDown(0))
        {

            //配和CameraMove的判断
            if (cameraMove.cameraState == CameraState.全景)//若回到初始位置
            {
                yRotationMinLimit = -30;
                yRotationMaxLimit = 30;
                xRotationMinLimit = -30;
                xRotationMaxLimit = 30;

            }
            else if (cameraMove.cameraState == CameraState.进入系统)
            {
                yRotationMinLimit = -100;
                yRotationMaxLimit = 100;
                xRotationMinLimit = -100;
                xRotationMaxLimit = 100;
            }

        }

        if (Input.GetMouseButtonUp(0))
        {
            mainCamera.transform.localEulerAngles = nowRotation;
        }

    }
    public void 退出缩放()
    {
        transform.DOMove(originTransform, 0.8f).SetEase(Ease.OutQuad);
        transform.DORotate(originRotation.eulerAngles, 0.8f).SetEase(Ease.OutQuad);
        cameraMove.enabled = true;
        isRestoringFov = true;
    }

    #region tools ClampValue

    //值范围值限定
    float ClampValue(float value, float min, float max)//控制旋转的角度
    {
        if (value < -360)
            value += 360;
        if (value > 360)
            value -= 360;
        return Mathf.Clamp(value, min, max);//限制value的值在min和max之间， 如果value小于min，返回min。 如果value大于max，返回max，否则返回value
    }


    #endregion

}
