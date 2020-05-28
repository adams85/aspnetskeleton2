using System;

namespace WebApp.Service
{
    public interface IKeyGeneratorCommand : ICommand
    {
        Action<ICommand, object>? OnKeyGenerated { get; set; }
    }
}
