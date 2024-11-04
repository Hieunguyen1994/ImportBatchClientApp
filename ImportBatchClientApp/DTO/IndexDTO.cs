using System.Runtime.Serialization;

namespace ImportBatchClientApp.DTO
{
    public class Batch
    {
        public Guid Id { get; set; }
        public string Format { get; set; }
        public string FileName { get; set; }
        public string ExternalReference { get; set; }
    }

    public class RequestData<T>
    {
        public T Data { get; set; }
        public bool UpdateNull { get; set; }
        public string Format { get; set; }
        [DataMember]
        public string Filename { get; set; }
        [DataMember]
        public string ExternalReference { get; set; }
    }
}
