using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class Result<T> where T : class
    {
        [DataMember]
        public T SomeResult { get; set; }

        [DataMember]
        public bool BoolRes { get; set; }

        [DataMember]
        public string ErrorRes { get; set; }
    }
}