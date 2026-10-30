namespace Whyland.Domain.Common.Enums;

/// <summary>
/// Supported payment methods (s9.8). Real gateway integration is not required in version 1;
/// real providers are represented only through this abstraction.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethod
{
    /// <summary>
    /// Direct bank gateway.
    /// </summary>
    BankGateway = 0,

    /// <summary>
    /// ZarinPal payment provider (abstracted; no real integration in version 1).
    /// </summary>
    ZarinPal = 1,

    /// <summary>
    /// SnappPay payment provider (abstracted; no real integration in version 1).
    /// </summary>
    SnappPay = 2
}
