using System;

namespace PoalimOnlineBusiness
{
    public class FixedLengthFileAttribute : Attribute
    {
        public FixedLengthFileAttribute(int sortOrder, int position, int length, DataType dataType, string format = null, Align align = Align.Left)
        {
            this.SortOrder = sortOrder;
            this.Position = position;
            this.Length = length;
            this.DataType = dataType;
            this.Format = format;
            this.Align = Align.Left;
        }

        public DataType DataType { get; set; } = DataType.X;

        public int SortOrder { get; set; } = 1;

        public int Length { get; set; } = 1;

        public int Position { get; set; } = 1;

        public string Format { get; set; }

        public Align Align { get; set; } = Align.Left;

    }

    public enum DataType
    {
        /// <summary>
        /// Text
        /// </summary>
        X,
        /// <summary>
        /// Number
        /// </summary>
        N
    }

    public enum Align
    {
        Right,
        Left
    }
}
