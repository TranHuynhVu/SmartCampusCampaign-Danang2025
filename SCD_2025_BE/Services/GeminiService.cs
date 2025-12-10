using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Service
{
    public class GeminiService : IGemini
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private const string GenerateContentModel = "gemini-2.0-flash-exp";
        private const string EmbeddingModel = "text-embedding-004";

        public GeminiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiKey = _configuration["Gemini:Key"] ?? throw new ArgumentNullException("Gemini API Key not found");
        }

        public async Task<StudentInfor?> ClassifyStudentCV(byte[] pdfBytes, string fileName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GenerateContentModel}:generateContent?key={_apiKey}";

                // Convert PDF to base64
                var base64Pdf = Convert.ToBase64String(pdfBytes);

                var prompt = @"
Bạn là một AI chuyên phân tích CV. Hãy trích xuất thông tin từ CV này và trả về dưới dạng JSON với các trường sau:
{
  ""name"": ""Tên sinh viên"",
  ""gpa"": ""Điểm GPA hoặc điểm trung bình"",
  ""skills"": ""Các kỹ năng (ngăn cách bằng dấu phẩy)"",
  ""achievements"": ""Các thành tích, giải thưởng"",
  ""yearOfStudy"": ""Năm học hiện tại hoặc năm tốt nghiệp"",
  ""major"": ""Chuyên ngành"",
  ""languages"": ""Ngôn ngữ (ví dụ: Tiếng Anh, Tiếng Việt)"",
  ""certifications"": ""Các chứng chỉ"",
  ""experiences"": ""Kinh nghiệm làm việc/thực tập"",
  ""projects"": ""Các dự án đã thực hiện""
}

Nếu không tìm thấy thông tin nào, để giá trị null. CHỈ trả về JSON, không thêm text giải thích.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = prompt },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = "application/pdf",
                                        data = base64Pdf
                                    }
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.1,
                        maxOutputTokens = 2048
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Gemini API Error: {responseContent}");
                    return null;
                }

                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
                var textResponse = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text;

                if (string.IsNullOrEmpty(textResponse))
                    return null;

                // Extract JSON from response (remove markdown code blocks if present)
                var jsonText = textResponse.Trim();
                if (jsonText.StartsWith("```json"))
                {
                    jsonText = jsonText.Substring(7);
                    jsonText = jsonText.Substring(0, jsonText.LastIndexOf("```")).Trim();
                }
                else if (jsonText.StartsWith("```"))
                {
                    jsonText = jsonText.Substring(3);
                    jsonText = jsonText.Substring(0, jsonText.LastIndexOf("```")).Trim();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var cvData = JsonSerializer.Deserialize<CVData>(jsonText, options);

                if (cvData == null)
                    return null;

                return new StudentInfor
                {
                    Name = cvData.Name ?? "Unknown",
                    GPA = cvData.GPA,
                    Skills = cvData.Skills,
                    Archievements = cvData.Achievements,
                    YearOfStudy = cvData.YearOfStudy,
                    Major = cvData.Major,
                    Languages = cvData.Languages,
                    Certifications = cvData.Certifications,
                    Experiences = cvData.Experiences,
                    Projects = cvData.Projects,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ClassifyStudentCV: {ex.Message}");
                return null;
            }
        }

        public async Task<List<double>> GetEmbedding(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return new List<double>();

                var client = _httpClientFactory.CreateClient();
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{EmbeddingModel}:embedContent?key={_apiKey}";

                var requestBody = new
                {
                    model = $"models/{EmbeddingModel}",
                    content = new
                    {
                        parts = new[]
                        {
                            new { text = text }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Gemini Embedding API Error: {responseContent}");
                    return new List<double>();
                }

                var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent);
                return embeddingResponse?.embedding?.values ?? new List<double>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetEmbedding: {ex.Message}");
                return new List<double>();
            }
        }

        public double CosineSimilarity(List<double> vector1, List<double> vector2)
        {
            if (vector1 == null || vector2 == null || vector1.Count == 0 || vector2.Count == 0)
                return 0;

            if (vector1.Count != vector2.Count)
                return 0;

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Count; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        public async Task<string> GetJobEmbedding(Job job)
        {
            try
            {
                var combinedText = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(job.Requirements))
                    combinedText.Add($"Requirements: {job.Requirements}");
                
                if (!string.IsNullOrWhiteSpace(job.NiceToHave))
                    combinedText.Add($"Nice to have: {job.NiceToHave}");

                if (combinedText.Count == 0)
                    return "[]";

                var textToEmbed = string.Join(". ", combinedText);
                var embedding = await GetEmbedding(textToEmbed);
                
                return JsonSerializer.Serialize(embedding);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetJobEmbedding: {ex.Message}");
                return "[]";
            }
        }

        public async Task<string> GetStudentEmbedding(StudentInfor student)
        {
            try
            {
                var combinedText = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(student.Skills))
                    combinedText.Add($"Skills: {student.Skills}");
                
                if (!string.IsNullOrWhiteSpace(student.Major))
                    combinedText.Add($"Major: {student.Major}");
                
                if (!string.IsNullOrWhiteSpace(student.Experiences))
                    combinedText.Add($"Experiences: {student.Experiences}");
                
                if (!string.IsNullOrWhiteSpace(student.Projects))
                    combinedText.Add($"Projects: {student.Projects}");
                
                if (!string.IsNullOrWhiteSpace(student.Certifications))
                    combinedText.Add($"Certifications: {student.Certifications}");

                if (combinedText.Count == 0)
                    return "[]";

                var textToEmbed = string.Join(". ", combinedText);
                var embedding = await GetEmbedding(textToEmbed);
                
                return JsonSerializer.Serialize(embedding);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStudentEmbedding: {ex.Message}");
                return "[]";
            }
        }

        // Helper classes for JSON deserialization
        private class GeminiResponse
        {
            public Candidate[]? candidates { get; set; }
        }

        private class Candidate
        {
            public Content? content { get; set; }
        }

        private class Content
        {
            public Part[]? parts { get; set; }
        }

        private class Part
        {
            public string? text { get; set; }
        }

        private class EmbeddingResponse
        {
            public EmbeddingData? embedding { get; set; }
        }

        private class EmbeddingData
        {
            public List<double>? values { get; set; }
        }

        private class CVData
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("gpa")]
            public string? GPA { get; set; }

            [JsonPropertyName("skills")]
            public string? Skills { get; set; }

            [JsonPropertyName("achievements")]
            public string? Achievements { get; set; }

            [JsonPropertyName("yearOfStudy")]
            public string? YearOfStudy { get; set; }

            [JsonPropertyName("major")]
            public string? Major { get; set; }

            [JsonPropertyName("languages")]
            public string? Languages { get; set; }

            [JsonPropertyName("certifications")]
            public string? Certifications { get; set; }

            [JsonPropertyName("experiences")]
            public string? Experiences { get; set; }

            [JsonPropertyName("projects")]
            public string? Projects { get; set; }
        }
    }
}
