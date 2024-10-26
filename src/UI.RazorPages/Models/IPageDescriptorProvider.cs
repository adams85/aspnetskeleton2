namespace WebApp.UI.Models;

public interface IPageDescriptorProvider
{
    static abstract PageDescriptor PageDescriptorStatic { get; }

    PageDescriptor PageDescriptor { get; }
}
