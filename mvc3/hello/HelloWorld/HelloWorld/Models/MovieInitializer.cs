using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace HelloWorld.Models
{
    public class MovieInitializer :  DropCreateDatabaseIfModelChanges<MovieDBContext>
    {
        protected override void Seed(MovieDBContext context)
        {
            var movies = new List<Movie> {   
                 new Movie { Title = "When Harry Met Sally",   
                             ReleaseDate=DateTime.Parse("1989-1-11"),   
                             Genre="Romantic Comedy",  
                             Rating="R",  
                             Price=7.99M},  

                     new Movie { Title = "Ghostbusters ",   
                             ReleaseDate=DateTime.Parse("1984-3-13"),   
                             Genre="Comedy",  
                             Rating="R",  
                             Price=8.99M},  
             };
            movies.ForEach(d => context.Movies.Add(d));
        }
    }
}