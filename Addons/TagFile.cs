namespace PURRNext.TagFile
{
    public class TagSearchData
    {
        public string Tag = "";
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
                var TagSearchData = new TagSearchData();
              
                var line = lines[i];
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
                        Console.WriteLine("Removing blank space at the start of the string");
                        tag_string = tag_string.Remove(tag_string.Length-1, 1);
                    }
                    else
                    {
                        Console.WriteLine("No blank space on the end of the string");
                    }
                    
                    //Assigns a tag to the search data
                    TagSearchData.Tag = tag_string;

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
                    TagSearchData.Tag = line;
                    TagSearchData.Pages = 10;
                    TagSearchData.Amount = 75;
                    result.Add(TagSearchData);                    
                }
            }
            return result;
        }
    }
}