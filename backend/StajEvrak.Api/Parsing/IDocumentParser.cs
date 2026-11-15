namespace StajEvrak.Api.Parsing;

// Soyutlama: çağıran, dosyanın PDF mi Word mü olduğunu bilmez.
public interface IDocumentParser
{
    bool CanParse(string extension);
    ParsedDocument Parse(string filePath);
}