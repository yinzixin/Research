using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using Biz.Web;
namespace HelloWorld.Models
{ 
    public class Movie
    {
         
        public int ID { get; set; } 
        [StringLength(10,MinimumLength=2,ErrorMessage="必须是2~10个字符长"),Required,Display(Name="名称")]
        public string Title { get; set; }
        [Display(Name="发布日期"),Date(MaxDate="2012-01-01",ErrorMessage="2012地球灭亡啦")]
        public DateTime ReleaseDate { get; set; }
        public string Genre { get; set; }
        [Range(1,100,ErrorMessage="必须是1~100")]
        public decimal Price { get; set; }
        
        public string Rating { get; set; }
    }


    public class MovieDBContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
    }
}