﻿using Microsoft.EntityFrameworkCore;

namespace apiservice;

public class BloggingContext : DbContext
{
    public BloggingContext(DbContextOptions<BloggingContext> options)
    : base(options)
    {
    }

    public DbSet<Blog> Blogs { get; set; }
}