using System;
using System.Net.Sockets;
using DigitalSignatureLab.Networking;
using DigitalSignatureLab.Signature;
using UnityEngine;

namespace DigitalSignatureLab.Presentation
{
    public class ReceiverPanel
    {
        private readonly TcpPacketListener packetListener;
        private readonly int listenPort;

        private string statusText = string.Empty;
        private string providerText = string.Empty;
        private string messageText = string.Empty;
        private string computedHashText = string.Empty;
        private string signatureText = string.Empty;
        private string publicKeyText = string.Empty;
        private string verificationText = "Esperando mensajes...";
        private Color verificationColor = Color.white;
        private Vector2 scrollPosition;

        public ReceiverPanel(TcpPacketListener packetListener, int listenPort)
        {
            this.packetListener = packetListener;
            this.listenPort = listenPort;
        }

        public void StartListening()
        {
            try
            {
                packetListener.Start(listenPort);
                statusText = "Escuchando por TCP en el puerto " + listenPort;
            }
            catch (Exception exception) when (exception is SocketException || exception is ArgumentOutOfRangeException)
            {
                statusText = "No se pudo abrir el puerto: " + exception.Message;
            }
        }

        public void StopListening()
        {
            packetListener.Stop();
        }

        public void ProcessIncomingPackets()
        {
            while (packetListener.TryDequeuePayload(out string payload))
            {
                VerifyPayload(payload);
            }

            while (packetListener.TryDequeueError(out string error))
            {
                statusText = "Error de recepción: " + error;
            }
        }

        public void Draw(Rect area)
        {
            scrollPosition = GuiPanelDrawer.BeginPanel(area, "Receptor", scrollPosition);
            GUILayout.Label(statusText);
            DrawVerificationResult();
            GuiPanelDrawer.DrawReadOnlyField("Algoritmos usados", providerText);
            GuiPanelDrawer.DrawReadOnlyField("Mensaje recibido", messageText);
            GuiPanelDrawer.DrawReadOnlyField("Hash recalculado (hex)", computedHashText);
            GuiPanelDrawer.DrawReadOnlyField("Firma recibida (Base64)", signatureText);
            GuiPanelDrawer.DrawReadOnlyField("Llave pública recibida", publicKeyText);
            GuiPanelDrawer.EndPanel();
        }

        private void DrawVerificationResult()
        {
            Color previousColor = GUI.contentColor;
            GUI.contentColor = verificationColor;
            GUILayout.Label("<b>" + verificationText + "</b>");
            GUI.contentColor = previousColor;
        }

        private void VerifyPayload(string payload)
        {
            try
            {
                SignedMessage signedMessage = ParseSignedMessage(payload);
                ShowReceivedFields(signedMessage);
                DigitalSignatureService verificationService = SignatureServiceFactory.Create(ParseProvider(signedMessage.provider));
                ShowVerificationResult(verificationService.Verify(signedMessage));
            }
            catch (Exception exception) when (exception is FormatException || exception is ArgumentException)
            {
                ShowVerificationFailure("Paquete inválido: " + exception.Message);
            }
        }

        private static SignedMessage ParseSignedMessage(string payload)
        {
            SignedMessage signedMessage = JsonUtility.FromJson<SignedMessage>(payload);
            if (signedMessage == null || !signedMessage.HasRequiredFields())
            {
                throw new FormatException("The packet is missing required fields.");
            }

            return signedMessage;
        }

        private static CryptoProvider ParseProvider(string providerName)
        {
            if (!Enum.TryParse(providerName, out CryptoProvider provider) || !Enum.IsDefined(typeof(CryptoProvider), provider))
            {
                throw new FormatException("The packet uses an unknown provider.");
            }

            return provider;
        }

        private void ShowReceivedFields(SignedMessage signedMessage)
        {
            providerText = signedMessage.provider;
            messageText = signedMessage.message;
            signatureText = signedMessage.signatureBase64;
            publicKeyText = signedMessage.publicKey;
            computedHashText = string.Empty;
        }

        private void ShowVerificationResult(VerificationResult result)
        {
            computedHashText = result.ComputedHashHex;
            if (result.IsValid)
            {
                verificationText = "FIRMA VÁLIDA: el mensaje es auténtico e íntegro";
                verificationColor = Color.green;
                return;
            }

            ShowVerificationFailure("FIRMA INVÁLIDA: el mensaje fue alterado o la llave no corresponde");
        }

        private void ShowVerificationFailure(string reason)
        {
            verificationText = reason;
            verificationColor = Color.red;
        }
    }
}
