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
    public const string 左观察 = "左观察";
    public const string 中观察 = "中观察";
    public const string 右观察 = "右观察";
    public const string 左侧面 = "左侧面";
    public const string 通风 = "通风";
    public const string 反风 = "反风";
    public const string 供电 = "供电";
    public const string 排水 = "排水";
    public const string 运输 = "运输";
    public const string 一号副井 = "一号副井";
    public const string 二号副井 = "二号副井";
    public const string 掘进工作面 = "掘进工作面";
    public const string 综采1 = "综采1";
    public const string 综采2 = "综采2";
    public const string 制冷设备1 = "制冷设备1";
    public const string 制冷设备2 = "制冷设备2";
    public const string 照明 = "照明";
    public const string 北山副井 = "北山副井";
    public const string 高效降温系统 = "高效降温系统";
}

public class ControlPanel : MonoBehaviour
{
    public Transform _控制面板;
    public GameObject controltoggle;
    private Tween tween;


    //[Header("TCP设置")]
    //public string ipAddress = "192.168.6.6"; // TCP IP 地址
    //public int port = 502; // TCP 端口
    //[Header("从站地址")]
    //public byte slaveAddress = 1;
    //private IModbusMaster master;
    //private TcpClient tcpClient;

    [Header("串口值")]
    public string portName = "COM3";
    [Header("波特率")]
    public int baudRate = 19200;
    [Header("从站地址")]
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
        //TCP初始化();
        串口初始化();

        ToggleConfigs = new Dictionary<string, (List<ushort>, List<ushort>, List<ushort>, string)>
        {
            {ToggleConst.左观察,(new List<ushort>{ },new List<ushort>{ },new List<ushort>{ },"左观察") },
        {ToggleConst.中观察,(new List<ushort>{ },new List<ushort>{ },new List<ushort>{ },"中观察") },
        {ToggleConst.右观察,(new List<ushort>{ },new List<ushort>{ },new List<ushort>{ },"右观察") },
        {ToggleConst.左侧面,(new List<ushort>{ },new List<ushort>{ },new List<ushort>{ },"左侧面") },
        {ToggleConst.通风,(new List<ushort>{ 0},new List<ushort>{ 0},new List<ushort>{40 },"通风") },
        {ToggleConst.反风,(new List < ushort > { 1 }, new List < ushort > { 1 }, new List<ushort>{ 41}, "反风") },
         {ToggleConst.供电,(new List < ushort > { 2 }, new List < ushort > { 2 }, new List<ushort>{ 42}, "供电") },
          {ToggleConst.排水,(new List < ushort > { 3 }, new List < ushort > { 3 }, new List<ushort>{43 }, "排水") },
          {ToggleConst.运输,(new List < ushort > { 4 }, new List < ushort > { 4 }, new List<ushort>{44 },"运输") },
          {ToggleConst.一号副井,(new List < ushort > { 5 }, new List < ushort > { 5 }, new List < ushort > { 45 }, "一号副井")},
          {ToggleConst.二号副井,(new List < ushort > { 6 }, new List < ushort > { 6 }, new List<ushort>{46 }, "二号副井")},
          {ToggleConst.综采1,(new List < ushort > { 7 }, new List < ushort > { 7 }, new List<ushort>{47 }, "综采1")},
          {ToggleConst.综采2,(new List < ushort > { 8 }, new List < ushort > { 8 }, new List<ushort>{48 }, "综采2")},
          {ToggleConst.掘进工作面,(new List < ushort > { 9 }, new List < ushort > { 9 }, new List<ushort>{49 }, "掘进工作面")},
          {ToggleConst.制冷设备1,(new List < ushort > { 10 }, new List < ushort > { 10 }, new List<ushort>{50 }, "制冷设备1")},
          {ToggleConst.制冷设备2,(new List < ushort > {11 }, new List < ushort > { 11 }, new List<ushort>{51 }, "制冷设备2")},
          {ToggleConst.照明,(new List < ushort > { 12 }, new List < ushort > { 12}, new List<ushort>{52 }, "照明")},
          {ToggleConst.北山副井,(new List < ushort > { 13 }, new List < ushort > { 13 }, new List<ushort>{53 }, "北山副井")},
          {ToggleConst.高效降温系统,(new List < ushort > { 14 }, new List < ushort > { 14 }, new List<ushort>{54 }, "高效降温系统")},
        };

        RegisterConfigs = new Dictionary<string, (List<ushort> registerAddress, Vector2 valueRange, float updateInterval)>
        {
          { "31130工作面瓦斯",(new List<ushort>{ 16},new Vector2(0,999),10f)},
          { "31130工作面一氧化碳",(new List<ushort>{ 17},new Vector2(0,24),10f)},
          { "水仓水位",(new List<ushort>{ 18},new Vector2(0,3000),10f)},
          { "31080掘进巷瓦斯",(new List<ushort>{ 19},new Vector2(0,999),10f)},
          { "31080掘进巷温度",(new List<ushort>{ 20},new Vector2(1900,3000),10f)},
          { "已14-31040综采瓦斯",(new List<ushort>{ 21},new Vector2(0,999),10f)},
          { "已14-31040综采二氧化碳",(new List<ushort>{ 22},new Vector2(0,500),10f)},
          { "已七下部水仓水位",(new List<ushort>{ 23},new Vector2(1500,3000),10f)},
          { "已七一期主运输下山风速",(new List<ushort>{ 24},new Vector2(0,2500),10f)},
          { "中央副井风速",(new List<ushort>{ 25},new Vector2(0,4000),10f)},
          { "中央风井负压",(new List<ushort>{ 26},new Vector2(0,2900),10f)},
          { "已七上部水仓水位",(new List<ushort>{ 27},new Vector2(1500,3000),10f)},
        };

        portConfigs = new Dictionary<Toggle, (List<ushort> registerAddress, List<ushort> closeRegisterAddress, bool state)>();
        registerUpdateTasks = new Dictionary<ushort, CancellationTokenSource>();
        isManuallyControlled = new Dictionary<Toggle, bool>();

        StartRegisterUpdate();
        StartSensorMonitoring();
        Onclick(true);
    }

    //private void TCP初始化()
    //{
    //    try
    //    {
    //        tcpClient = new TcpClient(ipAddress, port);
    //        master = ModbusIpMaster.CreateIp(tcpClient);
    //        Debug.Log("TCP连接成功");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"TCP连接失败: {ex.Message}");
    //    }
    //}
    private void 串口初始化()
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
            _控制面板.gameObject.SetActive(true);
            _控制面板.GetComponent<RectTransform>().localPosition = new Vector3(650, -343, 0);

            if (_控制面板.Find("ContentSizeFitter/GirdGroup").childCount == 0)
            {
                int i = 0;
                foreach (var config in ToggleConfigs)
                {
                    GameObject controlToggleClone = Instantiate(controltoggle, _控制面板.Find("ContentSizeFitter/GirdGroup"));
                    TextMeshProUGUI toggleText = controlToggleClone.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                    controlToggleClone.name = controltoggle.name + i.ToString();
                    i++;

                    if (toggleText != null)
                    {
                        toggleText.text = config.Value.displayName;
                    }
                }
            }

            cloneToggles = _控制面板.Find("ContentSizeFitter/GirdGroup").GetComponentsInChildren<Toggle>();
            foreach (var toggle in cloneToggles)
            {
                var config = ToggleConfigs.FirstOrDefault(c => c.Value.displayName == toggle.transform.Find("Name").GetComponent<TextMeshProUGUI>().text);
                if (config.Key != null)
                {
                    portConfigs[toggle] = (config.Value.registetrAddress, config.Value.closeRegisterAddress, false);
                    toggle.onValueChanged.AddListener((value) => {
                        MusicManager.Instance.音效系统播报(AudioEffect.MouseDown);
                        OnToggleValueChanged(toggle, value, true);
                        if (cameraMove.enabled==true)
                        {
                        cameraMove.OnToggleValueChanged(toggle, isOn);
                            
                        }
                        });
                }
            }

            Graphic[] graphics = _控制面板.GetComponentsInChildren<Graphic>();
            foreach (var graphic in graphics)
            {
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0f);
                tween = graphic.DOFade(0.8f, 0.5f).SetEase(Ease.OutQuad);
            }
        }
        else
        {
            _控制面板.gameObject.SetActive(false);
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


    //随机数写入

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
                Debug.Log($"写入寄存器 {registerAddress}: 值 {randomValue}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"写入寄存器 {registerAddress} 时出错: {ex.Message}");
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
    //读取寄存器
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
                        // 传感器触发时，如果按钮状态是手动控制的，强制更新状态
                        if (!isManuallyControlled.ContainsKey(toggle) || !isManuallyControlled[toggle])
                        {
                            UpdateToggleState(toggleKey, isActive);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"读取寄存器 {registerAddress} 时出错: {ex.Message}");
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
    //读取寄存器并触发按钮上的效果
    private void UpdateToggleState(string toggleKey, bool isActive)
    {
        var toggleConfig = ToggleConfigs.FirstOrDefault(c => c.Key == toggleKey);
        if (toggleConfig.Key != null)
        {
            var toggle = portConfigs.FirstOrDefault(p => p.Value.registerAddress == toggleConfig.Value.registetrAddress).Key;
            if (toggle != null)
            {
                // 如果是手动控制的按钮，我们不更新它的状态
                if (isManuallyControlled.ContainsKey(toggle) && isManuallyControlled[toggle] && toggle.isOn == isActive)
                {
                    return;
                }

                // 更新 toggle 状态
                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = isActive;
                toggle.onValueChanged.AddListener((value) =>
                {
                    MusicManager.Instance.音效系统播报(AudioEffect.MouseDown);
                    OnToggleValueChanged(toggle, value, true);
                });

                if (isActive)
                {
                    if (cameraMove.enabled == true)
                    {
                        cameraMove.OnToggleValueChanged(toggle, isActive);
                    }

                    // 检查并播放视频
                    if (!videoPlayedStatus.ContainsKey(toggleKey) || !videoPlayedStatus[toggleKey])
                    {
                        PlayVideo(toggleKey);
                        videoPlayedStatus[toggleKey] = true;
                    }

                    // 控制其他效果
                    if (toggleKey == "综采2")
                    {
                        uiFunction.上采煤(isActive);
                    }
                    if (toggleKey == "综采1")
                    {
                        uiFunction.下采煤(isActive);
                    }
                    if (toggleKey == "掘进工作面")
                    {
                        uiFunction.掘进按钮(isActive);
                    }
                    if (toggleKey == "制冷设备1")
                    {
                        uiFunction.管道透明(isActive);
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
    //停止随机写入
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
