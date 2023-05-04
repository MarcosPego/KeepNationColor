using System;
using System.Windows;
using System.IO;
using Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace CountryBuilder {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            var CurrentDirectory = Directory.GetCurrentDirectory();

            string countriesFile = "";

            //var _list1 = ProcessDirectory(CurrentDirectory + "/../../source/countries_tag/countries/", true, null);
            var _list1 = ProcessTags(CurrentDirectory + "/../../source/country_tags/00_countries.txt");
            var _list2 = ProcessDirectory(CurrentDirectory + "/../../source/countries/", false, _list1);
            string effectString = "";
            string potentialString = "";

            var customNations = new List<string>() {
                "D00","D01","D02","D03","D04","D05","D06","D07","D08","D09"
            };

            var lastTags = new List<string>() {
                "GER", // Germany
                "BYZ", // Byzantium
                "LAE", // Latin Empire
                "TUN", // Tunis
                "GLH", // Golden Horde
                "DLH", // Delhi
                "ETH", // Ethiopia
                "HAB", // Austria
                "FRA", // France
                "ENG", // England
                "BAV", // Bavaria
                "FRN", // Frankfurt
                "HAN", // Hannover
                "GRE", // Greece
                "PRU", // Prussia
                "KUR", // Kurland
                "TUS", // Tuscany
                "MCH", // Manchu
                "CRO", // Croatia
                "DAL", // Dalmatia
            };

            // Tag	Country	Capital	Provinces	Government	First Gov Reform	Primary Culture	Religion	Tech Group
            foreach (var entry in _list2) {
                if (lastTags.Contains(entry.Key)) {
                    continue;
                }

                effectString +=
                 "if = {\n" +
                 "  limit = { was_tag = " + entry.Key + " }\n" +
                 "  change_country_color = {\n" +
                 "      country = " + entry.Key + "\n" +
                 "  }\n" +
                 "  set_country_flag = has_overriden_color_flag\n" +
                 "}\n";

                potentialString +=
                "AND = {\n" +
                " NOT = { tag = " + entry.Key + " }\n" +
                " was_tag = " + entry.Key + "\n" +
                " OR = { ai = no\n" +
                "       AND = { ai = yes\n" +
                "               has_global_flag = ai_will_change_color\n" +
                "             }\n" +
                "       }\n" +
                "}\n";
            }

            foreach (var entry in customNations) {
                effectString +=
                 "if = {\n" +
                 "  limit = { was_tag = " + entry + " }\n" +
                 "  change_country_color = {\n" +
                 "      country = " + entry + "\n" +
                 "  }\n" +
                 "  set_country_flag = has_overriden_color_flag\n" +
                 "}\n";

                potentialString +=
                "AND = {\n" +
                " NOT = { tag = " + entry + " }\n" +
                " was_tag = " + entry + "\n" +
                " OR = { ai = no\n" +
                "       AND = { ai = yes\n" +
                "               has_global_flag = ai_will_change_color\n" +
                "             }\n" +
                "       }\n" +
                "}\n";
            }

            foreach (var entry in lastTags) {
                effectString +=
                 "if = {\n" +
                 "  limit = { was_tag = " + entry + " }\n" +
                 "  change_country_color = {\n" +
                 "      country = " + entry + "\n" +
                 "  }\n" +
                 "  set_country_flag = has_overriden_color_flag\n" +
                 "}\n";

                potentialString +=
                "AND = {\n" +
                " NOT = { tag = " + entry + " }\n" +
                " was_tag = " + entry + "\n" +
                " OR = { ai = no\n" +
                "       AND = { ai = yes\n" +
                "               has_global_flag = ai_will_change_color\n" +
                "             }\n" +
                "       }\n" +
                "}\n";
            }

            var finalFileString =
            "country_decisions = {\n" +
            "    change_back_color_nation = {\n" +
            "        major = yes\n" +
            "        potential = {\n" +
            "            OR = {\n" +
                        potentialString +

            "            }\n" +
            "            NOT = { has_country_flag = has_overriden_color_flag }\n" +
            "        }\n" +
            "        allow = {\n" +
            "        }\n" +
            "        effect = {\n" +
                        effectString +
                            "if = {\n" +
                 "  limit = { ai = yes }\n" +
                 "  clr_country_flag = has_overriden_color_flag\n" +
                 "}\n"+

            "        }\n" +
            "        ai_will_do = {\n" +
            "            factor = 5\n" +

            "        }\n" +
            "         ai_importance = 800\n" +
            "    }\n" +
            "}";

            File.WriteAllText(CurrentDirectory + "/../../source/potential.txt", potentialString);
            File.WriteAllText(CurrentDirectory + "/../../source/effect.txt", effectString);
            File.WriteAllText(CurrentDirectory + "/../../source/ColorDecision.txt", finalFileString);
        }

        public static Dictionary<string,string> ProcessDirectory(string targetDirectory, bool getTag, Dictionary<string, string> inputDir) {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            var dir = new Dictionary<string,string>();

            var brokenList = new List<string>();

            foreach (string fileName in fileEntries) {
                if (getTag) {
                    string[] files = fileName.Split('/');
                    string[] words = files[files.Length - 1].Split('-');

                    string word0 = words[0].Replace(" ", "");
                    string word1 = words[1].Replace(" ", "");

                    dir[word1] = word0;
                } else {
                    var line = ProcessFile(fileName);

                    string[] files = fileName.Split('/');
                    if (line != String.Empty) {
                        var input = files[files.Length - 1];
                        string input1 = input.Replace(" ", "");

                        if (inputDir.ContainsKey(input1)) {
                            var tag = inputDir[input1];
                            dir[tag] = line;
                        } else {
                            brokenList.Add(input1);
                        }

                       
                    }
                }
            }

            if (brokenList.Count > 0) {
                string finalString1 = "";
                foreach (var entry in brokenList) {
                    finalString1 += entry + "\n";
                }

                foreach (var entry in inputDir) {
                    if (!dir.ContainsKey(entry.Value)) {
                        finalString1 += entry.Key + "\n";
                    }
                }

                var CurrentDirectory = Directory.GetCurrentDirectory();
                File.WriteAllText(CurrentDirectory + "/../../source/broken.txt", finalString1);
            }

            return dir;
        }

        public static string ProcessFile(string path) {
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines) {
                if (Regex.Match(line, "color =*").Success) {
                    return line;
                }
            }
            return "";
        }

        public static Dictionary<string,string> ProcessTags(string path) {
            string[] lines = File.ReadAllLines(path);

            var tags = new Dictionary<string, string>();
            foreach (string line in lines) {
                if (Regex.Match(line, ".=*").Success) {
                    string[] words = line.Split('=');

                    string word0 = words[0].Replace(" ", "");
                    string word1 = "";
                    if (words.Length > 1) {
                        word1 = words[1].Replace(" ", "");
                        var noCountry = words[1].Replace(" ", "").Split("/");
                        if (noCountry.Length > 1) {
                            word1 = noCountry[1];
                        }

                    } else {
                        continue;
                    }

                    string word1Proper = word1.Replace("\"", "");
                    word1Proper = word1Proper.Trim('\t');
                    string word0Proper = word0.Replace("#", "");
                    word0Proper = word0Proper.Trim('\t');

                    if (word0Proper.Length == 3) {
                        tags[word1Proper] = word0Proper;
                    }
                }
            }
            return tags;
        }
    }
}
