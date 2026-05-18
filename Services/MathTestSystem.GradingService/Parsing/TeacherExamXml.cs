using System.Xml.Serialization;

namespace MathTestSystem.GradingService.Parsing;

/// <summary>
/// XSD-generated strongly-typed classes for deserializing a teacher exam submission XML.
/// These are the raw deserialization targets — mapping to domain objects happens in ExamXmlParser.
/// </summary>

[XmlRoot("Teacher")]
public sealed class TeacherXml
{
    [XmlAttribute("ID")]
    public string ID { get; set; } = string.Empty;

    // [XmlArray] provides the <Students> wrapper; [XmlArrayItem] names each child element.
    [XmlArray("Students")]
    [XmlArrayItem("Student")]
    public List<StudentXml> Students { get; set; } = [];
}

public sealed class StudentXml
{
    [XmlAttribute("ID")]
    public string ID { get; set; } = string.Empty;

    // No wrapper element — Exam elements are direct children of Student.
    [XmlElement("Exam")]
    public List<ExamXml> Exams { get; set; } = [];
}

public sealed class ExamXml
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    // No wrapper element — Task elements are direct children of Exam.
    [XmlElement("Task")]
    public List<TaskXml> Tasks { get; set; } = [];
}

public sealed class TaskXml
{
    [XmlAttribute("id")]
    public string Id { get; set; } = string.Empty;

    // Captures the text content of the Task element, e.g. " 2+3/6-4 = 74 ".
    [XmlText]
    public string Content { get; set; } = string.Empty;
}
