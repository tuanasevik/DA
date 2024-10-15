﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PharusTransmission;
using System.IO;
using System;
using UnityEngine.UI;
using UnityTracking;
using TMPro;

namespace UnityPharus
{
    /// <summary>
    /// The UnityPharusManager keeps control over the UnityPharusListener and the UnityPharusEventProcessor.
    /// </summary>
    [AddComponentMenu("UnityPharus/UnityPharusManager")]
	public class UnityPharusManager : MonoBehaviour, ITrackingManager
    {
        /// <summary>
        /// Overall PharusTransmission Settings
        /// </summary>
        [System.Serializable]
        public class PharusSettings
        {
            public enum EProtocolType
            {
                TCP,
                UDP
            }

			
			
			[SerializeField] private bool _tracklinkEnabled = true;
            [SerializeField] private EProtocolType _protocol = EProtocolType.UDP;
            [SerializeField] private string _tcpRemoteIpAddress = "127.0.0.1";
            [SerializeField] private int _tcpLocalPort = 44345;
            [SerializeField] private string _udpMulticastIpAddress = "239.1.1.1";
			[SerializeField] private int _udpLocalPort = 44345;
			[Tooltip("in pixel")]
			[SerializeField] private int _targetScreenWidth = 1920;
			[Tooltip("in pixel")]
			[SerializeField] private int _targetScreenHeight = 1080;
			[Tooltip("in centimeter")]
			[SerializeField] private float _stageX = 1600f;
			[Tooltip("in centimeter")]
			[SerializeField] private float _stageY = 900f;
            [Tooltip("Use a negative value to prevent automatically server reconnecting")]
			[SerializeField] private float _checkServerReconnectIntervall = 5;

			
			public bool TracklinkEnabled
			{
				get { return this._tracklinkEnabled; }
				set { this._tracklinkEnabled = value; }
			}
            public EProtocolType Protocol
            {
                get { return this._protocol; }
                set { this._protocol = value; }
            }
            public string TCP_IP_Address
            {
                get { return this._tcpRemoteIpAddress; }
                set { this._tcpRemoteIpAddress = value; }
            }
            public int TCP_Port
            {
                get { return this._tcpLocalPort; }
                set { this._tcpLocalPort = value; }
            }
            public string UDP_Multicast_IP_Address
            {
                get { return this._udpMulticastIpAddress; }
                set { this._udpMulticastIpAddress = value; }
            }
            public int UDP_Port
            {
                get { return this._udpLocalPort; }
                set { this._udpLocalPort = value; }
            }
            public int TargetScreenWidth
            {
                get { return this._targetScreenWidth; }
                set { this._targetScreenWidth = value; }
            }
            public int TargetScreenHeight
            {
                get { return this._targetScreenHeight; }
                set { this._targetScreenHeight = value; }
			}
			public float StageX
			{
				get { return this._stageX; }
				set { this._stageX = value; }
			}
			public float StageY
			{
				get { return this._stageY; }
				set { this._stageY = value; }
			}
            public float CheckServerReconnectIntervall
            {
                get { return this._checkServerReconnectIntervall; }
            }
		}
		
		#region event handlers
		public event EventHandler<EventArgs> OnTrackingInitialized;
		#endregion


		#region exposed inspector fields
		[SerializeField] private bool m_persistent = true;
		[SerializeField] private PharusSettings m_pharusSettings = new PharusSettings();
		
		public GameObject canvasControl;
		public TextMeshProUGUI trackingType;
		public TextMeshProUGUI protocolStatus;
		public TextMeshProUGUI resolutionStatus;
		public TextMeshProUGUI lastPacketStatus;
		#endregion

		public float timeSinceLastPacket = 0f;

		private UnityPharusXMLConfig m_unityPharusXMLConfig;
		private bool m_initialized = false;
		private UnityPharusListener m_listener;
		private UnityPharusEventProcessor m_eventProcessor;

		public UnityPharusEventProcessor EventProcessor
		{
			get { return m_eventProcessor; }
		}
        
        #region Singleton pattern
        private static UnityPharusManager m_instance;
        public static UnityPharusManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = (UnityPharusManager)FindObjectOfType(typeof(UnityPharusManager));
                    if (m_instance == null)
                    {
//						Debug.LogWarning (string.Format ("No instance of {0} available.", typeof(UnityPharusManager)));
                    }
                    else
                    {
                        m_instance.Awake();
                    }
                }
                return m_instance;
            }
        }
        #endregion


		// logic


		public class Subject
		{
			public Vector3 position;
			public int id;
		}

		public Dictionary<int, Subject> Subjects = new Dictionary<int, Subject>();
		
		public bool Simulate = false;

		// end logic


        #region unity messages
        void Awake()
            
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
            else
            {
                if (m_instance != this)
                {
                    Debug.LogWarning(string.Format("Other instance of {0} detected (will be destroyed)", typeof(UnityPharusManager)));
                    GameObject.Destroy(this.gameObject);
                    return;
                }
            }

            if (!m_initialized)
            {
                StartCoroutine(InitInstance());
            }
            
        }

        void Update()
        {
			HandleKeyboardInputs ();

            //Lister for Pharus Data if Tracklink is enabled
            if (m_eventProcessor != null)
            {
                m_eventProcessor.Process();
            }

			timeSinceLastPacket += Time.deltaTime;
// lastPacketStatus.text = "last pkt: "  + Util.FmtFloat(UnityPharusManager.Instance.timeSinceLastPacket);

			if (Simulate)
			{
				SimulateUpdate();
			}
        }

        void OnDestroy()
        {
            if (m_listener != null)
            {
                m_listener.Shutdown();
            }
        }
        #endregion

        #region public methods
        public void ReconnectListener(float theDelay = -1f)
        {
            if (theDelay <= 0)
            {
                m_listener.Reconnect();
            }
            else
            {
                StartCoroutine(ReconnectListenerDelayed(theDelay));
            }
        }
        #endregion

        #region private methods
        private IEnumerator InitInstance()
        {
            m_initialized = true;

            Application.runInBackground = true;

            if (m_pharusSettings.CheckServerReconnectIntervall > 0)
            {
                StartCoroutine(CheckServerAlive(m_pharusSettings.CheckServerReconnectIntervall));
            }

            if (m_persistent)
            {
                GameObject.DontDestroyOnLoad(this.gameObject);
            }

            // start: load config file
            yield return StartCoroutine(LoadConfigXML());
//			Debug.Log ("UnityPharusManager config loaded, continue InitInstance");
            if (m_unityPharusXMLConfig != null)
			{
				string configTrackLinkEnabled = null;
                string configProtocol = null;
                string configTCPIP = null;
                string configTCPPort = null;
                string configUDPMulticastIP = null;
                string configUDPPort = null;
				string targetResolutionX = null;
				string targetResolutionY = null;
				string stageX = null;
				string stageY = null;
                for (int i = 0; i < m_unityPharusXMLConfig.ConfigNodes.Length; i++)
                {
					switch(m_unityPharusXMLConfig.ConfigNodes[i].Name)
					{
						case "enabled":
							configTrackLinkEnabled = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						case "protocol":
							configProtocol = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						case "tcp-ip":
							configTCPIP = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						case "tcp-port":
							configTCPPort = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						case "udp-multicast-ip":
							configUDPMulticastIP = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						case "udp-port":
							configUDPPort = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						case "targetResolutionX":
							targetResolutionX = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						case "targetResolutionY":
							targetResolutionY = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						case "stageX":
							stageX = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						case "stageY":
							stageY = m_unityPharusXMLConfig.ConfigNodes[i].Value;
							break;
						default:
							break;
					}
                   
                }
				
				bool configTracklinkBool;
				if (configTrackLinkEnabled != null && Boolean.TryParse(configTrackLinkEnabled, out configTracklinkBool))
                {
                    m_pharusSettings.TracklinkEnabled = configTracklinkBool;
					Debug.Log(string.Format("XML config: TrackLink enabled: {0}", m_pharusSettings.TracklinkEnabled));
				} else {
					Debug.Log(string.Format("XML config: invalid TrackLink enabled config. Using settings from prefab instead: TrackLink enabled: {0}", m_pharusSettings.TracklinkEnabled));
				}

				if(configProtocol != null)
				{
					configProtocol = configProtocol.ToUpper();
					switch(configProtocol)
					{
					case "UDP":
						int configUDPPortInt;
						if (configUDPMulticastIP != null &&
						    configUDPPort != null && int.TryParse(configUDPPort, out configUDPPortInt))
						{
							m_pharusSettings.Protocol = PharusSettings.EProtocolType.UDP;
							m_pharusSettings.UDP_Multicast_IP_Address = configUDPMulticastIP;
							m_pharusSettings.UDP_Port = configUDPPortInt;
							Debug.Log(string.Format("XML config: using UDP: {0}:{1}", configUDPMulticastIP, configUDPPort));
						} else {
							Debug.LogWarning("XML config: invalid UDP config data");
						}
						break;
					case "TCP":
						int configTCPPortInt;
						if (configTCPIP != null &&
						    configTCPPort != null && int.TryParse(configTCPPort, out configTCPPortInt))
						{
							m_pharusSettings.Protocol = PharusSettings.EProtocolType.TCP;
							m_pharusSettings.TCP_IP_Address = configTCPIP;
							m_pharusSettings.TCP_Port = configTCPPortInt;
							Debug.Log(string.Format("XML config: using TCP: {0}:{1}", configTCPIP, configTCPPort));
						} else { 
							Debug.LogWarning("XML config: invalid TCP config data");
						}
						break;
					default:
						Debug.LogWarning("1 XML config: invalid protocol specification");
						break;
					}
				} else {
					Debug.LogWarning("2 XML config: invalid protocol specification");
				}

				int configResolutionIntX;
				int configResolutionIntY;
				if (targetResolutionX != null && int.TryParse(targetResolutionX, out configResolutionIntX) &&
				    targetResolutionY != null && int.TryParse(targetResolutionY, out configResolutionIntY))
				{
					m_pharusSettings.TargetScreenWidth = configResolutionIntX;
					m_pharusSettings.TargetScreenHeight = configResolutionIntY;
					Debug.Log(string.Format("XML config: new target resolution: {0}x{1}", m_pharusSettings.TargetScreenWidth, m_pharusSettings.TargetScreenHeight));
				}
				else
				{
					Debug.LogWarning(string.Format("XML config: invalid resolution config, using resolution specified in PharusManager prefab instead: {0}x{1}", m_pharusSettings.TargetScreenWidth, m_pharusSettings.TargetScreenHeight));
				}

				float configStageFloatX;
				float configStageFloatY;
				if (stageX != null && float.TryParse(stageX, out configStageFloatX) &&
					stageY != null && float.TryParse(stageY, out configStageFloatY))
				{
					m_pharusSettings.StageX = configStageFloatX;
					m_pharusSettings.StageY = configStageFloatY;
					Debug.Log(string.Format("XML config: new stage size: {0}x{1}", m_pharusSettings.StageX, m_pharusSettings.StageY));
				}
				else
				{
					Debug.LogWarning(string.Format("XML config: invalid stage size config, using stage size specified in PharusManager prefab instead: {0}x{1}", m_pharusSettings.StageX, m_pharusSettings.StageY));
				}
            }
            else
            {
                Debug.Log(string.Format("no config xml file found in resources: using default pharus settings ({0})", m_pharusSettings.Protocol.ToString()));
            }
            // end: load config file
			
			
			if (!m_pharusSettings.TracklinkEnabled) 
			{
				Debug.Log("Disable and Destroy UnityPharusManager");
				this.enabled = false;
				Destroy(this);
				yield break;
			}

            if (m_pharusSettings.Protocol == PharusSettings.EProtocolType.TCP)
            {
                m_listener = UnityPharusListener.NewUnityPharusListenerTCP(m_pharusSettings.TCP_IP_Address, m_pharusSettings.TCP_Port);
            }
            else if (m_pharusSettings.Protocol == PharusSettings.EProtocolType.UDP)
            {
                m_listener = UnityPharusListener.NewUnityPharusListenerUDP(m_pharusSettings.UDP_Multicast_IP_Address, m_pharusSettings.UDP_Port);
                
            }
            else
            {
                Debug.LogError("Invalid pharus settings!");
                yield break;
            }
            m_eventProcessor = new UnityPharusEventProcessor(m_listener);
			//m_eventProcessor.TrackUpdated += TrackUpdEvtDbg;
			//m_eventProcessor.TrackRemoved += TrackRemEvtDbg;
            
            if (OnTrackingInitialized != null)
            {
                OnTrackingInitialized(this, new EventArgs());
            }

            Screen.SetResolution(m_pharusSettings.TargetScreenWidth, m_pharusSettings.TargetScreenHeight, false);

			TrackingAdapter.InjectTrackingManager (m_instance);

            UpdateDebugGUI();

			if (m_unityPharusXMLConfig == null)
			{
				m_unityPharusXMLConfig = new();
				m_unityPharusXMLConfig.ConfigNodes = new UnityPharusXMLConfig.ConfigNode[1];
				m_unityPharusXMLConfig.ConfigNodes[0] = new();
				m_unityPharusXMLConfig.ConfigNodes[0].Name = "enabled";
				m_unityPharusXMLConfig.ConfigNodes[0].Value = "1";


				m_unityPharusXMLConfig.Save(Application.dataPath + "/../trackLinkConfig.xml");
			}
        }

		private void SimulateUpdate()
		{
			if (UnityPharusManager.Instance.Subjects == null)
			{
				UnityPharusManager.Instance.Subjects = new Dictionary<int, Subject>();	
			}
			if (UnityPharusManager.Instance.Subjects.Count == 0)
			{
				UnityPharusManager.Instance.Subjects[0] = new Subject();
			}

			var s = new Subject();
			s.id = 0;
			s.position = new Vector3(1.0f, 0.0f, 1.0f);
			UnityPharusManager.Instance.Subjects[0] = s;
		}

		public static void TrackUpdEvtDbg(object sender, UnityPharusEventProcessor.PharusEventTrackArgs args)
		{
			//Debug.Log("track upd: " + args.trackRecord.trackID + " / " + args.trackRecord.currentPos);

			UnityPharusManager.Instance.timeSinceLastPacket = 0f;

			Subject s = new Subject();
			s.id = args.trackRecord.trackID;
			s.position = new Vector3(args.trackRecord.currentPos.x, 0, args.trackRecord.currentPos.y);

			if (UnityPharusManager.Instance.Subjects == null)
			{
				UnityPharusManager.Instance.Subjects = new Dictionary<int, Subject>();	
			}
			UnityPharusManager.Instance.Subjects[args.trackRecord.trackID] = s;

			UnityPharusManager.Instance.UpdateDebugGUI();
		}

		public static void TrackRemEvtDbg(object sender, UnityPharusEventProcessor.PharusEventTrackArgs args)
		{
			Debug.Log("track rem: " + args.trackRecord.trackID + " / " + args.trackRecord.currentPos);

			if (UnityPharusManager.Instance.Subjects == null)
			{
				UnityPharusManager.Instance.Subjects = new Dictionary<int, Subject>();	
			}

			UnityPharusManager.Instance.Subjects.Remove(args.trackRecord.trackID);

			var go = GameObject.Find("subx" + args.trackRecord.trackID);
			Destroy(go);
		}

		private void HandleKeyboardInputs()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				if(canvasControl != null)
				{
					canvasControl.SetActive(true);
					UpdateDebugGUI();
				}
			} 
			else if (Input.GetKeyUp(KeyCode.Tab)) 
			{
				if(canvasControl != null)
				{
					canvasControl.SetActive(false);
					UpdateDebugGUI();
				}
			}
		}

        private void UpdateDebugGUI()
		{
			if (trackingType != null) {
				trackingType.text = "Tracking System: TrackLink";
			}
			if (protocolStatus != null) {
				string ipAddress = m_pharusSettings.Protocol == PharusSettings.EProtocolType.UDP ? m_pharusSettings.UDP_Multicast_IP_Address : m_pharusSettings.TCP_IP_Address;
				string port = m_pharusSettings.Protocol == PharusSettings.EProtocolType.UDP ? m_pharusSettings.UDP_Port.ToString() : m_pharusSettings.TCP_Port.ToString();
				protocolStatus.text = string.Format ("Protocol: {0} {1} : {2}", m_pharusSettings.Protocol, ipAddress, port);
			}
			if (resolutionStatus != null) {
				resolutionStatus.text = string.Format ("Resolution: {0} x {1}", m_pharusSettings.TargetScreenWidth, m_pharusSettings.TargetScreenHeight);
			}

			


			//foreach (KeyValuePair<int, Subject> kv in Subjects)
			//{

			//	GameObject go = GameObject.Find("subx" + kv.Key);
			//	if (go == null)
			//	{
			//		go = Instantiate(SubjectPrefab);
			//		go.name = "subx" + kv.Key;
			//	}

			//	go.GetComponent<SubjectBhv>().Subject = kv.Value;

			//}
        }

        private IEnumerator ReconnectListenerDelayed(float theDelay)
        {
            m_listener.Shutdown();
            yield return new WaitForSeconds(theDelay);
            m_listener.Reconnect();
        }

        private IEnumerator CheckServerAlive(float theWaitBetweenCheck)
        {
            while (true)
            {
                yield return new WaitForSeconds(theWaitBetweenCheck);
                if (m_listener != null && !m_listener.IsCurrentlyConnecting && !m_listener.HasDataReceivedSinceLastCheck())
                {
                    Debug.LogWarning(string.Format("--- There might be a connection problem... (no data received in the past {0} seconds)---", theWaitBetweenCheck));
                    StartCoroutine(ReconnectListenerDelayed(1f));
                }
            }
        }

        private IEnumerator LoadConfigXML()
        {
//			Debug.Log ("Trying to load config file");
			string aPathToConfigXML = Path.Combine(Application.dataPath, "trackLinkConfig.xml");
            aPathToConfigXML = "file:///" + aPathToConfigXML;
            WWW aWww = new WWW(aPathToConfigXML);
//			Debug.Log ("start loading file...");
            yield return aWww;
//			Debug.Log ("file loading complete");

            if (aWww.isDone && string.IsNullOrEmpty(aWww.error))
            {
//				Debug.Log ("no errors occured during config file load");
                m_unityPharusXMLConfig = UnityPharusXMLConfig.LoadFromText(aWww.text);
            }
        }
		#endregion

		#region interface properties
		public int TargetScreenWidth
		{
			get { return Instance.m_pharusSettings.TargetScreenWidth; }
		}
		public int TargetScreenHeight
		{
			get { return Instance.m_pharusSettings.TargetScreenHeight; }
		}

		public float TrackingStageX
		{
			get { return Instance.m_pharusSettings.StageX; }
		}
		public float TrackingStageY
		{
			get { return Instance.m_pharusSettings.StageY; }
		}
		#endregion

        #region interface methods
		public Vector2 GetScreenPositionFromRelativePosition(float x, float y)
        {
            return new Vector2((int)Mathf.Round(x * m_pharusSettings.TargetScreenWidth), m_pharusSettings.TargetScreenHeight - (int)Mathf.Round(y * m_pharusSettings.TargetScreenHeight));
        }
		#endregion

		#region static methods
		public static Vector2 GetScreenPositionFromRelativePosition(Vector2f pharusTrackPosition)
		{
			return Instance.GetScreenPositionFromRelativePosition(pharusTrackPosition.x, pharusTrackPosition.y);
		}
		#endregion
    }
}
