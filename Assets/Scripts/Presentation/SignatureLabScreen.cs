using DigitalSignatureLab.Networking;
using UnityEngine;

namespace DigitalSignatureLab.Presentation
{
    public class SignatureLabScreen : MonoBehaviour
    {
        [SerializeField] private string receiverHost = "127.0.0.1";
        [SerializeField] private int networkPort = 5050;
        [SerializeField] private int fontSize = 16;
        [SerializeField] private float panelMargin = 10f;

        private SenderPanel senderPanel;
        private ReceiverPanel receiverPanel;

        private void Awake()
        {
            senderPanel = new SenderPanel(new TcpPacketSender(), receiverHost, networkPort);
            receiverPanel = new ReceiverPanel(new TcpPacketListener(), networkPort);
        }

        private void Start()
        {
            receiverPanel.StartListening();
        }

        private void Update()
        {
            receiverPanel.ProcessIncomingPackets();
        }

        private void OnGUI()
        {
            ApplyGuiStyles();
            float halfScreenWidth = Screen.width / 2f;
            float panelWidth = halfScreenWidth - panelMargin * 2f;
            float panelHeight = Screen.height - panelMargin * 2f;
            senderPanel.Draw(new Rect(panelMargin, panelMargin, panelWidth, panelHeight));
            receiverPanel.Draw(new Rect(halfScreenWidth + panelMargin, panelMargin, panelWidth, panelHeight));
        }

        private void OnDestroy()
        {
            receiverPanel.StopListening();
        }

        private void ApplyGuiStyles()
        {
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.label.richText = true;
            GUI.skin.label.wordWrap = true;
            GUI.skin.button.fontSize = fontSize;
            GUI.skin.toggle.fontSize = fontSize;
            GUI.skin.textArea.fontSize = fontSize;
            GUI.skin.textArea.wordWrap = true;
        }
    }
}
