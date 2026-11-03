using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;
using System.Text.RegularExpressions;

namespace HireHub.Services.ATS
{
    public class AtsService : IAtsService
    {
        public async Task<string> ExtractTextAsync(IFormFile file)
        {
            var text = string.Empty;
            var ext = Path.GetExtension(file.FileName).ToLower();

            using var stream = file.OpenReadStream();

            if (ext == ".pdf")
            {
                using var pdf = PdfDocument.Open(stream);
                foreach (var page in pdf.GetPages())
                {
                    text += page.Text + " ";
                }
            }
            else if (ext == ".docx")
            {
                using var wordDoc = WordprocessingDocument.Open(stream, false);
                text = wordDoc.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
            }
            else
            {
                throw new InvalidOperationException("Only PDF and DOCX files are supported for ATS scoring.");
            }

            return text;
        }

        public (int Score, List<string> PassedChecks, List<string> FailedChecks) CalculateGeneralScore(string resumeText)
        {
            var passed = new List<string>();
            var failed = new List<string>();
            int score = 0;

            var lowerText = resumeText.ToLower();

            // 1. Contact Info (Email & Phone) - 15 Points
            bool hasEmail = Regex.IsMatch(lowerText, @"[\w\.-]+@[\w\.-]+\.\w+");
            bool hasPhone = Regex.IsMatch(lowerText, @"\+?\d[\d\s-]{8,}");

            if (hasEmail && hasPhone)
            {
                score += 15;
                passed.Add("Contact information (Email & Phone) found.");
            }
            else
            {
                failed.Add("Missing complete contact information (Email or Phone).");
            }

            // 2. Length Check - 10 Points
            int wordCount = lowerText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount >= 200 && wordCount <= 800)
            {
                score += 10;
                passed.Add("Resume length is optimal (200-800 words).");
            }
            else
            {
                failed.Add("Resume length is not ideal. Aim for 200-800 words.");
            }

            // 3. Essential Sections - 40 Points (10 points each)
            string[] sections = { "experience", "education", "skills", "projects" };
            foreach (var section in sections)
            {
                if (lowerText.Contains(section))
                {
                    score += 10;
                    passed.Add($"'{section}' section found.");
                }
                else
                {
                    failed.Add($"Missing a dedicated '{section}' section.");
                }
            }

            // 4. Action Verbs - 15 Points
            string[] actionVerbs = { "developed", "managed", "created", "led", "designed", "implemented", "increased", "reduced" };
            int verbCount = actionVerbs.Count(v => lowerText.Contains(v));
            if (verbCount >= 3)
            {
                score += 15;
                passed.Add("Good use of action verbs (e.g., developed, led, managed).");
            }
            else
            {
                failed.Add("Weak action verbs. Use words like 'developed', 'led', or 'managed'.");
            }

            // 5. Readability Check - 20 Points
            // If the text is very short, it's likely an image PDF or poorly parsed.
            if (resumeText.Length > 200)
            {
                score += 20;
                passed.Add("Text is readable by ATS (not an image).");
            }
            else
            {
                failed.Add("Resume text is too short or could not be parsed (might be an image).");
            }

            // Cap score at 100
            score = Math.Min(score, 100);

            return (score, passed, failed);
        }
    }
}