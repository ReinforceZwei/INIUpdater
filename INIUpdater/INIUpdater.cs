using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INIUpdater
{
    public static class INIUpdater
    {
        /// <summary>
        /// Merge two ini file. 
        /// New ini option will be inserted into old ini file. 
        /// Old config value will be kept despite the key of both new and old ini are the same
        /// </summary>
        /// <remarks>
        /// Modifying comment will cause new comment to be inserted instead of replacing old comment.
        /// </remarks>
        /// <param name="newINI"></param>
        /// <param name="oldINI"></param>
        /// <returns></returns>
        public static List<string> Merge(string[] newINI, string[] oldINI)
        {
            var newLines = ToINILine(new List<string>(newINI));
            var oldLines = ToINILine(new List<string>(oldINI));
            for (int i = 0; i < newLines.Count; i++)
            {
                var oldIndex = oldLines.IndexOf(newLines[i]);
                if (oldIndex != -1)
                {
                    if (!string.IsNullOrWhiteSpace(newLines[i].InlineComment)
                        && newLines[i].InlineComment != oldLines[oldIndex].InlineComment)
                    {
                        oldLines[oldIndex].InlineComment = newLines[i].InlineComment;
                    }
                    continue;
                }

                if (i == 0)
                {
                    oldLines.Insert(0, newLines[i]);
                }
                else
                {
                    var lastLine = newLines[i - 1];
                    int idx;
                    do
                    {
                        idx = oldLines.IndexOf(lastLine);
                        if (idx != -1)
                            oldLines.Insert(idx + 1, newLines[i]);
                    } while (idx == -1);
                }
            }
            var final = new List<string>();
            foreach (var line in oldLines)
            {
                final.Add(line.ToString());
            }
            return final;
        }

        private static List<INILine> ToINILine(List<string> ini)
        {
            var result = new List<INILine>();
            string currentSection = "";
            for (int i = 0; i < ini.Count; i++)
            {
                var line = new INILine(ini[i], currentSection);
                if (line.IsSection)
                {
                    currentSection = line.Value;
                }
                result.Add(line);
            }
            return result;
        }
    }

    public class INILine
    {
        public bool IsConfig { get; set; } = false;
        public bool IsSection { get; set; } = false;
        public bool IsComment { get; set; } = false;
        public bool IsEmpty { get; set; } = false;
        public bool HasInlineComment { get; set; } = false;
        public string Key { get; set; }
        public string Value { get; set; }
        /// <summary>
        /// Section name that this line belongs to
        /// </summary>
        public string Section { set; get; }
        public string InlineComment { set; get; }

        public INILine(string line, string section = "")
        {
            Section = section;
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                IsSection = true;
                Value = line;
                Section = line;
            }
            else if (line.StartsWith("#") || line.StartsWith(";"))
            {
                IsComment = true;
                Value = line;
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                IsEmpty = true;
            }
            else if (line.Contains("="))
            {
                string[] part = line.Split(new char[] { '=' }, 2); // Split first "=" only
                IsConfig = true;
                Key = part[0];
                Value = part[1];
                if (Value.Contains("#") || Value.Contains(";"))
                {
                    int index = Value.Trim().IndexOf(' ');
                    HasInlineComment = true;
                    InlineComment = Value.Trim().Substring(index);
                    Value = Value.Substring(0, index);
                }
            }
            else
            {
                throw new InvalidOperationException("Not an ini configuration line");
            }
        }

        public override string ToString()
        {
            if (IsComment || IsSection) return Value;
            if (IsConfig && HasInlineComment) return Key + "=" + Value + InlineComment;
            if (IsConfig) return Key + "=" + Value;
            if (IsEmpty) return "";
            return "";
        }

        /// <summary>
        /// </summary>
        /// <remarks>If it is Config, we don't check Value to keep old config value</remarks>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is INILine)
            {
                var i = (INILine)obj;
                if (IsConfig && i.IsConfig) return i.Key == Key && i.Section == Section;
                else return i.Key == Key && i.Section == Section && i.Value == Value;
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <remarks>If it is Config, we don't check Value to keep old config value</remarks>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (IsConfig) return Key.GetHashCode() ^ Section.GetHashCode();
            else return Key.GetHashCode() ^ Section.GetHashCode() ^ Value.GetHashCode();
        }
    }
}
