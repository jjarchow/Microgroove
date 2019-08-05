using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MicogrooveChallenge
{
    public class Converter {
        public class Record {
            public string date;
            public string type;
            public List<Order> orders;
            public Ender ender;
        }
        public class Ender {
            public string process;
            public string paid;
            public string created;
        }
        public class Order
        {
            public string date;
            public string code;
            public Buyer buyer;
            public Timings timings;
            public List<Item> items;
        }
        public class Item
        {
            public string sku;
            public string qty;
        }
        public class Buyer
        {
            public string name;
            public string street;
            public string zip;
        }
        public class Timings
        {
            public string start; 
            public string stop; 
            public string gap; 
            public string offset; 
            public string pause;
        }

        // Convert a csv containing a File record to a JSON file.
        // Input:  name of csv file
        // Return:  false if error or invalid input file format, else true.
        //   Assumes one File record per input file.
        //   Input file must contain one 'F' and one 'E' record.
        //   Each optional 'O' record must contain a single B & T record.
        //   Each record must contain minumum number of parameters.
        //   Illegal record types not allowed.
        //
        public bool ConvertCSVtoJSON(string input)
        {
            try {
                // read the input file
                string[] inputLines = File.ReadAllLines(input);
                int index = -1;
                Record record = null;

                // convert each input file line to its JSON object
                foreach(string entry in inputLines)
                {
                    // parse the input string
                    string s = entry.Replace("\"", "");
                    string[] words = s.Split(',');
                    if (words.Length < 3) return false;

                    if ("F" == words[0])
                    {
                        record = new Record();
                        record.orders = new List<Order>();
                        record.date = words[1];
                        record.type = words[2];
                    }
                    else if ("E" == words[0])
                    {
                        if (words.Length >= 4) {
                            record.ender = new Ender();;
                            record.ender.process = words[1];
                            record.ender.paid = words[2];
                            record.ender.created = words[3];
                        }
                        else
                            return false;
                    }
                    else if ("O" == words[0])
                    {
                        if (words.Length >= 4) {
                            index++;
                            record.orders.Add(new Order() {date = words[1], code=words[2]});
                            record.orders[index].items = new List<Item>();
                            ArrayList items = new ArrayList();
                            items.Add(new Buyer() {name=words[1], street = words[2]});
                        }
                        else
                            return false;
                    }
                    else if ("L" == words[0])
                    {
                        record.orders[index].items.Add(new Item() {
                            sku=words[1], qty=words[2]
                            });
                    }
                    else if ("B" == words[0])
                    {
                        if (words.Length >= 4) {
                        record.orders[index].buyer = new Buyer() {name=words[1], street = words[2], zip = words[3]};
                        }
                        else
                            return false;
                    }
                    else if ("T" == words[0])
                    {
                        if (words.Length >= 6) {
                        record.orders[index].timings = new Timings() 
                        {
                            start = words[1], stop = words[2], gap = words[3], offset = words[4], pause = words[5]
                        };
                        }
                        else
                            return false;
                    }
                    else
                        return false;   // illegal file record
                }

                // validate record contents
                if (record.ender == null)
                    return false;
                foreach (Order orderItem in record.orders) {
                    if (orderItem.buyer == null || orderItem.timings == null)
                        return false;
                }

                string result = JsonConvert.SerializeObject(record, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(".\\output.json", result);
            }
            catch (Exception e)
            {
                // error return with e
                Console.Write(e.Message);
                return false;
            }

            return true;
        }
    }
    class Program
    {
        //public static string InputFile = ".\\input.csv";
        static void Main(string[] args)
        {
                Converter cv = new Converter();
                bool retVal = cv.ConvertCSVtoJSON(".\\input.csv");
        }
    }
}
