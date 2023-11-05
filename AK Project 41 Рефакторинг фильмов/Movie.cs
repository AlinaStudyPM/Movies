using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AK_Project_41_Рефакторинг_фильмов
{
    internal class Movie
    {
        public List<string> Titles { get; set; }
        public string Code { get; set; }
        public string Code2 { get; set; }
        public string Director { get; set; }
        public string Rating { get; set; }
        public HashSet<string> CodsOfActors { get; set; }
        public HashSet<string> Tags { get; set; }

        public Movie(string title, string code)
        {
            Titles = new List<string> { title };
            Code = code;
            Director = "";
            Rating = "";
            CodsOfActors = new HashSet<string>();
            Tags = new HashSet<string>();
        }
        public void AddTitle(string title) { Titles.Add(title); }
        public void AddDirector(string director) { Director = director; }
        public void AddRating(string rating) { Rating = rating; }
        public void AddActor(string actor) { CodsOfActors.Add(actor); }
        public void AddTag(string tag) { Tags.Add(tag); }
        public void AddCode2(string code) { Code2 = code; }
        public void Print(Dictionary<string, Actor> ActorByCode)
        {
            Console.WriteLine(Code + "  ");
            Console.Write("Название: ");
            int q = 0;
            foreach (string title in Titles)
            {
                if (q > 0) { Console.Write(" | "); }
                Console.Write(title);
                q++;
            }
            Console.WriteLine();
            Console.WriteLine("Рейтинг: " + Rating);
            if (Director != "")
            {
                Console.WriteLine($"Режиссер: {ActorByCode[Director].Name}");
            }
            Console.Write("Актеры:");
            int p = 0;
            foreach (string actorCode in CodsOfActors)
            {
                if (p > 0) Console.Write(",");
                Console.Write(" " + ActorByCode[actorCode].Name);
                p++;
            }
            Console.WriteLine();
            Console.Write("Теги: |");
            foreach (string tag in Tags)
            {
                Console.Write(" " + tag + " |");
            }
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
