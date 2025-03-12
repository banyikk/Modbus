using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO.Ports;
using Modbus.Device;
using DG.Tweening;
using System.Net.Sockets;

public class ToggleConst
{
    public const string ��۲� = "��۲�";
    public const string �й۲� = "�й۲�";
    public const string �ҹ۲� = "�ҹ۲�";
    public const string ����� = "�����";
    public const string ͨ�� = "ͨ��";
    public const string ���� = "����";
    public const string ���� = "����";
    public const string ��ˮ = "��ˮ";
    public const string ���� = "����";
    public const string һ�Ÿ��� = "һ�Ÿ���";
    public const string ���Ÿ��� = "���Ÿ���";
    public const string ��������� = "���������";
    public const string �۲�1 = "�۲�1";
    public const string �۲�2 = "�۲�2";
    public const string �����豸1 = "�����豸1";
    public const string �����豸2 = "�����豸2";
    public const string ���� = "����";
    public const string ��ɽ���� = "��ɽ����";
    public const string ��Ч����ϵͳ = "��Ч����ϵͳ";
}

public class ControlPanel : MonoBehaviour
{
    public Transform _�������;
    public GameObject controltoggle;
    private Tween tween;


    //[Header("TCP����")]
    //public string ipAddress = "192.168.6.6"; // TCP IP ��ַ
    //public int port = 502; // TCP �˿�
    //[Header("��վ��ַ")]
    //public byte slaveAddress = 1;
    //private IModbusMaster master;
    //private TcpClient tcpClient;

    [Header("����ֵ")]
    public string portName = "COM3";
    [Header("������")]
    public int baudRate = 19200;
    [Header("��վ��ַ")]
    public byte slaveAddress = 1;
    private IModbusSerialMaster master;
    private SerialPort serialPort;

    public Dictionary<string, (List<ushort> registetrAddress, List<ushort> closeRegisterAddress, List<ushort> sensorAddress, string displayName)> ToggleConfigs;
    public Dictionary<string, (List<ushort> registerAddress, Vector2 valueRange, float updateInterval)> RegisterConfigs;
    private Dictionary<ushort, CancellationTokenSource> registerUpdateTasks;
    private Dictionary<Toggle, (List<ushort> registerAddress, List<ushort> closeRegisterAddress, bool state)> portConfigs;
    private Dictionary<Toggle, bool> isManuallyControlled;
    private readonly System.Random random = new System.Random();

    public Toggle[] cloneToggles;
    public CameraMove cameraMove;
    public RawImage[] videos;
    private RawImage currentActiveVideo;
    private Dictionary<string, bool> videoPlayedStatus = new Dictionary<string, bool>();
    public UiFunctionExpansion uiFunction;
    private void Awake()
    {
        
    }
    private void Start()
    {
        //TCP��ʼ��();
        ���ڳ�ʼ��();

        ToggleConfigs = new Dictionary<string, (List<ushort>, List<ushort>, List<ushort>, string)>
        {
            {ToggleConst.��۲�,(new List<ushort>{ },new List<ushort>{ },new List<ushort>{ },"��۲�") },
        {ToggleConst.�й۲�,(new List<ushort>{ },new List<ushort>{ },new List<ushort>{ },"�й۲�") },
        {ToggleConst.�ҹ۲�,(new List<ushort>{ },new List<ushort>{ },new List<ushort>{ },"�ҹ۲�") },
        {ToggleConst.�����,(new List<ushort>{ },new List<ushort>{ },new List<ushort>{ },"�����") },
        {ToggleConst.ͨ��,(new List<ushort>{ 0},new List<ushort>{ 0},new List<ushort>{40 },"ͨ��") },
        {ToggleConst.����,(new List < ushort > { 1 }, new List < ushort > { 1 }, new List<ushort>{ 41}, "����") },
         {ToggleConst.����,(new List < ushort > { 2 }, new List < ushort > { 2 }, new List<ushort>{ 42}, "����") },
          {ToggleConst.��ˮ,(new List < ushort > { 3 }, new List < ushort > { 3 }, new List<ushort>{43 }, "��ˮ") },
          {ToggleConst.����,(new List < ushort > { 4 }, new List < ushort > { 4 }, new List<ushort>{44 },"����") },
          {ToggleConst.һ�Ÿ���,(new List < ushort > { 5 }, new List < ushort > { 5 }, new List < ushort > { 45 }, "һ�Ÿ���")},
          {ToggleConst.���Ÿ���,(new List < ushort > { 6 }, new List < ushort > { 6 }, new List<ushort>{46 }, "���Ÿ���")},
          {ToggleConst.�۲�1,(new List < ushort > { 7 }, new List < ushort > { 7 }, new List<ushort>{47 }, "�۲�1")},
          {ToggleConst.�۲�2,(new List < ushort > { 8 }, new List < ushort > { 8 }, new List<ushort>{48 }, "�۲�2")},
          {ToggleConst.���������,(new List < ushort > { 9 }, new List < ushort > { 9 }, new List<ushort>{49 }, "���������")},
          {ToggleConst.�����豸1,(new List < ushort > { 10 }, new List < ushort > { 10 }, new List<ushort>{50 }, "�����豸1")},
          {ToggleConst.�����豸2,(new List < ushort > {11 }, new List < ushort > { 11 }, new List<ushort>{51 }, "�����豸2")},
          {ToggleConst.����,(new List < ushort > { 12 }, new List < ushort > { 12}, new List<ushort>{52 }, "����")},
          {ToggleConst.��ɽ����,(new List < ushort > { 13 }, new List < ushort > { 13 }, new List<ushort>{53 }, "��ɽ����")},
          {ToggleConst.��Ч����ϵͳ,(new List < ushort > { 14 }, new List < ushort > { 14 }, new List<ushort>{54 }, "��Ч����ϵͳ")},
        };

        RegisterConfigs = new Dictionary<string, (List<ushort> registerAddress, Vector2 valueRange, float updateInterval)>
        {
          { "31130��������˹",(new List<ushort>{ 16},new Vector2(0,999),10f)},
          { "31130������һ����̼",(new List<ushort>{ 17},new Vector2(0,24),10f)},
          { "ˮ��ˮλ",(new List<ushort>{ 18},new Vector2(0,3000),10f)},
          { "31080�������˹",(new List<ushort>{ 19},new Vector2(0,999),10f)},
          { "31080������¶�",(new List<ushort>{ 20},new Vector2(1900,3000),10f)},
          { "��14-31040�۲���˹",(new List<ushort>{ 21},new Vector2(0,999),10f)},
          { "��14-31040�۲ɶ�����̼",(new List<ushort>{ 22},new Vector2(0,500),10f)},
          { "�����²�ˮ��ˮλ",(new List<ushort>{ 23},new Vector2(1500,3000),10f)},
          { "����һ����������ɽ����",(new List<ushort>{ 24},new Vector2(0,2500),10f)},
          { "���븱������",(new List<ushort>{ 25},new Vector2(0,4000),10f)},
          { "����羮��ѹ",(new List<ushort>{ 26},new Vector2(0,2900),10f)},
          { "�����ϲ�ˮ��ˮλ",(new List<ushort>{ 27},new Vector2(1500,3000),10f)},
        };

        portConfigs = new Dictionary<Toggle, (List<ushort> registerAddress, List<ushort> closeRegisterAddress, bool state)>();
        registerUpdateTasks = new Dictionary<ushort, CancellationTokenSource>();
        isManuallyControlled = new Dictionary<Toggle, bool>();

        StartRegisterUpdate();
        StartSensorMonitoring();
        Onclick(true);
    }

    //private void TCP��ʼ��()
    //{
    //    try
    //    {
    //        tcpClient = new TcpClient(ipAddress, port);
    //        master = ModbusIpMaster.CreateIp(tcpClient);
    //        Debug.Log("TCP���ӳɹ�");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"TCP����ʧ��: {ex.Message}");
    //    }
    //}
    private void ���ڳ�ʼ��()
    {
        if (serialPort == null)
        {
            serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        if (!serialPort.IsOpen)
        {
            serialPort.Open();
            master = ModbusSerialMaster.CreateRtu(serialPort);
        }
    }

    public void Onclick(bool isOn)
    {
        if (isOn)
        {
            _�������.gameObject.SetActive(true);
            _�������.GetComponent<RectTransform>().localPosition = new Vector3(650, -343, 0);

            if (_�������.Find("ContentSizeFitter/GirdGroup").childCount == 0)
            {
                int i = 0;
                foreach (var config in ToggleConfigs)
                {
                    GameObject controlToggleClone = Instantiate(controltoggle, _�������.Find("ContentSizeFitter/GirdGroup"));
                    TextMeshProUGUI toggleText = controlToggleClone.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                    controlToggleClone.name = controltoggle.name + i.ToString();
                    i++;

                    if (toggleText != null)
                    {
                        toggleText.text = config.Value.displayName;
                    }
                }
            }

            cloneToggles = _�������.Find("ContentSizeFitter/GirdGroup").GetComponentsInChildren<Toggle>();
            foreach (var toggle in cloneToggles)
            {
                var config = ToggleConfigs.FirstOrDefault(c => c.Value.displayName == toggle.transform.Find("Name").GetComponent<TextMeshProUGUI>().text);
                if (config.Key != null)
                {
                    portConfigs[toggle] = (config.Value.registetrAddress, config.Value.closeRegisterAddress, false);
                    toggle.onValueChanged.AddListener((value) => {
                        MusicManager.Instance.��Чϵͳ����(AudioEffect.MouseDown);
                        OnToggleValueChanged(toggle, value, true);
                        if (cameraMove.enabled==true)
                        {
                        cameraMove.OnToggleValueChanged(toggle, isOn);
                            
                        }
                        });
                }
            }

            Graphic[] graphics = _�������.GetComponentsInChildren<Graphic>();
            foreach (var graphic in graphics)
            {
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0f);
                tween = graphic.DOFade(0.8f, 0.5f).SetEase(Ease.OutQuad);
            }
        }
        else
        {
            _�������.gameObject.SetActive(false);
            if (tween != null && tween.IsPlaying())
            {
                tween.Kill();
            }
        }
     

    }

    private void OnToggleValueChanged(Toggle toggle, bool isOn, bool isManual)
    {
        if (portConfigs.ContainsKey(toggle))
        {
            var currentConfig = portConfigs[toggle];
            portConfigs[toggle] = (currentConfig.registerAddress, currentConfig.closeRegisterAddress, isOn);

            if (isManual)
            {
                isManuallyControlled[toggle] = true;

                if (isOn)
                {
                    foreach (var registerAddress in currentConfig.registerAddress)
                    {
                        master.WriteSingleRegister(slaveAddress, registerAddress, 1);
                    }
                    if (cameraMove.enabled == true)
                    {
                        cameraMove.OnToggleValueChanged(toggle, true);
                    }
                }
                else
                {
                    foreach (var registerAddress in currentConfig.closeRegisterAddress)
                    {
                        master.WriteSingleRegister(slaveAddress, registerAddress, 0);
                    }
                    if (cameraMove.enabled == true)
                    {
                        cameraMove.OnToggleValueChanged(toggle, false);
                    }
                }
            }
        }
    }


    //�����д��

    private void StartRegisterUpdate()
    {
        foreach (var config in RegisterConfigs)
        {
            foreach (var register in config.Value.registerAddress)
            {
                if (!registerUpdateTasks.ContainsKey(register))
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    registerUpdateTasks[register] = cancellationTokenSource;
                    _ = UpdateRegisterValueAsync(register, config.Value.valueRange, config.Value.updateInterval, cancellationTokenSource.Token);
                }
            }
        }
    }

    private async Task UpdateRegisterValueAsync(ushort registerAddress, Vector2 valueRange, float interval, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            int randomValue = random.Next((int)valueRange.x, (int)valueRange.y + 1);

            try
            {
                master.WriteSingleRegister(slaveAddress, registerAddress, (ushort)randomValue);
                Debug.Log($"д��Ĵ��� {registerAddress}: ֵ {randomValue}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"д��Ĵ��� {registerAddress} ʱ����: {ex.Message}");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
    //��ȡ�Ĵ���
    private void StartSensorMonitoring()
    {
        foreach (var config in ToggleConfigs)
        {
            foreach (var coilAddress in config.Value.sensorAddress)
            {
                if (!registerUpdateTasks.ContainsKey(coilAddress))
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    registerUpdateTasks[coilAddress] = cancellationTokenSource;
                    _ = MonitorSensorAsync(coilAddress, config.Key, cancellationTokenSource.Token);
                }
            }
        }
    }

    private async Task MonitorSensorAsync(ushort registerAddress, string toggleKey, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                ushort[] registerValues = master.ReadHoldingRegisters(slaveAddress, registerAddress, 1);
                bool isActive = registerValues[0] > 0;

                var toggleConfig = ToggleConfigs.FirstOrDefault(c => c.Key == toggleKey);
                if (toggleConfig.Key != null)
                {
                    var toggle = portConfigs.FirstOrDefault(p => p.Value.registerAddress == toggleConfig.Value.registetrAddress).Key;
                    if (toggle != null)
                    {
                        // ����������ʱ�������ť״̬���ֶ����Ƶģ�ǿ�Ƹ���״̬
                        if (!isManuallyControlled.ContainsKey(toggle) || !isManuallyControlled[toggle])
                        {
                            UpdateToggleState(toggleKey, isActive);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"��ȡ�Ĵ��� {registerAddress} ʱ����: {ex.Message}");
            }

            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
    //��ȡ�Ĵ�����������ť�ϵ�Ч��
    private void UpdateToggleState(string toggleKey, bool isActive)
    {
        var toggleConfig = ToggleConfigs.FirstOrDefault(c => c.Key == toggleKey);
        if (toggleConfig.Key != null)
        {
            var toggle = portConfigs.FirstOrDefault(p => p.Value.registerAddress == toggleConfig.Value.registetrAddress).Key;
            if (toggle != null)
            {
                // ������ֶ����Ƶİ�ť�����ǲ���������״̬
                if (isManuallyControlled.ContainsKey(toggle) && isManuallyControlled[toggle] && toggle.isOn == isActive)
                {
                    return;
                }

                // ���� toggle ״̬
                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = isActive;
                toggle.onValueChanged.AddListener((value) =>
                {
                    MusicManager.Instance.��Чϵͳ����(AudioEffect.MouseDown);
                    OnToggleValueChanged(toggle, value, true);
                });

                if (isActive)
                {
                    if (cameraMove.enabled == true)
                    {
                        cameraMove.OnToggleValueChanged(toggle, isActive);
                    }

                    // ��鲢������Ƶ
                    if (!videoPlayedStatus.ContainsKey(toggleKey) || !videoPlayedStatus[toggleKey])
                    {
                        PlayVideo(toggleKey);
                        videoPlayedStatus[toggleKey] = true;
                    }

                    // ��������Ч��
                    if (toggleKey == "�۲�2")
                    {
                        uiFunction.�ϲ�ú(isActive);
                    }
                    if (toggleKey == "�۲�1")
                    {
                        uiFunction.�²�ú(isActive);
                    }
                    if (toggleKey == "���������")
                    {
                        uiFunction.�����ť(isActive);
                    }
                    if (toggleKey == "�����豸1")
                    {
                        uiFunction.�ܵ�͸��(isActive);
                    }
                }
                else
                {
                    StopVideo(toggleKey);
                    videoPlayedStatus[toggleKey] = false;
                }
            }
        }
    }

    private void PlayVideo(string toggleKey)
    {
        if (currentActiveVideo != null)
        {
            var previousPlayer = currentActiveVideo.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (previousPlayer != null)
            {
                previousPlayer.Stop();
                currentActiveVideo.gameObject.SetActive(false);
            }
        }

        var activeVideo = videos.FirstOrDefault(v => v.name == toggleKey);
        if (activeVideo != null)
        {
            currentActiveVideo = activeVideo;
            activeVideo.gameObject.SetActive(true);
            var videoPlayer = activeVideo.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.time = 0;
                videoPlayer.loopPointReached -= OnVideoEnd;
                videoPlayer.loopPointReached += OnVideoEnd;
                videoPlayer.Play();
            }
        }
    }

    private void StopVideo(string toggleKey)
    {
        var activeVideo = videos.FirstOrDefault(v => v.name == toggleKey);
        if (activeVideo != null && activeVideo == currentActiveVideo)
        {
            var videoPlayer = activeVideo.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                activeVideo.gameObject.SetActive(false);
                currentActiveVideo = null;
            }
        }
    }

    private void OnVideoEnd(UnityEngine.Video.VideoPlayer vp)
    {
        vp.Stop();
        vp.gameObject.SetActive(false);
        currentActiveVideo = null;
    }
    //ֹͣ���д��
    private void StopRegisterUpdate()
    {
        foreach (var cancellationTokenSource in registerUpdateTasks.Values)
        {
            cancellationTokenSource.Cancel();
        }

        registerUpdateTasks.Clear();
    }

    private void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            foreach (var entry in portConfigs)
            {
                foreach (var address in entry.Value.registerAddress)
                {
                    master.WriteSingleRegister(slaveAddress, address, 0);
                }
            }

            StopRegisterUpdate();
            serialPort.Close();
        }
    }
}
