using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using DigitalSignatureLab.Networking;
using DigitalSignatureLab.Signature;
using UnityEngine;

namespace DigitalSignatureLab.Presentation
{
    public class SenderPanel
    {
        private const string TamperSuffix = " [alterado]";
        private const float MessageAreaHeight = 80f;

        private static readonly string[] ProviderLabels = { "Librerías: SHA-256 + RSA", "Propios: RoundMix + Modular encadenada" };

        private readonly TcpPacketSender packetSender;
        private readonly string receiverHost;
        private readonly int receiverPort;

        private DigitalSignatureService signatureService;
        private CryptoProvider selectedProvider = CryptoProvider.Library;
        private string messageText = string.Empty;
        private bool tamperInTransit;
        private bool isSending;
        private string hashText = string.Empty;
        private string signatureText = string.Empty;
        private string publicKeyText = string.Empty;
        private string statusText = string.Empty;
        private Vector2 scrollPosition;

        public SenderPanel(TcpPacketSender packetSender, string receiverHost, int receiverPort)
        {
            this.packetSender = packetSender;
            this.receiverHost = receiverHost;
            this.receiverPort = receiverPort;
        }

        public void Draw(Rect area)
        {
            scrollPosition = GuiPanelDrawer.BeginPanel(area, "Emisor", scrollPosition);
            DrawProviderSelector();
            DrawMessageInput();
            DrawSendControls();
            DrawSignatureDetails();
            GuiPanelDrawer.EndPanel();
        }

        private void DrawProviderSelector()
        {
            GUILayout.Label("Algoritmos");
            selectedProvider = (CryptoProvider)GUILayout.Toolbar((int)selectedProvider, ProviderLabels);
        }

        private void DrawMessageInput()
        {
            GUILayout.Label("Mensaje");
            messageText = GUILayout.TextArea(messageText, GUILayout.MinHeight(MessageAreaHeight));
        }

        private void DrawSendControls()
        {
            tamperInTransit = GUILayout.Toggle(tamperInTransit, "Alterar el mensaje en tránsito");
            GUI.enabled = !isSending;
            if (GUILayout.Button("Firmar y enviar"))
            {
                SendSignedMessage();
            }

            GUI.enabled = true;
            GUILayout.Label(statusText);
        }

        private void DrawSignatureDetails()
        {
            GuiPanelDrawer.DrawReadOnlyField("Hash del mensaje (hex)", hashText);
            GuiPanelDrawer.DrawReadOnlyField("Firma digital (Base64)", signatureText);
            GuiPanelDrawer.DrawReadOnlyField("Llave pública enviada", publicKeyText);
        }

        private async void SendSignedMessage()
        {
            string validationError = ValidateInput();
            if (validationError != null)
            {
                statusText = validationError;
                return;
            }

            isSending = true;
            statusText = "Firmando y enviando...";
            try
            {
                SignedMessage signedMessage = await SignMessageAsync(messageText);
                if (tamperInTransit)
                {
                    signedMessage.message += TamperSuffix;
                }

                await packetSender.SendAsync(receiverHost, receiverPort, JsonUtility.ToJson(signedMessage));
                statusText = "Mensaje enviado a " + receiverHost + ":" + receiverPort;
            }
            catch (Exception exception) when (exception is SocketException || exception is IOException || exception is ArgumentOutOfRangeException)
            {
                statusText = "No se pudo enviar: " + exception.Message;
            }
            finally
            {
                isSending = false;
            }
        }

        private string ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return "Escriba un mensaje antes de enviar.";
            }

            if (string.IsNullOrWhiteSpace(receiverHost))
            {
                return "Configure el host del receptor en el Inspector.";
            }

            return null;
        }

        private async Task<SignedMessage> SignMessageAsync(string message)
        {
            DigitalSignatureService service = GetSignatureService();
            SignedMessage signedMessage = await Task.Run(() => service.Sign(message));
            hashText = service.ComputeHashHex(message);
            signatureText = signedMessage.signatureBase64;
            publicKeyText = signedMessage.publicKey;
            return signedMessage;
        }

        private DigitalSignatureService GetSignatureService()
        {
            if (signatureService == null || signatureService.Provider != selectedProvider)
            {
                signatureService = SignatureServiceFactory.Create(selectedProvider);
            }

            return signatureService;
        }
    }
}
