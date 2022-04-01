using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Renderer
{
    public class VertexFormatElement
    {
        public VertexFormatSemantic Semantic;
        public VertexPointerType Type;
        public byte Count;
        public short Offset;
        public short Divisor;
        public bool Normalized;

        public VertexFormatElement(VertexFormatSemantic semantic, VertexPointerType type, byte count, short offset, short divisor = 0, bool normalized = false)
        {
            Semantic = semantic;
            Type = type;
            Count = count;
            Offset = offset;
            Divisor = divisor;
            Normalized = normalized;
        }
    }
}
