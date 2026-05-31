using System.Text;
using System.Text.RegularExpressions;

namespace PURRNext.TagFile
{
    public class TagSearchData
    {
        public string Search = "";
        public int Pages = -1;
        public int Amount = -1;
    }
    public class TagImporter
    {
        public static List<TagSearchData> ImportTags(string path)
        {
            var result = new List<TagSearchData>();
            var lines = File.ReadAllLines(path);
            for(int i = 0; i < lines.Count(); i++)
            {
              
                var line = lines[i];

                line.Trim();
                //t:tags/p:pages/a:amount_of_posts
                //t:string/p:number/a:number
                var regex_parse = Regex.Match(line, @"t:(.+?)\/p:(\d+)\/a:(\d+)");

                if(regex_parse.Success)
                {
                    //Search Query
                    var search = regex_parse.Groups[1].Value;

                    //Setups the search query string
                    /*
                        Trims the whole string
                        Splits into substrings then Trims each entry and remove whitespaced/empty entries
                        Join everything back
                    */
                    search.Trim();
                    var splitted_search = search.Split(" ", options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    search = string.Join(" ", splitted_search);

                    //Amount of pages to go through
                    var pages = Int32.Parse(regex_parse.Groups[2].Value);
                    //Amount of posts per page
                    var amount = Int32.Parse(regex_parse.Groups[3].Value);

                    Console.WriteLine($"Search from line {i} has been successfuly parsed!");

                    Console.WriteLine($"Search: '{search}'\nPages: '{pages}'\nAmount of Posts: '{amount}'");
                    var TSD = new TagSearchData
                    {
                        Search = search,
                        Pages = pages,
                        Amount = amount
                    };

                    result.Add(TSD);
                }
                else
                {
                    Console.WriteLine($"Line {i} has failed to be parsed");
                }

                /*
                    if(line.Contains("p:") || line.Contains("a:"))
                    {
                        var separations = line.Split(" : ");

                        //Check if there isn't any empty space on the tag string end or start
                        var tag_string = separations[0];
                        if(tag_string.ElementAt(0) == ' ')
                        {
                            Console.WriteLine("Removing blank space at the start of the string");
                            tag_string = tag_string.Remove(0, 1);
                        }
                        else
                        {
                            Console.WriteLine("No blank space on the start of the string");
                        }
                        if(tag_string.ElementAt(tag_string.Length-1) == ' ')
                        {
                            Console.WriteLine("Removing blank space at the end of the string");
                            tag_string = tag_string.Remove(tag_string.Length-1, 1);
                        }
                        else
                        {
                            Console.WriteLine("No blank space on the end of the string");
                        }
                        
                        //Assigns a tag to the search data
                        TagSearchData.Search = tag_string;

                        var options = separations[1].Split(" ");
                        Console.WriteLine($"Options - {options.Length}");
                        for (int o = 0; o < options.Length; o++)
                        {
                            if (options[o] != "")
                            {
                                Console.WriteLine(options[o]);
                                if(options[o].Contains("p:"))
                                {
                                    try
                                    {
                                        int pages = Int32.Parse(options[o].Replace("p:", ""));
                                        TagSearchData.Pages = pages;
                                    }
                                    catch(Exception e)
                                    {
                                        Console.WriteLine($"An error has occurred\nError:{e}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No pagination option found, using default value");
                                    TagSearchData.Pages = 10;
                                }
                                if(options[o].Contains("a:"))
                                {
                                    try
                                    {
                                        int amount = Int32.Parse(options[o].Replace("a:", ""));
                                        TagSearchData.Amount = amount;
                                    }
                                    catch(Exception e)
                                    {
                                        Console.WriteLine($"An error has occurred\nError:{e}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No quantity option found, using default value");
                                    TagSearchData.Amount = 75;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Empty String removed at index - {o}");
                            }
                        }
                        result.Add(TagSearchData);
                    }
                    else
                    {
                        Console.WriteLine("No options provided utilizing default values");
                        Console.WriteLine("Formatting string");
                        var tag_string = line;
                        if(tag_string.ElementAt(0) == ' ')
                        {
                            Console.WriteLine("Removing blank space at the start of the string");
                            tag_string = tag_string.Remove(0, 1);
                        }
                        else
                        {
                            Console.WriteLine("No blank space on the start of the string");
                        }
                        if(tag_string.ElementAt(tag_string.Length-1) == ' ')
                        {
                            Console.WriteLine("Removing blank space at the end of the string");
                            tag_string = tag_string.Remove(tag_string.Length-1, 1);
                        }
                        else
                        {
                            Console.WriteLine("No blank space on the end of the string");
                        }
                        TagSearchData.Search = line;
                        TagSearchData.Pages = 10;
                        TagSearchData.Amount = 75;
                        result.Add(TagSearchData);                    
                    }
                */
            }
            return result;
        }
        public static void ClearTags(string path)
        {
            using (FileStream fs = File.Create(path))
            {
                byte[] info = new UTF8Encoding(true).GetBytes($"");
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
                fs.Close(); //Remove in case of regret
            }
        }
    }
}