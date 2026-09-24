using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// A transaction field that rules can check, with its data type and allowed operators. Stored in core.UserEngineFieldDefinitions.
    /// </summary>
    public class FieldDefinition
    {
        /// <summary>
        /// Field definition identifier
        /// </summary>
        [Key] public int FieldId { get; set; }

        /// <summary>
        /// Name of the transaction field, such as amount
        /// </summary>
        [MaxLength(100)] public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Data type of the field, such as decimal, string or bool
        /// </summary>
        [MaxLength(20)] public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Allowed operators stored as a JSON array of strings
        /// </summary>
        public string AllowedOperatorsJson { get; set; } = "[]";

        /// <summary>
        /// Allowed operators; reads and writes <see cref="AllowedOperatorsJson"/>
        /// </summary>
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
