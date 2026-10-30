using System.Security.Cryptography;

namespace Whyland.Infrastructure.Services;

public class CodeGeneratorService : ICodeGeneratorService
{
    public string Generate()
    {
        return RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
    }
}