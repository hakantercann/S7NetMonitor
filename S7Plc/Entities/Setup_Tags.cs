namespace S7Plc.Entities
{
    public class Setup_Tags
    {
        public int ID { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public string TagAddress { get; set; } = string.Empty;
        public int? VariableType { get; set; }
        public int? SkipByte { get; set; }
        public int? TakeByte { get; set; }
        public int? LabelType { get; set; }
        public int? ConfigID { get; set; }
    }
}
