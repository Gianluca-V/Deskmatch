using DeskMatch.CoreService.Domain.Workspaces;
using FluentAssertions;
using Xunit;

namespace DeskMatch.UnitTests.Domain;

public class AttributeKeyNormalizerTests
{
    [Theory]
    [InlineData("PetFriendly", "petfriendly")]
    [InlineData("PET-FRIENDLY", "pet-friendly")]
    [InlineData("Wifi Available", "wifi-available")]
    [InlineData("24/7 Access", "24-7-access")]
    [InlineData("Mascotas Permitidas", "mascotas-permitidas")]
    [InlineData("mascotas-permitidas", "mascotas-permitidas")]
    [InlineData("Cafetería", "cafeteria")]
    [InlineData("Año de construcción", "ano-de-construccion")]
    [InlineData("Niños", "ninos")]
    [InlineData("crème brûlée", "creme-brulee")]
    [InlineData("---already---clean---", "already-clean")]
    [InlineData("  spaces  everywhere  ", "spaces-everywhere")]
    [InlineData("camelCase123Key", "camelcase123key")]
    [InlineData("snake_case_key", "snake-case-key")]
    public void Normalize_ShouldTransformCorrectly(string input, string expected)
    {
        var result = AttributeKeyNormalizer.Normalize(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("PET FRIENDLY", "pet-friendly")]
    [InlineData("pet friendly", "pet-friendly")]
    [InlineData("pet-friendly", "pet-friendly")]
    [InlineData("Pet Friendly", "pet-friendly")]
    [InlineData("mascotas-permitidas", "mascotas-permitidas")]
    [InlineData("MASCOTAS PERMITIDAS", "mascotas-permitidas")]
    public void Normalize_DifferentRepresentations_ShouldProduceSameKey(
        string a,
        string b)
    {
        var resultA = AttributeKeyNormalizer.Normalize(a);
        var resultB = AttributeKeyNormalizer.Normalize(b);

        resultA.Should().Be(resultB);
    }
}
