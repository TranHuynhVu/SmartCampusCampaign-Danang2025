using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Service
{
    public interface IGemini
    {
        Task<StudentInfor?> ClassifyStudentCV(byte[] pdfBytes);
        Task<List<double>> GetEmbedding(string text);
        Task<string> GetJobEmbedding(Job job);
        Task<string> GetStudentEmbedding(StudentInfor student);
        double CosineSimilarity(List<double> vector1, List<double> vector2);
    }
}
