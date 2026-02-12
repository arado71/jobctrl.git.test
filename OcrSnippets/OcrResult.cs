namespace TcT.OcrSnippets
{
    public class OcrResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Text { get; set; }
        public float Confidence { get; set; }
    }
}