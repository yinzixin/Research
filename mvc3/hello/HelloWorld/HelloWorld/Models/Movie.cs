using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
namespace HelloWorld.Models
{
    //movie model test 
    public class Movie
    {
        public int ID { get; set; }
        [Display(Name="标题")]
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
    }


    public class MovieDBContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
    }
}