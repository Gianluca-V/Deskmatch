using DeskMatch.CoreService.Domain.Workspaces;
using FluentAssertions;
using Xunit;

namespace DeskMatch.UnitTests.Domain;

public class WorkspaceAttributeTests
{
    [Fact]
    public void Create_ShouldNormalizeKey()
    {
        var attr = WorkspaceAttribute.Create("Pet Friendly", "true");

        attr.Key.Should().Be("pet-friendly");
        attr.Value.Should().Be("true");
    }

    [Fact]
    public void Create_WithNullValue_ShouldAllowNull()
    {
        var attr = WorkspaceAttribute.Create("some-key", null);

        attr.Key.Should().Be("some-key");
        attr.Value.Should().BeNull();
    }

    [Fact]
    public void Equals_SameKeySameValue_ShouldBeEqual()
    {
        var attr1 = WorkspaceAttribute.Create("pet-friendly", "yes");
        var attr2 = WorkspaceAttribute.Create("pet-friendly", "yes");

        attr1.Should().Be(attr2);
        (attr1 == attr2).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameKeyDifferentValue_ShouldNotBeEqual()
    {
        var attr1 = WorkspaceAttribute.Create("key", "value1");
        var attr2 = WorkspaceAttribute.Create("key", "value2");

        attr1.Should().NotBe(attr2);
    }

    [Fact]
    public void Equals_DifferentKeySameValue_ShouldNotBeEqual()
    {
        var attr1 = WorkspaceAttribute.Create("key1", "value");
        var attr2 = WorkspaceAttribute.Create("key2", "value");

        attr1.Should().NotBe(attr2);
    }

    [Fact]
    public void GetHashCode_EqualAttributes_ShouldHaveSameHashCode()
    {
        var attr1 = WorkspaceAttribute.Create("pet-friendly", "yes");
        var attr2 = WorkspaceAttribute.Create("pet-friendly", "yes");

        attr1.GetHashCode().Should().Be(attr2.GetHashCode());
    }
}
