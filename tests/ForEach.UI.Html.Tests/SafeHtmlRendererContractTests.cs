using System.Reflection;

namespace ForEach.UI.Html.Tests;

/// <summary>The shape consumers rely on: a public sealed type with a public parameterless constructor.</summary>
public sealed class SafeHtmlRendererContractTests
{
    [Fact]
    public void ImplementsTheAbstraction()
    {
        new SafeHtmlRenderer().Should().BeAssignableTo<IHtmlRenderer>();
    }

    [Fact]
    public void IsPublicAndSealed()
    {
        typeof(SafeHtmlRenderer).IsPublic.Should().BeTrue();
        typeof(SafeHtmlRenderer).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void CanBeCreatedThroughReflectionWithoutArguments()
    {
        var instance = Activator.CreateInstance(typeof(SafeHtmlRenderer));

        instance.Should().BeOfType<SafeHtmlRenderer>();
    }

    [Fact]
    public void HasAPublicParameterlessConstructor()
    {
        typeof(SafeHtmlRenderer).GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes)
            .Should().NotBeNull();
    }

    [Fact]
    public void AbstractionsAssemblyHasNoDependenciesBeyondTheFramework()
    {
        var references = typeof(IHtmlRenderer).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => !n.StartsWith("System", StringComparison.Ordinal) && n != "netstandard" && n != "mscorlib");

        references.Should().BeEmpty();
    }

    [Fact]
    public void AbstractionsDeclareThreeMembers()
    {
        typeof(IHtmlRenderer).GetMethods().Select(m => m.Name)
            .Should().BeEquivalentTo("SanitizeUntrustedHtml", "RenderHelpMarkdown", "RenderChatMarkdown");
    }

    [Fact]
    public void RenderersAreIndependentInstances()
    {
        var a = new SafeHtmlRenderer();
        var b = new SafeHtmlRenderer();

        a.RenderChatMarkdown("**x**").Should().Be(b.RenderChatMarkdown("**x**"));
    }
}
