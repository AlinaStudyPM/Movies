using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AK_Project_41_Рефакторинг_фильмов
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, Movie> MovieByCode; //код 
            Dictionary<string, string> MovieCodeByTitle;
            Dictionary<string, Actor> ActorByCode;
            Dictionary<string, HashSet<string>> ActorCodesByName;
            Dictionary<string, HashSet<string>> CodesByCode2;
            Dictionary<string, string> TagByCode;
            Dictionary<string, string> CodeTagByName;

            //Dictionary<string, HashSet<string>> MovieCodesByActor;
            Dictionary<string, HashSet<string>> MovieCodesByTag;
            Stopwatch StopWatch = new Stopwatch();
            GC.Collect();

            StopWatch.Start();

            MovieByCode = new Dictionary<string, Movie>();
            MovieCodeByTitle = new Dictionary<string, string>();
            int[] index = new int[5];
            foreach (string s in File.ReadLines("MovieCodes_IMDB.tsv"))
            {
                index[0] = s.IndexOf('\t');
                for (int i = 1; i < index.Length; i++)
                {
                    index[i] = s.IndexOf('\t', index[i - 1] + 1);
                }
                string region = s.Substring(index[2] + 1, index[3] - index[2] - 1);
                string lang = s.Substring(index[3] + 1, index[4] - index[3] - 1);
                if (region == "RU" || region == "GB" || region == "US" || lang == "ru" || lang == "en")
                {
                    string code = s.Substring(0, index[0]);
                    string title = s.Substring(index[1] + 1, index[2] - index[1] - 1);
                    if (MovieByCode.TryGetValue(code, out Movie movie))
                    {
                        movie.AddTitle(title);
                    }
                    else
                    {
                        MovieByCode.Add(code, new Movie(title, code));
                    }
                    if (!MovieCodeByTitle.ContainsKey(title))
                    {
                        MovieCodeByTitle.Add(title, code);
                    }
                }
            }
            Console.WriteLine("Создание словарей MovieByCode и MovieCodeByTitle завершено " + StopWatch.Elapsed);
 
            
            

            ActorByCode = new Dictionary<string, Actor>();
            ActorCodesByName = new Dictionary<string, HashSet<string>>();
            index = new int[2];
            foreach (string s in File.ReadLines("ActorsDirectorsNames_IMDB.txt"))
            {
                index[0] = s.IndexOf('\t');
                for (int i = 1; i < index.Length; i++)
                {
                    index[i] = s.IndexOf('\t', index[i - 1] + 1);
                }
                string code = s.Substring(0, index[0]);
                string name = s.Substring(index[0] + 1, index[1] - index[0] - 1);
                ActorByCode.Add(code, new Actor(name, code));
                if (ActorCodesByName.TryGetValue(name, out HashSet<string> codes))
                {
                    codes.Add(code);
                }
                else
                {
                    ActorCodesByName.Add(name, new HashSet<string> { code });
                }
            }
            Console.WriteLine("Создание словаря ActorByCode завершено " + StopWatch.Elapsed);  //Многовато

            index = new int[4];
            foreach (string s in File.ReadLines("ActorsDirectorsCodes_IMDB.tsv"))
            {
                index[0] = s.IndexOf('\t');
                for (int i = 1; i < index.Length; i++)
                {
                    index[i] = s.IndexOf('\t', index[i - 1] + 1);
                }
                
                string category = s.Substring(index[2] + 1, index[3] - index[2] - 1);
                if (category == "actor")
                {
                    string movieCode = s.Substring(0, index[0]);
                    string actorCode = s.Substring(index[1] + 1, index[2] - index[1] - 1);
                    if (MovieByCode.TryGetValue(movieCode, out Movie movie) && ActorByCode.TryGetValue(actorCode, out Actor actor))
                    {
                        movie.AddActor(actor.Code);
                        actor.AddMovie(movieCode);
                    }
                }
                if (category == "director")
                {
                    string movieCode = s.Substring(0, index[0]);
                    string actorCode = s.Substring(index[1] + 1, index[2] - index[1] - 1);
                    if (MovieByCode.TryGetValue(movieCode, out Movie movie) && ActorByCode.TryGetValue(actorCode, out Actor actor))
                    {
                        movie.AddDirector(actor.Code);
                        actor.AddMovie(movieCode);
                    }
                }
            }
            Console.WriteLine("Добавление актеров к фильмам завершено " + StopWatch.Elapsed);  //Непозволительно долго!

            index = new int[2];
            foreach (string s in File.ReadLines("Ratings_IMDB.tsv"))
            {
                index[0] = s.IndexOf('\t');
                for (int i = 1; i < index.Length; i++)
                {
                    index[i] = s.IndexOf('\t', index[i - 1] + 1);
                }
                string codeMovie = s.Substring(0, index[0]);
                if (MovieByCode.TryGetValue(codeMovie, out Movie movie))
                {
                    string rating = s.Substring(index[0] + 1, index[1] - index[0] - 1);
                    movie.AddRating(rating);
                }
            }
            Console.WriteLine("Добавление рейтинга к фильмам завершено " + StopWatch.Elapsed);

            CodesByCode2 = new Dictionary<string, HashSet<string>>();
            index = new int[2];
            foreach (var s in File.ReadLines("links_IMDB_MovieLens.csv"))
            {
                index[0] = s.IndexOf(',');
                for (int i = 1; i < index.Length; i++)
                {
                    index[i] = s.IndexOf(',', index[i - 1] + 1);
                }
                string code2 = s.Substring(0, index[0]);
                string codeMovie = "tt" + s.Substring(index[0] + 1, index[1] - index[0] - 1);
                    if (code2 != "")
                    {
                        if (CodesByCode2.TryGetValue(code2, out HashSet<string> moviesCodes))
                        {
                            moviesCodes.Add(codeMovie);
                            //Console.WriteLine("Повторился код " + info[0] + ": " + movie.Title + "    " + MovieByCode2[info[2]].Title);
                        }
                        else
                        {
                            CodesByCode2.Add(code2, new HashSet<string>(new string[] { codeMovie }));
                        }
                }

            }
            Console.WriteLine("Сопоставление кодов фильмов завершено " + StopWatch.Elapsed); //Очень быстро

            TagByCode = new Dictionary<string, string>();
            CodeTagByName = new Dictionary<string, string>();
            index = new int[2];
            foreach (string s in File.ReadLines("TagCodes_MovieLens.csv"))
            {
                index[0] = s.IndexOf(',');
                index[1] = s.Length;
                string codeTag = s.Substring(0, index[0]);
                string tag = s.Substring(index[0] + 1, index[1] - index[0] - 1);
                TagByCode.Add(codeTag, tag);
                CodeTagByName.Add(tag, codeTag);
            }
            Console.WriteLine("Создание словаря TagByCode завершено " + StopWatch.Elapsed); //Очень быстро

            MovieCodesByTag = new Dictionary<string, HashSet<string>>();
            index = new int[3];
            foreach (var s in File.ReadLines("TagScores_MovieLens.csv"))
            {
                index[0] = s.IndexOf(',');
                for (int i = 1; i < index.Length - 1; i++)
                {
                    index[i] = s.IndexOf(',', index[i - 1] + 1);
                }
                index[2] = s.Length;
                string code2Movie = s.Substring(0, index[0]);
                string codeTag = s.Substring(index[0] + 1, index[1] - index[0] - 1);
                string relevance = s.Substring(index[1] + 1, index[2] - index[1] - 1);
                if (Convert.ToInt32(relevance[2]) >= Convert.ToInt32('5'))
                {
                    if (CodesByCode2.TryGetValue(code2Movie, out HashSet<string> codesMovies) && TagByCode.TryGetValue(codeTag, out string tag))
                    {
                        foreach (string code in codesMovies)
                        {
                            if (MovieByCode.TryGetValue(code, out Movie movie))
                            {
                                movie.AddTag(tag);
                            }
                            if (MovieCodesByTag.TryGetValue(tag, out HashSet<string> codesForTag))
                            {
                                codesForTag.Add(code);
                            }
                            else
                            {
                                MovieCodesByTag.Add(tag, new HashSet<string>() { code });
                            }
                        }

                    }
                }
            }
            Console.WriteLine("Добавление тэгов к фильмам завершено " + StopWatch.Elapsed);
            Console.WriteLine();


            Console.WriteLine("movie: title  -  найти фильм по названию");
            Console.WriteLine("actor: name  -  найти фильмы с участием данного актера");
            Console.WriteLine("tag: title  -  найти фильмы по тегу");
            Console.WriteLine();
            Console.WriteLine("Введите поисковой запрос:");
            string SearchedLine = Console.ReadLine();
            while (SearchedLine != null)
            {
                if (SearchedLine.Contains(":"))
                {
                    string type = SearchedLine.Substring(0, SearchedLine.IndexOf(":"));
                    if (type == "movie")
                    {
                        string title = SearchedLine.Substring(SearchedLine.IndexOf(":") + 2);
                        if (MovieCodeByTitle.TryGetValue(title, out string code))
                        {
                            Console.WriteLine();
                            MovieByCode[code].Print(ActorByCode);
                            Console.WriteLine("Введите поисковой запрос:");
                            SearchedLine = Console.ReadLine();
                        }
                        else
                        {
                            Console.WriteLine("Такой фильм не найден. Попробуйте ещё раз:");
                            SearchedLine = Console.ReadLine();
                        }
                    }
                    else if (type == "actor")
                    {
                        string name = SearchedLine.Substring(SearchedLine.IndexOf(":") + 2);
                        if (ActorCodesByName.TryGetValue(name, out HashSet<string> codes))
                        {
                            if (codes.Count > 1)
                            {
                                Console.WriteLine();
                                Console.WriteLine("По вашему запросу найдено несколько актёров");
                            }
                            Console.WriteLine();
                            foreach (string code in codes)
                            {
                                ActorByCode[code].Print(MovieByCode);
                            }
                            Console.WriteLine("Введите поисковой запрос:");
                            SearchedLine = Console.ReadLine();
                        }
                        else
                        {
                            Console.WriteLine("Такой актёр не найден. Попробуйте ещё раз:");
                            SearchedLine = Console.ReadLine();
                        }
                    }
                    else if (type == "tag")
                    {
                        string title = SearchedLine.Substring(SearchedLine.IndexOf(":") + 2);
                        int p = 0;
                        if (MovieCodesByTag.TryGetValue(title, out HashSet<string> codes))
                        {
                            Console.WriteLine();
                            foreach (string code in codes)
                            {
                                if (p < 100)
                                {
                                    Console.WriteLine(MovieByCode[code].Titles.First());
                                    p++;
                                    /*foreach (string movieTitle in MovieByCode[code].Titles)
                                    {
                                        if (p < 100)
                                        {
                                            Console.Write("| " + movieTitle + " ");
                                            p++;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Вывести ещё фильмы да/нет: ");
                                            string answer = Console.ReadLine();
                                            if (answer == "да") { p = 0; continue; }
                                            else { break; }
                                        }
                                    }*/
                                }
                                else
                                {
                                    Console.WriteLine("Вывести ещё фильмы да/нет: ");
                                    string answer = Console.ReadLine();
                                    if (answer == "да") { p = 0; continue; }
                                    else { break; }
                                }
                            }
                            Console.WriteLine();
                            Console.WriteLine("Введите поисковой запрос:");
                            SearchedLine = Console.ReadLine();
                        }
                        else
                        {
                            Console.WriteLine("\nТакой тэг не найден. Попробуйте ещё раз:");
                            SearchedLine = Console.ReadLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nНеизвестная команда. Попробуйте ещё раз:");
                        SearchedLine = Console.ReadLine();
                    }
                }
                else
                {
                    Console.WriteLine("\nНеизвестная строка. Попробуйте ещё раз:");
                    SearchedLine = Console.ReadLine();
                }
            }



            //Console.WriteLine("\nКакой фильм вы хотите найти: "); //"The Shawshank Redemption"

            while (SearchedLine != null)
            {
                if (!MovieCodeByTitle.TryGetValue(SearchedLine, out string code))
                {
                    Console.WriteLine("Такой фильм не найден. Попробуйте ещё раз: ");
                    SearchedLine = Console.ReadLine();
                }
                else
                {
                    //try
                    //{
                    Movie movie = MovieByCode[MovieCodeByTitle[SearchedLine]];
                    Console.Write(movie.Code + ' ');
                    int q = 0;
                    foreach (string title in movie.Titles)
                    {
                        if (q > 0) { Console.Write("|"); }
                        Console.Write(title);
                        q++;
                    }
                    Console.WriteLine("    " + movie.Rating);
                    if (movie.Director != "")
                    {
                        Console.WriteLine($"Режиссер: {ActorByCode[movie.Director].Name}");
                    }
                    Console.Write("\nАктеры:");
                    int p = 0;
                    foreach (string actorCode in movie.CodsOfActors)
                    {
                        if (p > 0) Console.Write(",");
                        Console.Write(" " + ActorByCode[actorCode].Name);
                        p++;
                    }
                    Console.WriteLine();
                    Console.Write("Теги: |");
                    foreach (string tag in movie.Tags)
                    {
                        Console.Write(" " + tag + " |");
                    }
                    Console.WriteLine("\nКакой фильм вы хотите найти: ");
                    SearchedLine = Console.ReadLine();
                }
                //
                //catch { }

            }
            Console.WriteLine();
            //Console.WriteLine(ActorByName["Marlene Dietrich"]);
            //Console.WriteLine(MoviesByTag[""]);
            Console.WriteLine("Нажмите клавишу:");
            Console.ReadKey();






            int k = 1;
            foreach (var code in CodesByCode2)
            {
                Console.WriteLine(Convert.ToString(k) + ") " + code.Key + " " + code.Value.Count);
                k++;
                if (k >= 100) { break; }
            }
            Console.ReadKey();








            

            /*int k = 1;
            foreach (var movie in MovieByCode)
            {
                Console.WriteLine(Convert.ToString(k) + ") " + movie.Key + " " + movie.Value.CodsOfActors.Count());
                k++;
            }
            Console.ReadKey();*/
            /*string GetSubstring(int n, string s, int[] index)
            {
                if (n == 0) return s.Substring(0, index[0]);
                if (n > 0) return s.Substring(index[n - 1] + 1, index[n] - index[n - 1] - 1);
                throw new Exception("Неверные входные данные");
            }*/
        }
    }
    class MyEqualityComparerByCode : IEqualityComparer<string[]>
    {
        public bool Equals(string[] x, string[] y)
        {
            return x[0] == y[0];
        }

        public int GetHashCode(string[] x)
        {
            return x[0].GetHashCode();
        }
    }
}

