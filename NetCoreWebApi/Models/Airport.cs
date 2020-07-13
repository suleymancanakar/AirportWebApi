
using AirportWebApi.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace AirportWebApi.Models
{
    public class Airport : IEntity
    {
        [Key]
        [Required]
        public string IataCode { get; set; }
        [Required]
        public Guid Id { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
    }
}
