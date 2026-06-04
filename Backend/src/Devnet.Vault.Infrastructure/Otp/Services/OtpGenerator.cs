using Devnet.Vault.Application.Features.Shared.Otp.Interfaces.Services;
using System.Security.Cryptography;
using static Devnet.Vault.Domain.Constants.Messages.ValidationMessages;

namespace Devnet.Vault.Infrastructure.Otp.Services;

internal sealed class OtpGenerator : IOtpGenerator
{
    public string Generate(int length = 6)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), OtpValidationMessages.OTP_LENGTH_ZERO);
        }

        var digits = new char[length];
        var buffer = new byte[length];

        using var random = RandomNumberGenerator.Create();
        random.GetBytes(buffer);

        for (var i = 0; i < length; i++)
        {
            digits[i] = (char)('0' + (buffer[i] % 10));
        }

        return new string(digits);
    }
}
