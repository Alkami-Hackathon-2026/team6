using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace HACK26.MS.MyMoneyRules.Data
{
    public class FieldDefinition
    {
        [Key] public int FieldId { get; set; }
        [MaxLength(100)] public string FieldName { get; set; } = string.Empty;
        [MaxLength(20)] public string DataType { get; set; } = string.Empty;
        public string AllowedOperatorsJson { get; set; } = "[]";

        [NotMapped]
        public List<string> AllowedOperators
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AllowedOperatorsJson))
                {
                    return new List<string>();
                }

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(AllowedOperatorsJson)))
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<string>));
                    return (List<string>)serializer.ReadObject(stream) ?? new List<string>();
                }
            }
            set
            {
                var serializer = new DataContractJsonSerializer(typeof(List<string>));

                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, value ?? new List<string>());
                    AllowedOperatorsJson = Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }
    }
}
