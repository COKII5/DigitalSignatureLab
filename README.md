# DigitalSignatureLab

Sistema de firma digital en Unity 6 (6000.0.82f1) para el laboratorio de Servicios Multimedia de la Universidad de San Buenaventura, Bogotá.

El emisor escribe un mensaje, calcula su hash, lo firma con su llave privada y lo envía por TCP junto con la llave pública. El receptor recalcula el hash, verifica la firma con la llave pública y muestra si el mensaje es auténtico e íntegro.

## Proveedores criptográficos

| Proveedor | Hash | Firma |
|---|---|---|
| Librerías | `SHA256` de .NET | `RSA` de .NET (2048 bits, PKCS#1 v1.5) |
| Propios | `RoundMixHash` (256 bits, 12 rondas de sustitución, difusión y permutación) | Firma modular encadenada (módulo de 512 bits, cuatro exponentes, encadenamiento por bloques) |

Ambos proveedores se conectan al flujo mediante el patrón Adaptador (`IHashAlgorithm`, `ISignatureAlgorithm`).

## Estructura

```
Assets/Scripts/
  Cryptography/Abstractions   Interfaces objetivo del patrón Adaptador
  Cryptography/Library        Adaptadores de SHA256 y RSA de .NET
  Cryptography/Custom         Algoritmos propios y sus adaptadores
  Signature                   Servicio de firma, fábrica y modelos
  Networking                  Envío y recepción por TCP
  Presentation                Interfaz de emisor y receptor (IMGUI)
```

## Ejecución

1. Abrir el proyecto con Unity 6000.0.82f1.
2. Abrir la escena `Assets/Scenes/SignatureLab.unity`.
3. Presionar Play. El objeto `SignatureLab` contiene el componente `SignatureLabScreen`; el host y el puerto se configuran desde el Inspector.
4. Elegir el proveedor, escribir un mensaje y presionar "Firmar y enviar". La opción "Alterar el mensaje en tránsito" permite comprobar que la verificación falla.
