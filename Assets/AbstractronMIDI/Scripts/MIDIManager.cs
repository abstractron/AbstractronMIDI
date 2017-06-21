using UnityEngine;
using System.Threading;
using Sanford.Multimedia.Midi;
using System;
using Sanford.Multimedia;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MIDIManager : MonoBehaviour {

    public KeyCode toggleUIKey = KeyCode.BackQuote;

    List<InputDevice> inputs;
    List<OutputDevice> outputs;

    bool isDirty = true;

    public const int SysExBufferSize = 128;
    public InputDevice inDevice = null;
    public OutputDevice outDevice = null;
    public SynchronizationContext context;

    public Canvas MenuCanvas;
    public TextMeshProUGUI midiDebugText;
    public string debugStringText;

    public TextMeshProUGUI midiTextChannel, midiTextCommand, midiTextData1, midiTextData2, midiSummary;
    public string midiChannel, midiCommand;
    public string midiData1, midiData2;

    public List<GameObject> menuPanels;

    #region MonoBehaviour

    // Use this for initialization
    void Start () {
        InitializeMIDI();

        if (InputDevice.DeviceCount == 0)
        {
            LogDebug("AbstractMIDI ERROR: No MIDI input devices available.");
            CloseMIDI();
        }
        else
        {
            try
            {
                context = SynchronizationContext.Current;

                LogDebug("  Attempting to use first device...");
                inDevice = new InputDevice(0);
                inDevice.ChannelMessageReceived += HandleChannelMessageReceived;
                inDevice.SysCommonMessageReceived += HandleSysCommonMessageReceived;
                inDevice.SysExMessageReceived += HandleSysExMessageReceived;
                inDevice.SysRealtimeMessageReceived += HandleSysRealtimeMessageReceived;
                inDevice.Error += new EventHandler<ErrorEventArgs>(inDevice_Error);

                RegisterInputDevice(inDevice);
                RegisterOutputDevices();
            }
            catch (Exception ex)
            {
                LogDebug(ex.Message);
                CloseMIDI();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(toggleUIKey))
        {
            ToggleUI();
        }

        if (isDirty)
        {
            UpdateDisplayText();

            isDirty = false;
        }
        
        OutputToDevice();
    }

    private void OutputToDevice()
    {
        ChannelMessageBuilder builder = new ChannelMessageBuilder();

        builder.Command = ChannelCommand.NoteOn;
        builder.MidiChannel = 1;
        builder.Data1 = 93;
        builder.Data2 = 53;
        builder.Build();

        ReportAndSendMidiOutput(builder.Result);
    }

    private void ReportAndSendMidiOutput(ChannelMessage message)
    {
        LogDebug(string.Format("CH {0} TYPE {1} STATUS {2} CMD {3} DATA1 {4} DATA2 {5}", message.MidiChannel, message.MessageType, message.Status, message.Command, message.Data1, message.Data2));
        outputs[1].Send(message);
    }

    private void UpdateDisplayText()
    {
        midiDebugText.text = debugStringText;

        //midiSummary.text = string.Format("CH {0}\nCMD {1}\nDATA1 {2}\nDATA2 {3}", midiChannel, midiCommand, midiData1, midiData2);

        midiTextChannel.text = midiChannel;
        midiTextCommand.text = midiCommand;
        midiTextData1.text = midiData1;
        midiTextData2.text = midiData2;
    }

    void OnDestroy()
    {
        CloseMIDI();
    }

    #endregion

    #region User Interface

    void ToggleUI()
    {
        MenuCanvas.gameObject.SetActive(!MenuCanvas.gameObject.activeSelf);
    }

    public void ShowMenuContentByName(string panelName)
    {
        LogDebug("Open menu " + panelName + "Content");
        var result = from p in menuPanels
                     where p.name == panelName
                     select p;

        foreach (var p in result)
        {
            LogDebug("Showing panel: " + p.name + "Content");
            GameObject.Find(p.name + "Content").SetActive(true);
        }
    }

    #endregion

    #region Event Handling

    private void inDevice_Error(object sender, ErrorEventArgs e)
    {
        LogDebug("AbstractMIDI ERROR: Device: " + e.Error.Message);
    }

    private void HandleChannelMessageReceived(object sender, ChannelMessageEventArgs e)
    {
        LogDebug("AbstractMIDI: Channel message received.");
        LogMIDICommand(e.Message);
    }

    private void HandleSysExMessageReceived(object sender, SysExMessageEventArgs e)
    {
        context.Post(delegate (object dummy)
        {
            string result = "\n\n";

            foreach (byte b in e.Message)
            {
                result += string.Format("{0:X2} ", b);
            }

            LogDebug(result);
        }, null);
    }

    private void HandleSysCommonMessageReceived(object sender, SysCommonMessageEventArgs e)
    {
        context.Post(delegate (object dummy)
        {
            LogDebug(e.Message.SysCommonType.ToString() + '\t' + '\t' +
                e.Message.Data1.ToString() + '\t' +
                e.Message.Data2.ToString());
        }, null);
    }

    private void HandleSysRealtimeMessageReceived(object sender, SysRealtimeMessageEventArgs e)
    {
        context.Post(delegate (object dummy)
        {
            LogDebug(e.Message.SysRealtimeType.ToString());
        }, null);
    }

    private void LogMIDICommand(ChannelMessage message)
    {
        LogDebug(string.Format("CH {0} TYPE {1} STATUS {2} CMD {3} DATA1 {4} DATA2 {5}", message.MidiChannel, message.MessageType, message.Status, message.Command, message.Data1, message.Data2));

        midiChannel = message.MidiChannel.ToString();
        midiCommand = message.Command.ToString();
        
        midiData1 = message.Data1.ToString();
        midiData2 = message.Data2.ToString();
        
        if (!message.Command.ToString().ToLower().Contains("pressure"))
            isDirty = true;
    }

    #endregion

    #region Device Management

    private void RegisterInputDevice(InputDevice inDeviceRef)
    {
        if (inDeviceRef == null)
        {
            LogDebug("AbstractMIDI: Error: Attempted to register device that is not ready: " + inDeviceRef.ToString());
        }
        else
        {
            var capabilities = InputDevice.GetDeviceCapabilities(inDeviceRef.DeviceID);
            LogDebug("AbstractMIDI: Input device found: " + capabilities.name);
            inputs.Add(inDeviceRef);
        }

        LogDebug("AbstractMIDI: Input device found: " + inDeviceRef.DeviceID);
        inputs.Add(inDeviceRef);

        StartMIDI();
    }

    private void RegisterOutputDevices()
    {
        for (int o = 0; o < OutputDevice.DeviceCount; o++)
        {
            outDevice = new OutputDevice(o);
            RegisterOutputDevice(outDevice);
        }
    }

    private void RegisterOutputDevice(OutputDevice outDeviceRef)
    {
        if (outDeviceRef == null)
        {
            LogDebug("AbstractMIDI: Error: Attempted to register device that is not ready: " + outDeviceRef.ToString());
        }
        else
        {
            var capabilities = OutputDevice.GetDeviceCapabilities(outDeviceRef.DeviceID);
            LogDebug("AbstractMIDI: Output device found: " + capabilities.name + ": " + capabilities.technology);
        }

        outputs.Add(outDeviceRef);
    }

    #endregion

    #region Error handling

    private void LogDebug(string message)
    {
        Debug.Log(message);
        debugStringText = message + "\n";
    }

    private void ClearLogText()
    {
        debugStringText = "";
    }

    #endregion

    #region MIDI lifecycle

    void InitializeMIDI()
    {
        ClearLogText();
        LogDebug("AbstractMIDI: Start()");

        menuPanels = new List<GameObject>();
        menuPanels.AddRange(GameObject.FindGameObjectsWithTag("menucontent"));

        inputs = new List<InputDevice>();
        outputs = new List<OutputDevice>();

        LogDebug("AbstractMIDI: Input count: " + InputDevice.DeviceCount);
        LogDebug("AbstractMIDI: Output count: " + OutputDevice.DeviceCount);
    }

    public void StartMIDI()
    {
        try
        {
            isDirty = true;
            inDevice.StartRecording();
        }
        catch (Exception ex)
        {
            LogDebug("AbstractMIDI ERROR: " + ex.Message);
        }
        finally
        {
            LogDebug("AbstractMIDI: Started MIDI OK.");
        }
    }

    public void StopMIDI()
    {
        LogDebug("AbstractMIDI: Stopping MIDI...");

        try
        {
            isDirty = false;
            inDevice.StopRecording();
            inDevice.Reset();
        }
        catch (Exception ex)
        {
            LogDebug("AbstractMIDI ERROR: " + ex.Message);
        }
        finally
        {
            LogDebug("AbstractMIDI: Stopped MIDI OK.");
        }
    }

    void CloseMIDI()
    {
        if (inDevice != null)
        {
            inDevice.Close();
        }
    }

    #endregion
}
