using System.Xml.Serialization;

[XmlRoot(ElementName = "ClearAppointmentRequest")]
public class ClearAppointmentRequest
{
    [XmlElement(ElementName = "jobEmp_SeqNo")]
    public int JobEmpSeqNo { get; set; }

    [XmlElement(ElementName = "adUserName")]
    public string AdUserName { get; set; } = string.Empty;

    [XmlElement(ElementName = "job_No")]
    public string JobNo { get; set; } = string.Empty;

    [XmlElement(ElementName = "utcStatusDateTime")]
    public DateTime UtcStatusDateTime { get; set; }
}