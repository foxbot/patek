using System.ComponentModel.DataAnnotations.Schema;

namespace Patek.Data
{
    public class Tag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public long __OwnerId { get; set; }
        [NotMapped]
        public ulong OwnerId
        {
            get => (ulong)__OwnerId;
            set => __OwnerId = (long)value;
        }
        public uint Color { get; set; }
    }
}
