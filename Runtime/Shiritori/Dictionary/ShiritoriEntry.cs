using System;

namespace Shiritori.Dictionary
{
    [Serializable]
    public struct ShiritoriEntry
    {
        public string Reading;    // ひらがな
        public string Surface;    // "紙/神" みたいに / 区切り
        public string Pos2;       // 普通名詞 / 固有名詞 / 数詞 ...
        public string Pos3;       // 一般 / 地名 / 人名 ...
        public string Compound;   // A / B / C
    }
}