namespace CodegenTools.Templates;

public abstract class CommandTemplateBase : TemplateBase
{
    public string Namespace { get; set; } = null!;
    public string Group { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsKeyGenerator { get; set; }
    public bool IsEventProducer { get; set; }
}
