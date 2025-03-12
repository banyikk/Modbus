using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public enum CameraState
{
    ȫ��,
    ����ϵͳ,
}


public class CameraScale : MonoBehaviour
{

    private Camera mainCamera;
    private Vector3 originTransform;
    private Quaternion originRotation;
    private Vector3 nowRotation;
    private Vector3 originEuA;
    //��ת���Ƕ�
    private int yRotationMinLimit = -30;
    private int yRotationMaxLimit = 30;
    private int xRotationMinLimit = -30;
    private int xRotationMaxLimit = 30;
    //��ת�ٶ�
    public float xRotationSpeed = 300f;
    public float yRotationSpeed = 300f;
    //��ת�Ƕ�
    private float xRotation;
    private float yRotation;

    //fov �����С�Ƕ�
    public int fovMinLimit = 5;
    public int fovMaxLimit = 60;
    //fov �仯�ٶ�
    public float fovSpeed = 100.0f;
    //fov �Ƕ�
    private float fov = 0.0f;
    private bool isRestoringFov = false;

    private CameraMove cameraMove;
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError(GetType() + "camera Get Error ����");
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
            // ��fov�Ѿ��ӽ�Ŀ��fovֵʱ��ֹͣ��ֵ�ָ�����
            if (Mathf.Abs(fovMaxLimit - fov) < 0.01f)
            {
                isRestoringFov = false;
            }
        }
    }
    /// <summary>
    /// ����ƶ�������ת
    /// </summary>
    void CameraRotateAndFOV()
    {
        #region Camera Rotation
        if (Input.GetMouseButton(0))
        {

            mainCamera.DOKill();
            cameraMove.enabled = false;
            //Input.GetAxis("MouseX")��ȡ����ƶ���X��ľ���
            xRotation -= Input.GetAxis("Mouse X") * xRotationSpeed * 0.02f;
            yRotation += Input.GetAxis("Mouse Y") * yRotationSpeed * 0.02f;


            yRotation = ClampValue(yRotation, yRotationMinLimit, yRotationMaxLimit);//��������ڽ�β
            xRotation = ClampValue(xRotation, xRotationMinLimit, xRotationMaxLimit);//��������ڽ�β

            //ŷ����ת��Ϊ��Ԫ��
            //Quaternion rotation = Quaternion.Euler(-yRotation, -xRotation, 0);
            Quaternion rotation = Quaternion.Euler(originEuA.x - yRotation, originEuA.y - xRotation, originEuA.z);

            mainCamera.transform.DOLocalRotateQuaternion(rotation, 0.8f);
            nowRotation = mainCamera.transform.localEulerAngles;
            #endregion

            #region Camera fov
            //��ȡ�����ֵĻ�����
            fov -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 100 * fovSpeed;

            // fov ��������
            fov = Mathf.Clamp(fov, fovMinLimit, fovMaxLimit);
            //fov = Mathf.Lerp(fov, targetFov, Time.deltaTime*fovSpeed);
            //�ı������ fov
            mainCamera.fieldOfView = fov;
        }
        #endregion
        if (Input.GetMouseButtonDown(0))
        {

            //���CameraMove���ж�
            if (cameraMove.cameraState == CameraState.ȫ��)//���ص���ʼλ��
            {
                yRotationMinLimit = -30;
                yRotationMaxLimit = 30;
                xRotationMinLimit = -30;
                xRotationMaxLimit = 30;

            }
            else if (cameraMove.cameraState == CameraState.����ϵͳ)
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
    public void �˳�����()
    {
        transform.DOMove(originTransform, 0.8f).SetEase(Ease.OutQuad);
        transform.DORotate(originRotation.eulerAngles, 0.8f).SetEase(Ease.OutQuad);
        cameraMove.enabled = true;
        isRestoringFov = true;
    }

    #region tools ClampValue

    //ֵ��Χֵ�޶�
    float ClampValue(float value, float min, float max)//������ת�ĽǶ�
    {
        if (value < -360)
            value += 360;
        if (value > 360)
            value -= 360;
        return Mathf.Clamp(value, min, max);//����value��ֵ��min��max֮�䣬 ���valueС��min������min�� ���value����max������max�����򷵻�value
    }


    #endregion

}
