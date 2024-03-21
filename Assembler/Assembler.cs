using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembler
{
    public class Assembler
    {
        public required string Src;
        public Tokenization tokenization;
        public Generation generation;
        public void Build()
        {
            tokenization = new Tokenization();
            tokenization.Build(Src);
            generation = new Generation()
            {
                m_src = tokenization.tokens.ToArray()
            };
            generation.Build();
        }
    }
}
