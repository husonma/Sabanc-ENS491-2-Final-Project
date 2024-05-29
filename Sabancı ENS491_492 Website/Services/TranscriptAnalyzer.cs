namespace Sabancı_ENS491_492_Website.Services
{
    using iTextSharp.text.pdf;
    using iTextSharp.text.pdf.parser;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;

    public class TranscriptAnalyzer
    {
        public List<string> ExtractHighGrades(string path)
        {
            List<string> highGradeCourses = new List<string>();
            bool collectCourses = false;  // Flag to start collecting courses under correct sections
            using (PdfReader reader = new PdfReader(path))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    string text = PdfTextExtractor.GetTextFromPage(reader, i);
                    var lines = text.Split('\n');
                    foreach (var line in lines)
                    {
                        // Check if the line indicates the beginning of course listings
                        if (Regex.IsMatch(line, @"(Fall|Spring|Summer)\s+\d{4}"))
                        {
                            collectCourses = true;  // Start collecting courses
                            continue;
                        }
                        // Stop collecting at the end of courses listing, if some identifier is found, e.g., a line stating "Standing:"
                        if (line.StartsWith("Standing:"))
                        {
                            collectCourses = false;
                        }

                        if (collectCourses)
                        {
                            // Regex to match course code, name, and check for A or A-
                            Match match = Regex.Match(line, @"^(\w+\s+\d+)\s+(.+?)\s+(A-|A)\s");
                            if (match.Success)
                            {
                                highGradeCourses.Add(match.Groups[1].Value);
                            }
                        }
                    }
                }
            }
            return highGradeCourses;
        }
    }



}
