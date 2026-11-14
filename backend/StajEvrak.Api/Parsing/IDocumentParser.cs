namespace StajEvrak.Api.Parsing;

///"Bir belge dosyasından içerik oku" sözleşmesi. Çağıran, dosyanın PDF mi Word mü olduğunu bilmez.
public interface IDocumentParser
{
    bool CanParse(string extension);          // ör. ".pdf"
    ParsedDocument Parse(string filePath);
}