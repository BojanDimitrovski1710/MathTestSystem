using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MathTestSystem.Domain.Constants;

namespace MathTestSystem.GradingService.Parsing;

/// <summary>
/// Generic XML processor that validates against an XSD schema and deserializes
/// into a strongly-typed object in a single streaming pass.
/// </summary>
public static class XmlProcessor
{
    /// <summary>
    /// Validates <paramref name="xml"/> against <paramref name="schemaSet"/> and deserializes
    /// it into <typeparamref name="T"/>. Throws <see cref="InvalidOperationException"/> with a
    /// <see cref="ResultCodes.XmlSchemaValidationFailed"/> prefix if the XML violates the schema.
    /// </summary>
    public static T DeserializeWithValidation<T>(string xml, XmlSchemaSet schemaSet) where T : class
    {
        List<string> validationErrors = [];

        XmlReaderSettings settings = new()
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemaSet
        };

        // Collect all schema violations instead of throwing on the first one,
        // so the error message can include the full list of problems.
        settings.ValidationEventHandler += (_, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
                validationErrors.Add(e.Message);
        };

        using StringReader stringReader = new(xml);
        using XmlReader reader = XmlReader.Create(stringReader, settings);

        XmlSerializer serializer = new(typeof(T));

        // Validation and deserialization happen simultaneously in a single streaming pass.
        T? result = serializer.Deserialize(reader) as T;

        if (validationErrors.Count > 0)
            throw new InvalidOperationException(
                $"{ResultCodes.XmlSchemaValidationFailed}: {string.Join("; ", validationErrors)}");

        return result ?? throw new InvalidOperationException(ResultCodes.XmlSchemaValidationFailed);
    }
}
