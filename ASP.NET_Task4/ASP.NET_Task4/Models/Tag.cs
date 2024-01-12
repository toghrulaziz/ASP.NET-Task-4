﻿namespace ASP.NET_Task4.Models
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = null!;
        public List<Product>? Products { get; set; }
    }
}
